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
        var project = context.Project;
        // Prefer project brand voice/audience when project is set
        var brandVoice = project?.BrandVoice ?? agency.BrandVoice;
        var targetAudience = project?.TargetAudience ?? agency.TargetAudience;
        var productName = project?.Name ?? agency.ProductName;

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        var projectContext = !string.IsNullOrWhiteSpace(project?.ExtractedContext)
            ? $"\n\nPROJECT CONTEXT (extracted from website):\n{project!.ExtractedContext}\n"
            : string.Empty;

        // Build document context block (RAG)
        var documentsBlock = string.Empty;
        if (context.Documents != null && context.Documents.Count > 0)
        {
            var docTexts = string.Join("\n\n", context.Documents.Select(d =>
                $"── {d.Name} ──\n{(d.ExtractedText.Length > 5000 ? d.ExtractedText[..5000] + "..." : d.ExtractedText)}"));
            documentsBlock = $"""

            ADDITIONAL CONTEXT DOCUMENTS (use these as knowledge base for content creation):
            {docTexts}

            Use the information from these documents to create more accurate, detailed and grounded content.
            Reference specific data, features, or insights from the documents when relevant.
            """;
        }

        // Build recent content history block with body excerpts for better diversity
        var recentHistoryBlock = string.Empty;
        if (context.RecentContents != null && context.RecentContents.Count > 0)
        {
            var recentEntries = string.Join("\n", context.RecentContents.Select(r =>
                $"  - [{r.CreatedAt:dd/MM}] {r.Title}\n    Summary: {r.BodyExcerpt ?? "(no excerpt)"}"));
            recentHistoryBlock = $"""

            ⚠️ CRITICAL — CONTENT DIVERSITY REQUIREMENT (THIS IS MANDATORY):
            The following articles have ALREADY been written. You MUST NOT repeat any of these topics, angles, or themes.

            RECENTLY PUBLISHED/GENERATED ARTICLES:
            {recentEntries}

            DIVERSITY RULES:
            1. Your new article MUST cover a COMPLETELY DIFFERENT topic from ALL items above
            2. Do NOT rephrase or reword the same ideas — find genuinely new angles
            3. Rotate among these content pillars (pick the LEAST used above):
               - Deep-dive into a SPECIFIC feature or capability not yet covered
               - Tutorial / step-by-step guide on a particular use case
               - Problem-solution article addressing a specific audience pain point
               - Industry trends and how the product relates to them
               - Comparison or best practices article
            - Case study or real-world application scenario
            - Educational content about the broader domain
            - Tips, tricks, or lesser-known aspects of the product
            Choose the pillar LEAST represented in the recent articles list above.
            """;
        }

        // If a custom per-project template is set, use it (with placeholders substituted)
        string generatePrompt;
        if (!string.IsNullOrWhiteSpace(project?.BlogPromptTemplate))
        {
            generatePrompt = project!.BlogPromptTemplate!
                .Replace("{product}", productName)
                .Replace("{brandVoice}", $"{brandVoice.Tone}, {brandVoice.Style}, language {brandVoice.Language}")
                .Replace("{keywords}", string.Join(", ", brandVoice.Keywords))
                .Replace("{audience}", targetAudience.Description)
                .Replace("{sources}", sourcesContext)
                .Replace("{projectContext}", project.ExtractedContext ?? string.Empty)
                .Replace("{task}", context.Input ?? "Scrivi un articolo blog di alta qualità rilevante per questo progetto.");
            generatePrompt += recentHistoryBlock;
            generatePrompt += documentsBlock;
            generatePrompt += "\n\nFORMAT YOUR RESPONSE AS:\nTITLE: [article title]\nMETA_DESCRIPTION: [150-160 char meta description]\n---\n[article body in markdown]";
        }
        else
        {
            generatePrompt = $"""
                You are an expert content writer for "{productName}".
                Today's date is {DateTime.UtcNow:MMMM yyyy, dd/MM/yyyy}.
                {projectContext}
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
                {recentHistoryBlock}
                {documentsBlock}

                TASK: {context.Input ?? "Write a high-quality blog post about a relevant topic for our audience. Stay strictly within the project's domain and expertise as described above — do NOT write generic marketing/AI content unless that is the actual topic of the project."}

                REQUIREMENTS:
                - Write in {brandVoice.Language} language
                - Ground the article in the PROJECT CONTEXT above when available. Do not invent features or topics outside the project's actual scope.
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
        }

        _logger.LogInformation("Generating content for agency {AgencyId}", agency.Id);

        // Step 2: Generate article
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(generatePrompt);
        var generatedResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var generatedText = generatedResponse.Content ?? string.Empty;

        // Parse title and body
        var (title, body) = ParseGeneratedContent(generatedText);

        // Step 3: Score the content
        var recentTitlesForScore = context.RecentContents != null && context.RecentContents.Count > 0
            ? string.Join(", ", context.RecentContents.Take(5).Select(r => $"\"{r.Title}\""))
            : "(none)";
        var scorePrompt = $"""
            You are a content quality analyst. Score the following article on a scale of 1-10 for each criterion.

            BRAND VOICE GUIDELINES:
            - Tone: {brandVoice.Tone}, Style: {brandVoice.Style}
            - Keywords: {string.Join(", ", brandVoice.Keywords)}
            - Forbidden words: {string.Join(", ", brandVoice.ForbiddenWords)}

            TARGET AUDIENCE: {targetAudience.Description}
            RECENT ARTICLES (for diversity check): {recentTitlesForScore}

            ARTICLE:
            Title: {title}
            {body}

            Score each criterion from 1 to 10. Respond ONLY in this exact format (numbers only, no decimals):
            QUALITY: [score]
            RELEVANCE: [score]
            SEO: [score]
            BRAND_VOICE: [score]
            DIVERSITY: [score — 1 if topic/angle is identical to recent articles, 10 if completely fresh]
            OVERALL: [score — average of all above, penalize heavily if DIVERSITY < 5]
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
