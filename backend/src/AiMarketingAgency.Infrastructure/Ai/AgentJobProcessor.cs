using System.Text.Json;
using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai;

public class AgentJobProcessor : IAgentJobProcessor
{
    private readonly IAppDbContext _context;
    private readonly ILlmKernelFactory _kernelFactory;
    private readonly IEnumerable<IMarketingAgent> _agents;
    private readonly IImageGenerationServiceFactory _imageGenerationServiceFactory;
    private readonly IVideoGenerationServiceFactory _videoGenerationServiceFactory;
    private readonly IImageOverlayService _overlayService;
    private readonly ILlmKeyVault _keyVault;
    private readonly ILogger<AgentJobProcessor> _logger;

    public AgentJobProcessor(
        IAppDbContext context,
        ILlmKernelFactory kernelFactory,
        IEnumerable<IMarketingAgent> agents,
        IImageGenerationServiceFactory imageGenerationServiceFactory,
        IVideoGenerationServiceFactory videoGenerationServiceFactory,
        IImageOverlayService overlayService,
        ILlmKeyVault keyVault,
        ILogger<AgentJobProcessor> logger)
    {
        _context = context;
        _kernelFactory = kernelFactory;
        _agents = agents;
        _imageGenerationServiceFactory = imageGenerationServiceFactory;
        _videoGenerationServiceFactory = videoGenerationServiceFactory;
        _overlayService = overlayService;
        _keyVault = keyVault;
        _logger = logger;
    }

    public async Task ProcessJobAsync(Guid jobId, CancellationToken ct = default)
    {
        // Load job from DB
        var job = await _context.AgentJobs
            .Include(j => j.Agency)
                .ThenInclude(a => a.DefaultLlmProviderKey)
            .Include(j => j.Agency)
                .ThenInclude(a => a.ImageLlmProviderKey)
            .Include(j => j.Agency)
                .ThenInclude(a => a.VideoLlmProviderKey)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new InvalidOperationException($"Job {jobId} not found.");

        // Mark as running
        job.Status = JobStatus.Running;
        job.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        try
        {
            // Find the correct agent
            var agent = _agents.FirstOrDefault(a => a.Type == job.AgentType)
                ?? throw new InvalidOperationException($"No agent registered for type {job.AgentType}.");

            // Build context
            var kernel = await _kernelFactory.CreateKernelAsync(job.AgencyId, ct);

            var sources = await _context.ContentSources
                .Where(s => s.AgencyId == job.AgencyId && s.IsActive)
                .ToListAsync(ct);

            var agency = job.Agency;

            // Load project if set
            Project? project = null;
            if (job.ProjectId.HasValue)
            {
                project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == job.ProjectId.Value && p.IsActive, ct);
            }

            var jobContext = new AgentJobContext(
                Kernel: kernel,
                Agency: agency,
                Input: job.Input,
                Sources: sources,
                Project: project);

            _logger.LogInformation(
                "Executing agent {AgentType} for job {JobId}, agency {AgencyId}",
                job.AgentType, jobId, job.AgencyId);

            // Execute agent
            var result = await agent.ExecuteAsync(jobContext, ct);

            // Prepare image generation service if configured
            IImageGenerationService? imageService = null;
            if (agency.ImageLlmProviderKeyId != null && agency.ImageLlmProviderKey != null)
            {
                try
                {
                    var imageApiKey = agency.ImageLlmProviderKey.EncryptedApiKey;
                    imageService = _imageGenerationServiceFactory.Create(
                        agency.ImageLlmProviderKey.ProviderType,
                        imageApiKey,
                        agency.ImageLlmProviderKey.EncryptedApiKeySecret,
                        agency.ImageLlmProviderKey.BaseUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to initialize image generation service for agency {AgencyId}", agency.Id);
                }
            }

            // Save generated content
            foreach (var content in result.Contents)
            {
                string? imageUrl = content.ImageUrl;
                string? imagePrompt = content.ImagePrompt;
                List<string>? imageUrls = null;

                // Generate image(s) if service is available and no image was already provided
                if (imageService != null && job.ImageMode != ImageGenerationMode.None && string.IsNullOrEmpty(imageUrl))
                {
                    try
                    {
                        // Step 1: Derive contextual visual concept from the generated body
                        var visualConcept = await BuildContextualVisualPromptAsync(kernel, agency, content, ct);

                        // Step 2: Generate N images
                        var count = job.ImageMode == ImageGenerationMode.Carousel
                            ? Math.Clamp(job.ImageCount, 2, 10)
                            : 1;

                        var generated = new List<string>();
                        for (int i = 0; i < count; i++)
                        {
                            var slidePrompt = count > 1
                                ? $"{visualConcept}. Carousel slide {i + 1}/{count}, cohesive series, consistent style/palette/composition, variation {i + 1}"
                                : visualConcept;

                            var imageResult = await imageService.GenerateImageAsync(
                                slidePrompt,
                                new ImageGenerationOptions(),
                                ct);

                            var finalUrl = imageResult.ImageUrl;

                            // Apply logo overlay if configured
                            if (agency.EnableLogoOverlay && !string.IsNullOrWhiteSpace(agency.LogoUrl))
                            {
                                try
                                {
                                    finalUrl = await _overlayService.ApplyLogoOverlayAsync(
                                        imageResult.ImageUrl,
                                        agency.LogoUrl,
                                        (LogoPosition)agency.LogoOverlayPosition,
                                        ct);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to apply logo overlay for content '{Title}'", content.Title);
                                }
                            }

                            generated.Add(finalUrl);
                            if (i == 0)
                            {
                                imageUrl = finalUrl;
                                imagePrompt = imageResult.RevisedPrompt;
                            }
                        }

                        if (count > 1)
                            imageUrls = generated;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate image for content '{Title}'", content.Title);
                    }
                }

                // Generate video if service is configured
                string? videoUrl = null;
                string? videoPrompt = null;
                int? videoDuration = null;

                if (agency.VideoLlmProviderKeyId != null && agency.VideoLlmProviderKey != null)
                {
                    try
                    {
                        var videoKey = agency.VideoLlmProviderKey;
                        var videoService = _videoGenerationServiceFactory.Create(
                            videoKey.ProviderType,
                            videoKey.EncryptedApiKey,
                            videoKey.EncryptedApiKeySecret,
                            videoKey.BaseUrl);

                        var vPrompt = $"Create a short marketing video for: {content.Title}. {content.Body[..Math.Min(200, content.Body.Length)]}";
                        var videoResult = await videoService.GenerateVideoAsync(
                            vPrompt,
                            new VideoGenerationOptions(),
                            ct);
                        videoUrl = videoResult.VideoUrl;
                        videoPrompt = videoResult.RevisedPrompt;
                        videoDuration = videoResult.DurationSeconds;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate video for content '{Title}'", content.Title);
                    }
                }

                var generatedContent = new GeneratedContent
                {
                    AgencyId = job.AgencyId,
                    TenantId = job.TenantId,
                    JobId = job.Id,
                    ProjectId = job.ProjectId,
                    ContentType = content.Type,
                    Title = content.Title,
                    Body = content.Body,
                    QualityScore = content.QualityScore,
                    RelevanceScore = content.RelevanceScore,
                    SeoScore = content.SeoScore,
                    BrandVoiceScore = content.BrandVoiceScore,
                    OverallScore = content.OverallScore,
                    ScoreExplanation = content.ScoreExplanation,
                    ImageUrl = imageUrl,
                    ImagePrompt = imagePrompt,
                    ImageUrls = imageUrls != null ? JsonSerializer.Serialize(imageUrls) : null,
                    VideoUrl = videoUrl,
                    VideoPrompt = videoPrompt,
                    VideoDurationSeconds = videoDuration,
                    Status = DetermineContentStatus(agency, content.OverallScore),
                    AutoApproved = ShouldAutoApprove(agency, content.OverallScore),
                    ApprovedAt = ShouldAutoApprove(agency, content.OverallScore) ? DateTime.UtcNow : null,
                };

                _context.GeneratedContents.Add(generatedContent);
            }

            // Update job
            job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
            job.Output = result.Output;
            job.CompletedAt = DateTime.UtcNow;

            if (!result.Success)
                job.ErrorMessage = result.Output;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Job {JobId} completed with {ContentCount} content items, status: {Status}",
                jobId, result.Contents.Count, job.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed with error", jobId);

            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            job.RetryCount++;

            await _context.SaveChangesAsync(ct);
            throw;
        }
    }

    private async Task<string> BuildContextualVisualPromptAsync(
        Kernel kernel, Agency agency, GeneratedContentResult content, CancellationToken ct)
    {
        try
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var brand = agency.BrandVoice;
            var bodyExcerpt = content.Body.Length > 1500 ? content.Body[..1500] : content.Body;

            var prompt = $"""
                You convert blog content into a visual concept for an image generator.
                Extract from the article below the main subject, setting, mood and key visual elements.
                Then write ONE single rich visual prompt (max 60 words) in English that an image generator
                can use to create an image that is specifically and clearly about the article's topic.

                Rules:
                - Mention the concrete subject(s), not abstract metaphors
                - Reflect the article's tone/emotion
                - Include lighting, composition and style hints
                - Style must match brand: tone={brand.Tone}, style={brand.Style}
                - Do NOT include any text or typography in the image
                - Product/brand name: {agency.ProductName}

                ARTICLE TITLE: {content.Title}
                ARTICLE BODY:
                {bodyExcerpt}

                Return ONLY the visual prompt, no labels, no quotes, no explanations.
                """;

            var history = new ChatHistory();
            history.AddUserMessage(prompt);
            var response = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
            var text = response.Content?.Trim().Trim('"', '\'') ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return $"Professional editorial photograph about \"{content.Title}\", {brand.Style} style, cinematic lighting, high detail";
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to build contextual visual prompt for '{Title}', falling back", content.Title);
            return $"Professional editorial photograph about \"{content.Title}\", cinematic lighting, high detail";
        }
    }

    private static bool ShouldAutoApprove(Agency agency, decimal overallScore)
    {
        return agency.ApprovalMode switch
        {
            ApprovalMode.AutoApprove => true,
            ApprovalMode.AutoApproveAboveScore => overallScore >= agency.AutoApproveMinScore,
            ApprovalMode.Manual => false,
            _ => false
        };
    }

    private static ContentStatus DetermineContentStatus(Agency agency, decimal overallScore)
    {
        if (ShouldAutoApprove(agency, overallScore))
            return ContentStatus.Approved;

        return ContentStatus.InReview;
    }
}
