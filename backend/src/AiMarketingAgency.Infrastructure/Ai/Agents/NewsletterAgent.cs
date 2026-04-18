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
        var project = context.Project;
        var brandVoice = project?.BrandVoice ?? agency.BrandVoice;
        var targetAudience = project?.TargetAudience ?? agency.TargetAudience;
        var productName = project?.Name ?? agency.ProductName;
        var projectContextBlock = !string.IsNullOrWhiteSpace(project?.ExtractedContext)
            ? $"\nPROJECT CONTEXT (from website):\n{project!.ExtractedContext}\n"
            : string.Empty;

        var sourcesContext = string.Join("\n", context.Sources.Select(s =>
            $"- [{s.Name ?? s.Type.ToString()}] {s.Url}"));

        // Build recent content history block
        var recentHistoryBlock = string.Empty;
        if (context.RecentContents != null && context.RecentContents.Count > 0)
        {
            var recentTitles = string.Join("\n", context.RecentContents.Select(r =>
                $"  - [{r.CreatedAt:dd/MM}] {r.Title}"));
            recentHistoryBlock = $"""

            PREVIOUSLY SENT NEWSLETTERS (DO NOT repeat these themes):
            {recentTitles}

            CONTENT DIVERSITY STRATEGY:
            Each newsletter MUST have a COMPLETELY DIFFERENT main theme from those listed above.
            Rotate the featured story focus among:
            - Product update or new feature spotlight
            - Industry analysis and market trends
            - Practical tutorial or how-to guide
            - Customer use case or success scenario
            - Expert insights or thought leadership
            - Seasonal/timely content tied to current events
            - FAQ or common challenges addressed
            - Behind-the-scenes or company culture
            Choose the theme LEAST covered in the previous newsletters above.
            """;
        }

        string generatePrompt;
        if (!string.IsNullOrWhiteSpace(project?.NewsletterPromptTemplate))
        {
            generatePrompt = project!.NewsletterPromptTemplate!
                .Replace("{product}", productName)
                .Replace("{brandVoice}", $"{brandVoice.Tone}, {brandVoice.Style}, language {brandVoice.Language}")
                .Replace("{audience}", targetAudience.Description)
                .Replace("{projectContext}", project.ExtractedContext ?? string.Empty)
                .Replace("{sources}", sourcesContext)
                .Replace("{task}", context.Input ?? "Crea una newsletter rilevante per il dominio del progetto.")
                + recentHistoryBlock
                + "\n\nFORMAT YOUR RESPONSE AS:\nTITLE: [newsletter subject]\n---\n[newsletter body]";
        }
        else
        {
        generatePrompt = $"""
            You are an expert newsletter curator and writer for "{productName}".
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

            CONTENT SOURCES (curate from these):
            {sourcesContext}
            {recentHistoryBlock}

            TASK: {context.Input ?? "Create a compelling weekly newsletter for our audience. Stay strictly within the project's actual domain — do NOT write generic marketing/AI content unless that is the project's actual topic."}

            Ground everything in the PROJECT CONTEXT above when available. Do not invent features or topics outside the project's actual scope.

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
        }

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
