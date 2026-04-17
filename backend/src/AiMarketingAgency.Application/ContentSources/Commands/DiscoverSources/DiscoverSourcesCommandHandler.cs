using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Application.ContentSources.Commands.DiscoverSources;

public class DiscoverSourcesCommandHandler : IRequestHandler<DiscoverSourcesCommand, List<SuggestedSource>>
{
    private readonly IAppDbContext _context;
    private readonly ILlmKernelFactory _kernelFactory;
    private readonly ILogger<DiscoverSourcesCommandHandler> _logger;

    public DiscoverSourcesCommandHandler(
        IAppDbContext context,
        ILlmKernelFactory kernelFactory,
        ILogger<DiscoverSourcesCommandHandler> logger)
    {
        _context = context;
        _kernelFactory = kernelFactory;
        _logger = logger;
    }

    public async Task<List<SuggestedSource>> Handle(DiscoverSourcesCommand request, CancellationToken ct)
    {
        // Load agency for fallback brand voice
        var agency = await _context.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, ct)
            ?? throw new KeyNotFoundException("Agency not found.");

        // Load project if specified
        Domain.Entities.Project? project = null;
        if (request.ProjectId.HasValue)
        {
            project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId.Value && p.AgencyId == request.AgencyId && p.IsActive, ct);
        }

        // Determine brand context from project (preferred) or agency (fallback)
        var brandVoice = project?.BrandVoice ?? agency.BrandVoice;
        var targetAudience = project?.TargetAudience ?? agency.TargetAudience;
        var extractedContext = project?.ExtractedContext ?? agency.Description ?? "";
        var websiteUrl = project?.WebsiteUrl ?? agency.WebsiteUrl ?? "";
        var keywords = brandVoice.Keywords.Count > 0
            ? string.Join(", ", brandVoice.Keywords)
            : "";
        var audience = targetAudience.Description ?? "";

        // Load existing content sources to exclude
        var existingSourcesQuery = _context.ContentSources
            .AsNoTracking()
            .Where(cs => cs.AgencyId == request.AgencyId && cs.IsActive);

        if (request.ProjectId.HasValue)
            existingSourcesQuery = existingSourcesQuery.Where(cs => cs.ProjectId == request.ProjectId.Value);

        var existingUrls = await existingSourcesQuery
            .Select(cs => cs.Url)
            .ToListAsync(ct);

        var existingUrlsText = existingUrls.Count > 0
            ? string.Join("\n", existingUrls)
            : "(none)";

        // Build prompt
        var prompt = $"""
            You are a content marketing expert. Based on the following brand/product context, suggest 8-10 highly relevant content sources (blogs, news sites, RSS feeds, industry portals) that would be valuable for content creation and inspiration.

            Brand/Product: {extractedContext}
            Keywords: {keywords}
            Target Audience: {audience}
            Website: {websiteUrl}

            Already existing sources (DO NOT suggest these):
            {existingUrlsText}

            Return a JSON array with objects containing:
            - "url": the full URL of the source
            - "name": short name of the source
            - "description": 1-line description of why it's relevant (in Italian)
            - "type": 1 for RSS feeds, 2 for websites

            Return ONLY the JSON array, no markdown, no explanation.
            """;

        // Call LLM
        var kernel = await _kernelFactory.CreateKernelAsync(request.AgencyId, ct);
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddUserMessage(prompt);
        var response = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        var text = response.Content?.Trim() ?? "";

        // Parse JSON response
        var jsonStart = text.IndexOf('[');
        var jsonEnd = text.LastIndexOf(']');
        if (jsonStart < 0 || jsonEnd <= jsonStart)
        {
            _logger.LogWarning("LLM did not return valid JSON array for source discovery: {Text}", text);
            throw new InvalidOperationException("LLM did not return valid JSON.");
        }

        var json = text.Substring(jsonStart, jsonEnd - jsonStart + 1);

        List<SuggestedSourceDto>? suggestions;
        try
        {
            suggestions = JsonSerializer.Deserialize<List<SuggestedSourceDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse source suggestions JSON: {Json}", json);
            throw new InvalidOperationException("Failed to parse source suggestions response.");
        }

        if (suggestions == null || suggestions.Count == 0)
            return new List<SuggestedSource>();

        return suggestions
            .Select(s => new SuggestedSource(
                s.Url ?? "",
                s.Name ?? "",
                s.Description ?? "",
                s.Type))
            .Where(s => !string.IsNullOrWhiteSpace(s.Url))
            .ToList();
    }

    private class SuggestedSourceDto
    {
        public string? Url { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
    }
}
