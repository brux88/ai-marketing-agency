using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai.Agents;

public class SeoOptimizerAgent : IMarketingAgent
{
    private readonly ILogger<SeoOptimizerAgent> _logger;

    public SeoOptimizerAgent(ILogger<SeoOptimizerAgent> logger)
    {
        _logger = logger;
    }

    public AgentType Type => AgentType.SeoOptimizer;

    public async Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct)
    {
        var chatCompletion = context.Kernel.GetRequiredService<IChatCompletionService>();
        var agency = context.Agency;
        var brandVoice = agency.BrandVoice;

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var prompt = $"""
            You are an expert SEO analyst and optimizer for "{agency.ProductName}".

            BRAND:
            - Product: {agency.ProductName}
            - Description: {agency.Description ?? "N/A"}
            - Website: {agency.WebsiteUrl ?? "N/A"}
            - Keywords focus: {string.Join(", ", brandVoice.Keywords)}
            - Language: {brandVoice.Language}

            TARGET AUDIENCE:
            - {agency.TargetAudience.Description}
            - Interests: {string.Join(", ", agency.TargetAudience.Interests)}

            CONTENT SOURCES:
            {sourcesContext}

            TASK: {context.Input ?? "Perform a comprehensive SEO audit and optimization report."}

            Provide a detailed SEO report including:

            1. **KEYWORD ANALYSIS**
               - Primary keywords (5-10) with estimated search volume (High/Medium/Low)
               - Long-tail keyword opportunities (10-15)
               - Semantic keyword clusters
               - Keyword difficulty assessment

            2. **CONTENT OPTIMIZATION RECOMMENDATIONS**
               - Title tag templates
               - Meta description templates
               - H1/H2 heading structure recommendations
               - Internal linking strategy
               - Content gaps vs. competitors

            3. **TECHNICAL SEO CHECKLIST**
               - Schema markup recommendations (FAQ, Article, Product, Organization)
               - Open Graph and Twitter Card templates
               - Canonical URL strategy
               - Site structure recommendations

            4. **CONTENT CALENDAR SEO ALIGNMENT**
               - Priority topics based on keyword opportunity
               - Seasonal keyword trends
               - Content update schedule for existing content

            5. **COMPETITOR ANALYSIS**
               - Key competitive keywords
               - Content format gaps
               - Link building opportunities

            Write in {brandVoice.Language} language.

            TITLE: Report SEO - {agency.ProductName}
            ---
            """;

        _logger.LogInformation("Running SEO analysis for agency {AgencyId}", agency.Id);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);
        var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var text = response.Content ?? string.Empty;

        var title = $"Analisi SEO - {DateTime.UtcNow:yyyy-MM-dd}";
        foreach (var line in text.Split('\n'))
        {
            if (line.Trim().StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                title = line.Trim()["TITLE:".Length..].Trim();
                break;
            }
        }

        // Score
        var scorePrompt = $"""
            Score this SEO report on a scale of 1-10:

            {text[..Math.Min(text.Length, 3000)]}

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
        var scoreText = scoreResponse.Content ?? "";

        return new AgentJobResult(
            Success: true,
            Output: $"Generated SEO report: {title}",
            Contents: [new GeneratedContentResult(
                Title: title,
                Body: text,
                Type: ContentType.Report,
                QualityScore: ExtractScore(scoreText, "QUALITY"),
                RelevanceScore: ExtractScore(scoreText, "RELEVANCE"),
                SeoScore: ExtractScore(scoreText, "SEO"),
                BrandVoiceScore: ExtractScore(scoreText, "BRAND_VOICE"),
                OverallScore: ExtractScore(scoreText, "OVERALL"),
                ScoreExplanation: ExtractExplanation(scoreText))]);
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
