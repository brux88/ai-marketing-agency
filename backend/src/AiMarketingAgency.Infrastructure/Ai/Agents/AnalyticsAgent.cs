using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai.Agents;

public class AnalyticsAgent : IMarketingAgent
{
    private readonly ILogger<AnalyticsAgent> _logger;

    public AnalyticsAgent(ILogger<AnalyticsAgent> logger)
    {
        _logger = logger;
    }

    public AgentType Type => AgentType.Analytics;

    public async Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct)
    {
        var chatCompletion = context.Kernel.GetRequiredService<IChatCompletionService>();
        var agency = context.Agency;
        var brandVoice = agency.BrandVoice;
        var targetAudience = agency.TargetAudience;

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var generatePrompt = $"""
            You are a marketing analytics expert for "{agency.ProductName}".

            BRAND & PRODUCT CONTEXT:
            - Product: {agency.ProductName}
            - Description: {agency.Description ?? "N/A"}
            - Website: {agency.WebsiteUrl ?? "N/A"}
            - Target audience: {targetAudience.Description}
            - Audience interests: {string.Join(", ", targetAudience.Interests)}
            - Audience pain points: {string.Join(", ", targetAudience.PainPoints)}

            CONTENT SOURCES (analyze these):
            {sourcesContext}

            TASK: {context.Input ?? "Generate a comprehensive marketing analytics report with actionable insights and improvement suggestions."}

            Generate a report with the following sections:
            1. EXECUTIVE SUMMARY - Key findings and metrics overview
            2. CONTENT PERFORMANCE ANALYSIS - Evaluate content strategy effectiveness
            3. AUDIENCE INSIGHTS - Audience engagement patterns and preferences
            4. COMPETITIVE LANDSCAPE - Market positioning observations
            5. RECOMMENDATIONS - Specific, actionable improvement suggestions
            6. NEXT STEPS - Priority actions for the next period

            Write in {brandVoice.Language} language.

            FORMAT YOUR RESPONSE AS:
            TITLE: [report title]
            ---
            [full report in markdown]
            """;

        _logger.LogInformation("Generating analytics report for agency {AgencyId}", agency.Id);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(generatePrompt);
        var generatedResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var generatedText = generatedResponse.Content ?? string.Empty;

        var (title, body) = ParseContent(generatedText);

        // Score the report
        var scorePrompt = $"""
            Score this analytics report from 1-10 on each criterion.

            CONTEXT: Marketing analytics report for {agency.ProductName}
            TARGET AUDIENCE: {targetAudience.Description}

            REPORT TITLE: {title}
            REPORT:
            {body}

            Respond ONLY in this exact format:
            QUALITY: [score]
            RELEVANCE: [score]
            SEO: [score]
            BRAND_VOICE: [score]
            OVERALL: [score]
            EXPLANATION: [brief explanation]
            """;

        var scoreHistory = new ChatHistory();
        scoreHistory.AddUserMessage(scorePrompt);
        var scoreResponse = await chatCompletion.GetChatMessageContentAsync(scoreHistory, cancellationToken: ct);
        var scores = ParseScores(scoreResponse.Content ?? string.Empty);

        _logger.LogInformation(
            "Analytics report generated for agency {AgencyId}: overall score {Score}",
            agency.Id, scores.Overall);

        var result = new GeneratedContentResult(
            Title: title,
            Body: body,
            Type: ContentType.Report,
            QualityScore: scores.Quality,
            RelevanceScore: scores.Relevance,
            SeoScore: scores.Seo,
            BrandVoiceScore: scores.BrandVoice,
            OverallScore: scores.Overall,
            ScoreExplanation: scores.Explanation);

        return new AgentJobResult(
            Success: true,
            Output: $"Generated analytics report: {title}",
            Contents: [result]);
    }

    private static (string Title, string Body) ParseContent(string text)
    {
        var title = "Analytics Report";
        var body = text;
        var lines = text.Split('\n', StringSplitOptions.None);
        var bodyStartIndex = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                title = line["TITLE:".Length..].Trim();
                bodyStartIndex = i + 1;
            }
            else if (line == "---")
            {
                bodyStartIndex = i + 1;
                break;
            }
        }

        if (bodyStartIndex > 0 && bodyStartIndex < lines.Length)
            body = string.Join('\n', lines[bodyStartIndex..]).Trim();

        return (title, body);
    }

    private static ContentScores ParseScores(string text)
    {
        return new ContentScores(
            ExtractScore(text, "QUALITY"),
            ExtractScore(text, "RELEVANCE"),
            ExtractScore(text, "SEO"),
            ExtractScore(text, "BRAND_VOICE"),
            ExtractScore(text, "OVERALL"),
            ExtractField(text, "EXPLANATION"));
    }

    private static decimal ExtractScore(string text, string label)
    {
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith($"{label}:", StringComparison.OrdinalIgnoreCase))
            {
                var valueStr = trimmed[$"{label}:".Length..].Trim();
                if (decimal.TryParse(valueStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var score))
                    return Math.Clamp(score, 1m, 10m);
            }
        }
        return 5m;
    }

    private static string ExtractField(string text, string label)
    {
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith($"{label}:", StringComparison.OrdinalIgnoreCase))
                return trimmed[$"{label}:".Length..].Trim();
        }
        return string.Empty;
    }

    private record ContentScores(decimal Quality, decimal Relevance, decimal Seo, decimal BrandVoice, decimal Overall, string Explanation);
}
