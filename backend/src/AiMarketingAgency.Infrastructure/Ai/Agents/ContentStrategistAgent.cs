using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai.Agents;

public class ContentStrategistAgent : IMarketingAgent
{
    private readonly ILogger<ContentStrategistAgent> _logger;

    public ContentStrategistAgent(ILogger<ContentStrategistAgent> logger)
    {
        _logger = logger;
    }

    public AgentType Type => AgentType.ContentStrategist;

    public async Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct)
    {
        var chatCompletion = context.Kernel.GetRequiredService<IChatCompletionService>();
        var agency = context.Agency;
        var brandVoice = agency.BrandVoice;
        var targetAudience = agency.TargetAudience;

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var prompt = $"""
            You are an expert content strategist and editorial planner for "{agency.ProductName}".

            BRAND VOICE:
            - Tone: {brandVoice.Tone}
            - Style: {brandVoice.Style}
            - Keywords: {string.Join(", ", brandVoice.Keywords)}
            - Language: {brandVoice.Language}

            TARGET AUDIENCE:
            - Description: {targetAudience.Description}
            - Age range: {targetAudience.AgeRange ?? "Not specified"}
            - Interests: {string.Join(", ", targetAudience.Interests)}
            - Pain points: {string.Join(", ", targetAudience.PainPoints)}

            CONTENT SOURCES (current industry context):
            {sourcesContext}

            TASK: {context.Input ?? "Create a comprehensive weekly content strategy plan."}

            Create a detailed editorial content plan that includes:

            1. **WEEKLY CONTENT CALENDAR** (7 days)
               For each day, specify:
               - Content type (blog post, social post, newsletter, video script)
               - Topic/title
               - Target platform(s)
               - Key angle/hook
               - Estimated engagement potential (High/Medium/Low)

            2. **CONTENT PILLARS** (3-5 themes)
               - Main theme
               - Sub-topics
               - Why this resonates with the target audience

            3. **TRENDING OPPORTUNITIES**
               - Current trends relevant to the brand
               - Seasonal/timely angles
               - Competitor gaps to exploit

            4. **CONTENT MIX RECOMMENDATION**
               - Percentage split by type (educational, entertaining, promotional, community)
               - Platform priority ranking
               - Optimal posting times

            Write in {brandVoice.Language} language.
            Format your response clearly with headers and bullet points.

            TITLE: Piano Editoriale Settimanale - {agency.ProductName}
            ---
            """;

        _logger.LogInformation("Generating content strategy for agency {AgencyId}", agency.Id);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);
        var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var text = response.Content ?? string.Empty;

        var title = $"Piano Strategico - {DateTime.UtcNow:yyyy-MM-dd}";
        var body = text;

        // Extract title if present
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                title = line.Trim()["TITLE:".Length..].Trim();
                break;
            }
        }

        // Score the strategy
        var scorePrompt = $"""
            Score this content strategy plan on a scale of 1-10:

            {text[..Math.Min(text.Length, 3000)]}

            QUALITY: [1-10 for completeness and actionability]
            RELEVANCE: [1-10 for audience fit]
            SEO: [1-10 for SEO awareness]
            BRAND_VOICE: [1-10 for brand alignment]
            OVERALL: [1-10 overall]
            EXPLANATION: [brief explanation]
            """;

        var scoreHistory = new ChatHistory();
        scoreHistory.AddUserMessage(scorePrompt);
        var scoreResponse = await chatCompletion.GetChatMessageContentAsync(scoreHistory, cancellationToken: ct);
        var scores = ParseScores(scoreResponse.Content ?? "");

        return new AgentJobResult(
            Success: true,
            Output: $"Generated content strategy: {title}",
            Contents: [new GeneratedContentResult(
                Title: title,
                Body: body,
                Type: ContentType.Report,
                QualityScore: scores.Quality,
                RelevanceScore: scores.Relevance,
                SeoScore: scores.Seo,
                BrandVoiceScore: scores.BrandVoice,
                OverallScore: scores.Overall,
                ScoreExplanation: scores.Explanation)]);
    }

    private static (decimal Quality, decimal Relevance, decimal Seo, decimal BrandVoice, decimal Overall, string Explanation) ParseScores(string text)
    {
        return (
            ExtractScore(text, "QUALITY"),
            ExtractScore(text, "RELEVANCE"),
            ExtractScore(text, "SEO"),
            ExtractScore(text, "BRAND_VOICE"),
            ExtractScore(text, "OVERALL"),
            ExtractExplanation(text));
    }

    private static decimal ExtractScore(string text, string label)
    {
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith($"{label}:", StringComparison.OrdinalIgnoreCase))
            {
                var val = trimmed[$"{label}:".Length..].Trim();
                if (decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var score))
                    return Math.Clamp(score, 1m, 10m);
            }
        }
        return 5m;
    }

    private static string ExtractExplanation(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            if (line.Trim().StartsWith("EXPLANATION:", StringComparison.OrdinalIgnoreCase))
                return line.Trim()["EXPLANATION:".Length..].Trim();
        }
        return string.Empty;
    }
}
