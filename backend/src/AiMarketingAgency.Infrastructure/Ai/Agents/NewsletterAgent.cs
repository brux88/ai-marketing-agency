using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai.Agents;

public class NewsletterAgent : IMarketingAgent
{
    private readonly ILogger<NewsletterAgent> _logger;

    public NewsletterAgent(ILogger<NewsletterAgent> logger)
    {
        _logger = logger;
    }

    public AgentType Type => AgentType.Newsletter;

    public async Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct)
    {
        var chatCompletion = context.Kernel.GetRequiredService<IChatCompletionService>();
        var agency = context.Agency;
        var brandVoice = agency.BrandVoice;
        var targetAudience = agency.TargetAudience;

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var generatePrompt = $"""
            You are an expert newsletter curator and writer for "{agency.ProductName}".

            BRAND VOICE:
            - Tone: {brandVoice.Tone}
            - Style: {brandVoice.Style}
            - Keywords to include: {string.Join(", ", brandVoice.Keywords)}
            - Forbidden words: {string.Join(", ", brandVoice.ForbiddenWords)}
            - Language: {brandVoice.Language}

            TARGET AUDIENCE:
            - Description: {targetAudience.Description}
            - Interests: {string.Join(", ", targetAudience.Interests)}
            - Pain points: {string.Join(", ", targetAudience.PainPoints)}

            CONTENT SOURCES (curate from these):
            {sourcesContext}

            TASK: {context.Input ?? "Create a compelling weekly newsletter for our audience."}

            Create a newsletter with the following structure:
            1. SUBJECT LINE - compelling email subject
            2. HEADER - brief intro paragraph
            3. FEATURED STORY - main story with summary (2-3 paragraphs)
            4. INDUSTRY NEWS - 3-4 curated news items with brief commentary
            5. TIPS & INSIGHTS - actionable advice for the audience
            6. CALL TO ACTION - closing with CTA

            Write in {brandVoice.Language} language.

            FORMAT YOUR RESPONSE AS:
            TITLE: [newsletter subject line]
            ---
            [full newsletter body in HTML-friendly markdown]
            """;

        _logger.LogInformation("Generating newsletter for agency {AgencyId}", agency.Id);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(generatePrompt);
        var generatedResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var generatedText = generatedResponse.Content ?? string.Empty;

        var (title, body) = ParseContent(generatedText);

        // Score the newsletter
        var scorePrompt = $"""
            Score this newsletter from 1-10 on each criterion.

            BRAND VOICE: Tone={brandVoice.Tone}, Style={brandVoice.Style}
            TARGET AUDIENCE: {targetAudience.Description}

            SUBJECT: {title}
            CONTENT:
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
            "Newsletter generated for agency {AgencyId}: overall score {Score}",
            agency.Id, scores.Overall);

        var result = new GeneratedContentResult(
            Title: title,
            Body: body,
            Type: ContentType.Newsletter,
            QualityScore: scores.Quality,
            RelevanceScore: scores.Relevance,
            SeoScore: scores.Seo,
            BrandVoiceScore: scores.BrandVoice,
            OverallScore: scores.Overall,
            ScoreExplanation: scores.Explanation);

        return new AgentJobResult(
            Success: true,
            Output: $"Generated newsletter: {title}",
            Contents: [result]);
    }

    private static (string Title, string Body) ParseContent(string text)
    {
        var title = "Newsletter";
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
