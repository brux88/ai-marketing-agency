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
        var project = context.Project;
        var brandVoice = project?.BrandVoice ?? agency.BrandVoice;
        var targetAudience = project?.TargetAudience ?? agency.TargetAudience;
        var productName = project?.Name ?? agency.ProductName;
        var projectContextBlock = !string.IsNullOrWhiteSpace(project?.ExtractedContext)
            ? $"\nPROJECT CONTEXT (from website):\n{project!.ExtractedContext}\n"
            : string.Empty;

        // Build document context block (RAG)
        var documentsBlock = string.Empty;
        if (context.Documents != null && context.Documents.Count > 0)
        {
            var docTexts = string.Join("\n\n", context.Documents.Select(d =>
                $"── {d.Name} ──\n{(d.ExtractedText.Length > 3000 ? d.ExtractedText[..3000] + "..." : d.ExtractedText)}"));
            documentsBlock = $"""

            ADDITIONAL CONTEXT DOCUMENTS (use these as knowledge base):
            {docTexts}

            Use specific details, data, and insights from these documents in your posts.
            """;
        }

        var allPlatforms = new[]
        {
            (Platform: SocialPlatform.Twitter, Name: "Twitter/X", Tag: "TWITTER", MaxLength: 280, ToneHint: "concise, punchy, use hashtags"),
            (Platform: SocialPlatform.LinkedIn, Name: "LinkedIn", Tag: "LINKEDIN", MaxLength: 3000, ToneHint: "professional, insightful, thought-leadership"),
            (Platform: SocialPlatform.Instagram, Name: "Instagram", Tag: "INSTAGRAM", MaxLength: 2200, ToneHint: "visual-oriented, casual, use emojis and hashtags"),
            (Platform: SocialPlatform.Facebook, Name: "Facebook", Tag: "FACEBOOK", MaxLength: 5000, ToneHint: "conversational, engaging, encourage comments"),
        };

        // Filter platforms according to schedule > project preference (null/empty = all)
        var platformPref = !string.IsNullOrWhiteSpace(context.Schedule?.EnabledSocialPlatforms)
            ? context.Schedule!.EnabledSocialPlatforms
            : project?.EnabledSocialPlatforms;
        var enabledPrefs = !string.IsNullOrWhiteSpace(platformPref)
            ? platformPref!
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant())
                .ToHashSet()
            : null;
        var platforms = enabledPrefs == null
            ? allPlatforms
            : allPlatforms.Where(p => enabledPrefs.Contains(p.Tag) || enabledPrefs.Contains(p.Platform.ToString().ToUpperInvariant())).ToArray();
        if (platforms.Length == 0) platforms = allPlatforms; // safety fallback

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var platformDescriptions = string.Join("\n", platforms.Select(p =>
            $"- {p.Name}: max {p.MaxLength} chars, tone: {p.ToneHint}"));

        var platformFormatBlocks = string.Join("\n\n", platforms.Select(p =>
            $"[{p.Tag}]\nTITLE: [short descriptive title]\nPOST: [post content]\n[/{p.Tag}]"));

        var projectUrl = project?.WebsiteUrl ?? agency.WebsiteUrl;
        var ctaRule = !string.IsNullOrWhiteSpace(projectUrl)
            ? $"If the post includes a call-to-action link, use EXACTLY this URL: {projectUrl}. NEVER write placeholder text like [Link Demo], [link], [URL], [tuo link], [website]. Either write the real URL or omit the link entirely."
            : "Do NOT include any placeholder text like [Link Demo], [link], [URL], [tuo link], [website] or similar brackets. Either write a real URL or omit the link entirely.";

        // Build recent content history block
        var recentHistoryBlock = string.Empty;
        if (context.RecentContents != null && context.RecentContents.Count > 0)
        {
            var recentTitles = string.Join("\n", context.RecentContents.Select(r =>
                $"  - [{r.CreatedAt:dd/MM}] {r.Title}"));
            recentHistoryBlock = $"""

            RECENTLY PUBLISHED/GENERATED CONTENT (DO NOT repeat these topics or angles):
            {recentTitles}

            CONTENT DIVERSITY STRATEGY:
            You MUST create content that is COMPLETELY DIFFERENT from the above list.
            Use a different angle, topic, feature, or theme each time. Rotate among these approaches:
            - Highlight a SPECIFIC feature or capability of the product (pick one not covered recently)
            - Address a specific pain point or use case of the target audience
            - Share a practical tip, tutorial, or how-to
            - Tell a customer success story or use case scenario
            - Discuss an industry trend relevant to the product's domain
            - Create educational content about the problem the product solves
            - Compare approaches or methodologies relevant to the audience
            - Share behind-the-scenes insights, updates, or news about the product
            Pick the approach LEAST represented in the recent content list above.
            """;
        }

        string generatePrompt;
        if (!string.IsNullOrWhiteSpace(project?.SocialPromptTemplate))
        {
            generatePrompt = project!.SocialPromptTemplate!
                .Replace("{product}", productName)
                .Replace("{brandVoice}", $"{brandVoice.Tone}, {brandVoice.Style}, language {brandVoice.Language}")
                .Replace("{audience}", targetAudience.Description)
                .Replace("{projectContext}", project.ExtractedContext ?? string.Empty)
                .Replace("{sources}", sourcesContext)
                .Replace("{task}", context.Input ?? "Crea post social rilevanti per il dominio specifico del progetto.")
                + recentHistoryBlock
                + documentsBlock
                + $"\n\nCTA / LINK RULE: {ctaRule}"
                + $"\n\nGenerate ONLY for the following platforms:\n{platformDescriptions}"
                + $"\n\nFORMAT YOUR RESPONSE EXACTLY AS:\n{platformFormatBlocks}";
        }
        else
        {
        generatePrompt = $"""
            You are an expert social media manager for "{productName}".
            Today's date is {DateTime.UtcNow:MMMM yyyy, dd/MM/yyyy}.
            {projectContextBlock}
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
            {recentHistoryBlock}
            {documentsBlock}

            TASK: {context.Input ?? "Create engaging social media posts about a relevant topic for our audience. Stay strictly within the project's actual domain — do NOT write generic marketing content."}

            CTA / LINK RULE: {ctaRule}

            Ground every post in the PROJECT CONTEXT above when available. Do not invent features or topics outside the project's actual scope.

            Generate ONE post for EACH of the following platforms (and ONLY these), adapting tone and length:
            {platformDescriptions}

            FORMAT YOUR RESPONSE EXACTLY AS:
            {platformFormatBlocks}
            """;
        }

        _logger.LogInformation(
            "Generating social media posts for agency {AgencyId}, enabled platforms: [{Platforms}] (project pref: '{Pref}')",
            agency.Id,
            string.Join(",", platforms.Select(p => p.Tag)),
            project?.EnabledSocialPlatforms ?? "(null)");

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(generatePrompt);
        var generatedResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var generatedText = generatedResponse.Content ?? string.Empty;

        var enabledTags = platforms.Select(p => p.Tag).ToArray();
        var posts = ParsePlatformPosts(generatedText, enabledTags);
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

    private static List<(string Platform, string Title, string Body)> ParsePlatformPosts(string text, string[] enabledTags)
    {
        var results = new List<(string, string, string)>();

        foreach (var tag in enabledTags)
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
