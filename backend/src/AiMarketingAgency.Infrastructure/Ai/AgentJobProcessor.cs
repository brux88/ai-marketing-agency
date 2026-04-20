using System.Text.Json;
using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Application.Common;
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
    private readonly INotificationService _notificationService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITelegramBotService _telegramBot;
    private readonly ILogger<AgentJobProcessor> _logger;

    public AgentJobProcessor(
        IAppDbContext context,
        ILlmKernelFactory kernelFactory,
        IEnumerable<IMarketingAgent> agents,
        IImageGenerationServiceFactory imageGenerationServiceFactory,
        IVideoGenerationServiceFactory videoGenerationServiceFactory,
        IImageOverlayService overlayService,
        ILlmKeyVault keyVault,
        INotificationService notificationService,
        IEmailNotificationService emailNotificationService,
        IPushNotificationService pushNotificationService,
        ITelegramBotService telegramBot,
        ILogger<AgentJobProcessor> logger)
    {
        _context = context;
        _kernelFactory = kernelFactory;
        _agents = agents;
        _imageGenerationServiceFactory = imageGenerationServiceFactory;
        _videoGenerationServiceFactory = videoGenerationServiceFactory;
        _overlayService = overlayService;
        _keyVault = keyVault;
        _notificationService = notificationService;
        _emailNotificationService = emailNotificationService;
        _pushNotificationService = pushNotificationService;
        _telegramBot = telegramBot;
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

            var sourcesQuery = _context.ContentSources
                .Where(s => s.AgencyId == job.AgencyId && s.IsActive);

            if (job.ProjectId.HasValue)
                sourcesQuery = sourcesQuery.Where(s => s.ProjectId == job.ProjectId.Value || s.ProjectId == null);

            var sources = await sourcesQuery.ToListAsync(ct);

            var agency = job.Agency;

            // Load project if set
            Project? project = null;
            if (job.ProjectId.HasValue)
            {
                project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == job.ProjectId.Value && p.IsActive, ct);
            }

            // Load schedule if job was triggered by one
            ContentSchedule? schedule = null;
            if (job.ScheduleId.HasValue)
            {
                schedule = await _context.ContentSchedules
                    .FirstOrDefaultAsync(s => s.Id == job.ScheduleId.Value, ct);
            }

            // Load recent content to avoid repetition
            var recentQuery = _context.GeneratedContents
                .Where(c => c.AgencyId == job.AgencyId)
                .AsNoTracking();
            if (job.ProjectId.HasValue)
                recentQuery = recentQuery.Where(c => c.ProjectId == job.ProjectId.Value);
            // Filter by agent type to get relevant content history
            var agentContentType = job.AgentType switch
            {
                AgentType.SocialManager => ContentType.SocialPost,
                AgentType.ContentWriter => ContentType.BlogPost,
                AgentType.Newsletter => ContentType.Newsletter,
                _ => (ContentType?)null
            };
            if (agentContentType.HasValue)
                recentQuery = recentQuery.Where(c => c.ContentType == agentContentType.Value);
            var recentContents = await recentQuery
                .OrderByDescending(c => c.CreatedAt)
                .Take(15)
                .Select(c => new Application.Agents.RecentContentSummary(
                    c.Title,
                    c.Body.Length > 200 ? c.Body.Substring(0, 200) : c.Body,
                    c.ContentType,
                    c.CreatedAt))
                .ToListAsync(ct);

            // Load project documents for RAG context
            List<Application.Agents.ProjectDocumentSummary>? documents = null;
            if (job.ProjectId.HasValue)
            {
                documents = await _context.ProjectDocuments
                    .AsNoTracking()
                    .Where(d => d.ProjectId == job.ProjectId.Value && d.IsActive
                                && d.ExtractedText != null && d.ExtractedText != "")
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(20)
                    .Select(d => new Application.Agents.ProjectDocumentSummary(d.Name, d.ExtractedText!))
                    .ToListAsync(ct);
            }

            var jobContext = new AgentJobContext(
                Kernel: kernel,
                Agency: agency,
                Input: job.Input,
                Sources: sources,
                Project: project,
                Schedule: schedule,
                RecentContents: recentContents,
                Documents: documents);

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
            var imageProviderType = agency.ImageLlmProviderKey?.ProviderType;
            var textProviderType = agency.DefaultLlmProviderKey?.ProviderType;
            foreach (var content in result.Contents)
            {
                string? imageUrl = content.ImageUrl;
                string? originalImageUrl = null;
                string? imagePrompt = content.ImagePrompt;
                List<string>? imageUrls = null;
                decimal? imageCost = null;
                decimal? textCost = EstimateTextCost(textProviderType, content.Title.Length + content.Body.Length);

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

                            // Apply logo overlay if configured (project overrides agency when set)
                            var overlayEnabled = project?.EnableLogoOverlay ?? agency.EnableLogoOverlay;
                            var overlayLogoUrl = !string.IsNullOrWhiteSpace(project?.LogoUrl)
                                ? project!.LogoUrl
                                : agency.LogoUrl;
                            var overlayPosition = project?.LogoOverlayPosition ?? agency.LogoOverlayPosition;
                            var overlayMode = project?.LogoOverlayMode ?? agency.LogoOverlayMode;
                            var bannerColor = project?.BrandBannerColor ?? agency.BrandBannerColor;
                            if (overlayEnabled && !string.IsNullOrWhiteSpace(overlayLogoUrl))
                            {
                                try
                                {
                                    if (overlayMode == 1 && !string.IsNullOrWhiteSpace(bannerColor))
                                    {
                                        finalUrl = await _overlayService.ApplyBrandBannerAsync(
                                            imageResult.ImageUrl,
                                            overlayLogoUrl!,
                                            bannerColor!,
                                            ct);
                                    }
                                    else
                                    {
                                        finalUrl = await _overlayService.ApplyLogoOverlayAsync(
                                            imageResult.ImageUrl,
                                            overlayLogoUrl!,
                                            (LogoPosition)overlayPosition,
                                            ct);
                                    }
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
                                originalImageUrl = imageResult.ImageUrl;
                                imagePrompt = imageResult.RevisedPrompt;
                            }
                        }

                        imageCost = EstimateImageCost(imageProviderType) * count;

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
                    OriginalImageUrl = originalImageUrl,
                    ImagePrompt = imagePrompt,
                    ImageUrls = imageUrls != null ? JsonSerializer.Serialize(imageUrls) : null,
                    VideoUrl = videoUrl,
                    VideoPrompt = videoPrompt,
                    VideoDurationSeconds = videoDuration,
                    Status = DetermineContentStatus(agency, project, schedule, content.OverallScore),
                    AutoApproved = ShouldAutoApprove(agency, project, schedule, content.OverallScore),
                    ApprovedAt = ShouldAutoApprove(agency, project, schedule, content.OverallScore) ? DateTime.UtcNow : null,
                    AiGenerationCostUsd = textCost,
                    AiImageCostUsd = imageCost,
                };

                _context.GeneratedContents.Add(generatedContent);
            }

            await _context.SaveChangesAsync(ct);

            // Auto-schedule approved social posts when a connector exists for the detected platform.
            var savedContents = await _context.GeneratedContents
                .Where(c => c.JobId == job.Id && c.Status == ContentStatus.Approved)
                .ToListAsync(ct);
            foreach (var approved in savedContents)
            {
                try
                {
                    await CalendarAutoScheduler.TryScheduleAsync(_context, approved, _logger, ct, schedule);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Auto-schedule failed for content {ContentId}", approved.Id);
                }
            }

            // Update job
            job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
            job.Output = result.Output;
            job.CompletedAt = DateTime.UtcNow;

            if (!result.Success)
                job.ErrorMessage = result.Output;

            var agentLabel = job.AgentType switch
            {
                AgentType.SocialManager => "Social Manager",
                AgentType.ContentWriter => "Blog Writer",
                AgentType.Newsletter => "Newsletter",
                _ => job.AgentType.ToString()
            };
            _context.Notifications.Add(new Notification
            {
                TenantId = job.TenantId,
                AgencyId = job.AgencyId,
                JobId = job.Id,
                ProjectId = job.ProjectId,
                Type = result.Success ? "job.completed" : "job.failed",
                Title = result.Success
                    ? $"{agentLabel} completato ({result.Contents.Count} contenuti)"
                    : $"{agentLabel} fallito",
                Body = result.Success ? result.Output : (job.ErrorMessage ?? "Errore sconosciuto"),
                Link = "/jobs",
                Read = false
            });

            await _context.SaveChangesAsync(ct);

            try
            {
                var createdContents = await _context.GeneratedContents
                    .Where(c => c.JobId == job.Id)
                    .ToListAsync(ct);
                foreach (var c in createdContents)
                {
                    await _notificationService.NotifyContentGenerated(
                        job.TenantId, job.AgencyId, c.Id, c.Title, c.Status.ToString());
                }
                await _notificationService.NotifyJobStatusChanged(
                    job.TenantId, job.AgencyId, job.Id, job.Status.ToString(), job.AgentType.ToString());

                // Telegram notifications — send each content with photo + inline buttons
                if (result.Success && createdContents.Count > 0)
                {
                    foreach (var c in createdContents)
                    {
                        var typeEmoji = c.ContentType switch
                        {
                            ContentType.SocialPost => "📱 Social",
                            ContentType.BlogPost => "📝 Blog",
                            ContentType.Newsletter => "📧 Newsletter",
                            _ => "📄 Contenuto"
                        };
                        var caption = $"{typeEmoji} | <b>{c.Title}</b>\n\n{Truncate(c.Body, 800)}\n\n<i>Stato: {c.Status}</i>";
                        var buttons = new List<TelegramInlineButton>();
                        if (c.Status == ContentStatus.InReview)
                        {
                            buttons.Add(new TelegramInlineButton("✅ Approva", $"approve_{c.Id}"));
                            buttons.Add(new TelegramInlineButton("❌ Rifiuta", $"reject_{c.Id}"));
                        }
                        await _telegramBot.NotifyAgencyWithContentAsync(
                            job.AgencyId, job.ProjectId, caption, c.ImageUrl,
                            buttons.Count > 0 ? buttons : null, ct);
                    }
                }

                // Email notifications for generated content
                if (result.Success && createdContents.Count > 0)
                {
                    var notifyProject = job.ProjectId.HasValue
                        ? await _context.Projects.AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == job.ProjectId.Value, ct)
                        : null;

                    if (notifyProject?.NotifyEmailOnGeneration == true && !string.IsNullOrWhiteSpace(notifyProject.NotificationEmail))
                    {
                        var titles = string.Join(", ", createdContents.Select(c => c.Title));
                        var subject = $"Nuovi contenuti generati - {notifyProject.Name}";
                        var htmlBody = $"""
                            <h2>Contenuti generati</h2>
                            <p>Sono stati generati <strong>{createdContents.Count}</strong> nuovi contenuti per il progetto <strong>{notifyProject.Name}</strong>:</p>
                            <ul>
                            {string.Join("", createdContents.Select(c => $"<li><strong>{c.Title}</strong> — Stato: {c.Status}</li>"))}
                            </ul>
                            <p>Accedi alla piattaforma per revisionarli.</p>
                            """;
                        await _emailNotificationService.SendEmailNotificationAsync(
                            job.AgencyId, job.ProjectId, subject, htmlBody, ct);
                    }

                    if (job.ProjectId.HasValue)
                    {
                        var pushTitle = notifyProject != null
                            ? $"Nuovi contenuti · {notifyProject.Name}"
                            : "Nuovi contenuti generati";
                        var pushBody = createdContents.Count == 1
                            ? createdContents[0].Title
                            : $"{createdContents.Count} contenuti pronti per la revisione";
                        await _pushNotificationService.SendToProjectAsync(
                            job.AgencyId,
                            job.ProjectId,
                            PushEventType.ContentGenerated,
                            pushTitle,
                            pushBody,
                            new Dictionary<string, string>
                            {
                                ["agencyId"] = job.AgencyId.ToString(),
                                ["projectId"] = job.ProjectId.Value.ToString(),
                                ["event"] = "content.generated",
                            },
                            ct);
                    }
                }
            }
            catch (Exception notifyEx)
            {
                _logger.LogWarning(notifyEx, "Failed to send notification for job {JobId}", jobId);
            }

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

            _context.Notifications.Add(new Notification
            {
                TenantId = job.TenantId,
                AgencyId = job.AgencyId,
                JobId = job.Id,
                ProjectId = job.ProjectId,
                Type = "job.failed",
                Title = $"{job.AgentType} fallito",
                Body = ex.Message,
                Link = "/jobs",
                Read = false
            });

            await _context.SaveChangesAsync(ct);

            try
            {
                await _notificationService.NotifyJobStatusChanged(
                    job.TenantId, job.AgencyId, job.Id, job.Status.ToString(), job.AgentType.ToString());
            }
            catch (Exception notifyEx)
            {
                _logger.LogWarning(notifyEx, "Failed to send SignalR notification for job {JobId}", jobId);
            }

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

    private static bool ShouldAutoApprove(Agency agency, Project? project, ContentSchedule? schedule, decimal overallScore)
    {
        var mode = schedule?.ApprovalMode ?? project?.ApprovalMode ?? agency.ApprovalMode;
        var minScore = schedule?.AutoApproveMinScore ?? project?.AutoApproveMinScore ?? agency.AutoApproveMinScore;
        return mode switch
        {
            ApprovalMode.AutoApprove => true,
            ApprovalMode.AutoApproveAboveScore => overallScore >= minScore,
            ApprovalMode.Manual => false,
            _ => false
        };
    }

    private static ContentStatus DetermineContentStatus(Agency agency, Project? project, ContentSchedule? schedule, decimal overallScore)
    {
        if (ShouldAutoApprove(agency, project, schedule, overallScore))
            return ContentStatus.Approved;

        return ContentStatus.InReview;
    }

    // Best-effort cost estimates (USD). Replace with real billing when provider SDKs expose usage.
    private static decimal EstimateImageCost(LlmProviderType? provider)
    {
        return provider switch
        {
            LlmProviderType.NanoBanana => 0.039m,
            LlmProviderType.OpenAI => 0.040m,
            LlmProviderType.AzureOpenAI => 0.040m,
            LlmProviderType.Custom => 0.020m,
            _ => 0.030m
        };
    }

    private static decimal EstimateTextCost(LlmProviderType? provider, int outputChars)
    {
        // Rough: 4 chars ≈ 1 token; assume similar input tokens; price per 1K tokens varies by provider.
        var tokens = Math.Max(1, outputChars / 4) * 2m;
        var pricePerK = provider switch
        {
            LlmProviderType.OpenAI => 0.01m,       // gpt-4o input+output average
            LlmProviderType.AzureOpenAI => 0.01m,
            LlmProviderType.Anthropic => 0.015m,   // Claude average
            LlmProviderType.Custom => 0.002m,
            _ => 0.005m
        };
        return Math.Round(tokens / 1000m * pricePerK, 6);
    }

    private static string Truncate(string? text, int maxLen)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLen ? text : text[..maxLen] + "…";
    }
}
