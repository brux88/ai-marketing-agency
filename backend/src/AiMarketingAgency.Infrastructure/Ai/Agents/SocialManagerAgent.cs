using AiMarketingAgency.Application.Agents;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Infrastructure.Ai.Agents;

public class SocialManagerAgent : IMarketingAgent
{
    private readonly ILogger<SocialManagerAgent> _logger;

    public SocialManagerAgent(ILogger<SocialManagerAgent> logger)
    {
        _logger = logger;
    }

    public AgentType Type => AgentType.SocialManager;

    public async Task<AgentJobResult> ExecuteAsync(AgentJobContext context, CancellationToken ct)
    {
        var chatCompletion = context.Kernel.GetRequiredService<IChatCompletionService>();
        var agency = context.Agency;
        var brandVoice = agency.BrandVoice;
        var targetAudience = agency.TargetAudience;

        var platforms = new[]
        {
            (Platform: SocialPlatform.Twitter, Name: "Twitter/X", MaxLength: 280, ToneHint: "concise, punchy, use hashtags"),
            (Platform: SocialPlatform.LinkedIn, Name: "LinkedIn", MaxLength: 3000, ToneHint: "professional, insightful, thought-leadership"),
            (Platform: SocialPlatform.Instagram, Name: "Instagram", MaxLength: 2200, ToneHint: "visual-oriented, casual, use emojis and hashtags"),
            (Platform: SocialPlatform.Facebook, Name: "Facebook", MaxLength: 5000, ToneHint: "conversational, engaging, encourage comments"),
        };

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var platformDescriptions = string.Join("\n", platforms.Select(p =>
            $"- {p.Name}: max {p.MaxLength} chars, tone: {p.ToneHint}"));

        var generatePrompt = $"""
            You are an expert social media manager for "{agency.ProductName}".

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

            CONTENT SOURCES (for context and inspiration):
            {sourcesContext}

            TASK: {context.Input ?? "Create engaging social media posts about a relevant topic for our audience."}

            Generate ONE post for EACH of the following platforms, adapting tone and length:
            {platformDescriptions}

            FORMAT YOUR RESPONSE EXACTLY AS:
            [TWITTER]
            TITLE: [short descriptive title]
            POST: [post content]
            [/TWITTER]

            [LINKEDIN]
            TITLE: [short descriptive title]
            POST: [post content]
            [/LINKEDIN]

            [INSTAGRAM]
            TITLE: [short descriptive title]
            POST: [post content]
            [/INSTAGRAM]

            [FACEBOOK]
            TITLE: [short descriptive title]
            POST: [post content]
            [/FACEBOOK]
            """;

        _logger.LogInformation("Generating social media posts for agency {AgencyId}", agency.Id);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(generatePrompt);
        var generatedResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var generatedText = generatedResponse.Content ?? string.Empty;

        var posts = ParsePlatformPosts(generatedText);
        var results = new List<GeneratedContentResult>();

        // Score each post
        foreach (var (platformTag, title, body) in posts)
        {
            var scorePrompt = $"""
                Score this {platformTag} social media post from 1-10 on each criterion.

                BRAND VOICE: Tone={brandVoice.Tone}, Style={brandVoice.Style}
                TARGET AUDIENCE: {targetAudience.Description}

                POST TITLE: {title}
                POST CONTENT: {body}

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

            results.Add(new GeneratedContentResult(
                Title: $"[{platformTag}] {title}",
                Body: body,
                Type: ContentType.SocialPost,
                QualityScore: scores.Quality,
                RelevanceScore: scores.Relevance,
                SeoScore: scores.Seo,
                BrandVoiceScore: scores.BrandVoice,
                OverallScore: scores.Overall,
                ScoreExplanation: scores.Explanation));
        }

        _logger.LogInformation(
            "Generated {Count} social posts for agency {AgencyId}",
            results.Count, agency.Id);

        return new AgentJobResult(
            Success: true,
            Output: $"Generated {results.Count} social media posts",
            Contents: results);
    }

    private static List<(string Platform, string Title, string Body)> ParsePlatformPosts(string text)
    {
        var results = new List<(string, string, string)>();
        var platformTags = new[] { "TWITTER", "LINKEDIN", "INSTAGRAM", "FACEBOOK" };

        foreach (var tag in platformTags)
        {
            var startMarker = $"[{tag}]";
            var endMarker = $"[/{tag}]";
            var startIdx = text.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
            var endIdx = text.IndexOf(endMarker, StringComparison.OrdinalIgnoreCase);

            if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) continue;

            var section = text[(startIdx + startMarker.Length)..endIdx].Trim();
            var title = "Social Post";
            var body = section;

            var lines = section.Split('\n', StringSplitOptions.None);
            var bodyLines = new List<string>();
            var foundPost = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
                {
                    title = trimmed["TITLE:".Length..].Trim();
                }
                else if (trimmed.StartsWith("POST:", StringComparison.OrdinalIgnoreCase))
                {
                    bodyLines.Add(trimmed["POST:".Length..].Trim());
                    foundPost = true;
                }
                else if (foundPost)
                {
                    bodyLines.Add(line);
                }
            }

            if (bodyLines.Count > 0)
                body = string.Join('\n', bodyLines).Trim();

            results.Add((tag, title, body));
        }

        return results;
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
