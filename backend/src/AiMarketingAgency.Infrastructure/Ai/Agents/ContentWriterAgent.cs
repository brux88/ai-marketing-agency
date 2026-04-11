using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai.Agents;

public class ContentWriterAgent : IMarketingAgent
{
    private readonly ILogger<ContentWriterAgent> _logger;

    public ContentWriterAgent(ILogger<ContentWriterAgent> logger)
    {
        _logger = logger;
    }

    public AgentType Type => AgentType.ContentWriter;

    public async Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct)
    {
        var chatCompletion = context.Kernel.GetRequiredService<IChatCompletionService>();
        var agency = context.Agency;
        var brandVoice = agency.BrandVoice;
        var targetAudience = agency.TargetAudience;

        // Step 1: Build generation prompt
        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var generatePrompt = $"""
            You are an expert content writer for "{agency.ProductName}".

            BRAND VOICE:
            - Tone: {brandVoice.Tone}
            - Style: {brandVoice.Style}
            - Keywords to include: {string.Join(", ", brandVoice.Keywords)}
            - Example phrases: {string.Join("; ", brandVoice.ExamplePhrases)}
            - Forbidden words: {string.Join(", ", brandVoice.ForbiddenWords)}
            - Language: {brandVoice.Language}

            TARGET AUDIENCE:
            - Description: {targetAudience.Description}
            - Age range: {targetAudience.AgeRange ?? "Not specified"}
            - Interests: {string.Join(", ", targetAudience.Interests)}
            - Pain points: {string.Join(", ", targetAudience.PainPoints)}

            CONTENT SOURCES (for context and inspiration):
            {sourcesContext}

            TASK: {context.Input ?? "Write a high-quality blog post about a relevant topic for our audience."}

            REQUIREMENTS:
            - Write in {brandVoice.Language} language
            - Optimize for SEO with proper headings (H2, H3), meta description, and keyword placement
            - Include a compelling title
            - Make it engaging and valuable for the target audience
            - Follow the brand voice guidelines strictly
            - Length: 800-1500 words

            FORMAT YOUR RESPONSE AS:
            TITLE: [article title]
            META_DESCRIPTION: [150-160 char meta description]
            ---
            [article body in markdown]
            """;

        _logger.LogInformation("Generating content for agency {AgencyId}", agency.Id);

        // Step 2: Generate article
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(generatePrompt);
        var generatedResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var generatedText = generatedResponse.Content ?? string.Empty;

        // Parse title and body
        var (title, body) = ParseGeneratedContent(generatedText);

        // Step 3: Score the content
        var scorePrompt = $"""
            You are a content quality analyst. Score the following article on a scale of 1-10 for each criterion.

            BRAND VOICE GUIDELINES:
            - Tone: {brandVoice.Tone}, Style: {brandVoice.Style}
            - Keywords: {string.Join(", ", brandVoice.Keywords)}
            - Forbidden words: {string.Join(", ", brandVoice.ForbiddenWords)}

            TARGET AUDIENCE: {targetAudience.Description}

            ARTICLE:
            Title: {title}
            {body}

            Score each criterion from 1 to 10. Respond ONLY in this exact format (numbers only, no decimals):
            QUALITY: [score]
            RELEVANCE: [score]
            SEO: [score]
            BRAND_VOICE: [score]
            OVERALL: [score]
            EXPLANATION: [one paragraph explaining the scores]
            """;

        var scoreHistory = new ChatHistory();
        scoreHistory.AddUserMessage(scorePrompt);
        var scoreResponse = await chatCompletion.GetChatMessageContentAsync(scoreHistory, cancellationToken: ct);
        var scoreText = scoreResponse.Content ?? string.Empty;

        var scores = ParseScores(scoreText);

        _logger.LogInformation(
            "Content generated for agency {AgencyId}: overall score {Score}",
            agency.Id, scores.Overall);

        var result = new GeneratedContentResult(
            Title: title,
            Body: body,
            Type: ContentType.BlogPost,
            QualityScore: scores.Quality,
            RelevanceScore: scores.Relevance,
            SeoScore: scores.Seo,
            BrandVoiceScore: scores.BrandVoice,
            OverallScore: scores.Overall,
            ScoreExplanation: scores.Explanation);

        return new AgentJobResult(
            Success: true,
            Output: $"Generated blog post: {title}",
            Contents: [result]);
    }

    private static (string Title, string Body) ParseGeneratedContent(string text)
    {
        var title = "Untitled";
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
        {
            body = string.Join('\n', lines[bodyStartIndex..]).Trim();
        }

        return (title, body);
    }

    private static ContentScores ParseScores(string text)
    {
        var quality = ExtractScore(text, "QUALITY");
        var relevance = ExtractScore(text, "RELEVANCE");
        var seo = ExtractScore(text, "SEO");
        var brandVoice = ExtractScore(text, "BRAND_VOICE");
        var overall = ExtractScore(text, "OVERALL");
        var explanation = ExtractExplanation(text);

        return new ContentScores(quality, relevance, seo, brandVoice, overall, explanation);
    }

    private static decimal ExtractScore(string text, string label)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith($"{label}:", StringComparison.OrdinalIgnoreCase))
            {
                var valueStr = trimmed[$"{label}:".Length..].Trim();
                if (decimal.TryParse(valueStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var score))
                {
                    return Math.Clamp(score, 1m, 10m);
                }
            }
        }
        return 5m; // Default if parsing fails
    }

    private static string ExtractExplanation(string text)
    {
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().StartsWith("EXPLANATION:", StringComparison.OrdinalIgnoreCase))
            {
                return lines[i].Trim()["EXPLANATION:".Length..].Trim();
            }
        }
        return string.Empty;
    }

    private record ContentScores(
        decimal Quality,
        decimal Relevance,
        decimal Seo,
        decimal BrandVoice,
        decimal Overall,
        string Explanation);
}
