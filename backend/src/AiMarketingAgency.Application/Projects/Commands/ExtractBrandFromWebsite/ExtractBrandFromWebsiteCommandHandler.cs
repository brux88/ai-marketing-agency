using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Projects.Dtos;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiMarketingAgency.Application.Projects.Commands.ExtractBrandFromWebsite;

public class ExtractBrandFromWebsiteCommandHandler : IRequestHandler<ExtractBrandFromWebsiteCommand, ProjectDto>
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly IAppDbContext _context;
    private readonly ILlmKernelFactory _kernelFactory;
    private readonly ILogger<ExtractBrandFromWebsiteCommandHandler> _logger;

    public ExtractBrandFromWebsiteCommandHandler(
        IAppDbContext context,
        ILlmKernelFactory kernelFactory,
        ILogger<ExtractBrandFromWebsiteCommandHandler> logger)
    {
        _context = context;
        _kernelFactory = kernelFactory;
        _logger = logger;
    }

    public async Task<ProjectDto> Handle(ExtractBrandFromWebsiteCommand request, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(p => p.ContentSources)
            .Include(p => p.GeneratedContents)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.AgencyId == request.AgencyId && p.IsActive, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        if (string.IsNullOrWhiteSpace(project.WebsiteUrl))
            throw new InvalidOperationException("Website URL is not set for this project.");

        string html;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, project.WebsiteUrl);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AiMarketingAgencyBot/1.0)");
            using var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();
            html = await resp.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch website {Url}", project.WebsiteUrl);
            throw new InvalidOperationException($"Could not fetch website: {ex.Message}");
        }

        var excerpt = ExtractTextContent(html);
        var meta = ExtractMetaTags(html);
        var logoUrl = ExtractLogoUrl(html, project.WebsiteUrl);

        // Try to fetch a few inner pages (blog, guides, features) for richer context
        var extraPages = await FetchExtraPagesAsync(project.WebsiteUrl, html, ct);
        var combinedExcerpt = excerpt;
        if (extraPages.Count > 0)
        {
            var joined = string.Join("\n\n--- PAGE BREAK ---\n\n", extraPages);
            combinedExcerpt = excerpt + "\n\n--- PAGE BREAK ---\n\n" + joined;
            if (combinedExcerpt.Length > 8000) combinedExcerpt = combinedExcerpt[..8000];
        }

        var kernel = await _kernelFactory.CreateKernelAsync(project.AgencyId, ct);
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var prompt = $$"""
            You are a brand strategist analyzing a company website. Your job is to deeply understand what this product/service ACTUALLY does, then build brand voice AND content-generation prompts tuned for this specific domain.
            Website URL: {{project.WebsiteUrl}}

            META TAGS:
            {{meta}}

            VISIBLE TEXT EXCERPT (may include multiple pages separated by PAGE BREAK, ~8000 chars):
            {{combinedExcerpt}}

            Return ONLY a JSON object with this exact shape (no markdown, no explanations, no trailing commas):
            {
              "tone": "one word like Professional, Friendly, Playful, Authoritative, Inspirational",
              "style": "short description (max 10 words) of writing style",
              "keywords": ["5-8 brand keywords"],
              "examplePhrases": ["2-3 short slogan-like phrases typical of this brand"],
              "forbiddenWords": [],
              "language": "it or en or detected ISO code",
              "audienceDescription": "short description (max 15 words) of the ideal audience",
              "audienceInterests": ["3-5 interests"],
              "audiencePainPoints": ["2-3 pain points the brand addresses"],
              "projectContext": "200-400 words describing what this project/product/service IS, what it does, who it serves, what problems it solves, and what topics are relevant for blog/social content. This will be injected into every future content-generation prompt to keep content on-domain.",
              "blogPromptTemplate": "A template prompt for generating a blog article specifically for this project. Use placeholders: {product}, {brandVoice}, {audience}, {projectContext}, {sources}, {task}. Should instruct the LLM to stay within the project's domain, use terminology from the site, and target the actual audience. Write in {{project.WebsiteUrl}} native language. ~300 words.",
              "socialPromptTemplate": "A template prompt for generating social media posts (Twitter/LinkedIn/Instagram/Facebook) specifically for this project. Use the same placeholders as blog. ~250 words.",
              "newsletterPromptTemplate": "A template prompt for generating a newsletter specifically for this project. Use the same placeholders. ~250 words."
            }
            """;

        var history = new ChatHistory();
        history.AddUserMessage(prompt);
        var response = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        var text = response.Content?.Trim() ?? "";

        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart < 0 || jsonEnd <= jsonStart)
            throw new InvalidOperationException("LLM did not return valid JSON.");

        var json = text.Substring(jsonStart, jsonEnd - jsonStart + 1);

        ExtractedBrand? extracted;
        try
        {
            extracted = JsonSerializer.Deserialize<ExtractedBrand>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse brand JSON: {Json}", json);
            throw new InvalidOperationException("Failed to parse brand extraction response.");
        }

        if (extracted == null)
            throw new InvalidOperationException("Brand extraction returned empty result.");

        project.BrandVoice = new BrandVoice
        {
            Tone = extracted.Tone ?? project.BrandVoice.Tone,
            Style = extracted.Style ?? project.BrandVoice.Style,
            Keywords = extracted.Keywords ?? project.BrandVoice.Keywords,
            ExamplePhrases = extracted.ExamplePhrases ?? project.BrandVoice.ExamplePhrases,
            ForbiddenWords = project.BrandVoice.ForbiddenWords,
            Language = extracted.Language ?? project.BrandVoice.Language,
        };

        project.TargetAudience = new TargetAudience
        {
            Description = extracted.AudienceDescription ?? project.TargetAudience.Description,
            AgeRange = project.TargetAudience.AgeRange,
            Interests = extracted.AudienceInterests ?? project.TargetAudience.Interests,
            PainPoints = extracted.AudiencePainPoints ?? project.TargetAudience.PainPoints,
            Personas = project.TargetAudience.Personas,
        };

        if (!string.IsNullOrWhiteSpace(extracted.ProjectContext))
        {
            project.ExtractedContext = extracted.ProjectContext;
            project.ExtractedContextAt = DateTime.UtcNow;
        }
        if (!string.IsNullOrWhiteSpace(extracted.BlogPromptTemplate))
            project.BlogPromptTemplate = extracted.BlogPromptTemplate;
        if (!string.IsNullOrWhiteSpace(extracted.SocialPromptTemplate))
            project.SocialPromptTemplate = extracted.SocialPromptTemplate;
        if (!string.IsNullOrWhiteSpace(extracted.NewsletterPromptTemplate))
            project.NewsletterPromptTemplate = extracted.NewsletterPromptTemplate;
        if (!string.IsNullOrWhiteSpace(logoUrl) && string.IsNullOrWhiteSpace(project.LogoUrl))
            project.LogoUrl = logoUrl;

        await _context.SaveChangesAsync(ct);

        return new ProjectDto
        {
            Id = project.Id,
            AgencyId = project.AgencyId,
            Name = project.Name,
            Description = project.Description,
            WebsiteUrl = project.WebsiteUrl,
            LogoUrl = project.LogoUrl,
            BrandVoice = project.BrandVoice,
            TargetAudience = project.TargetAudience,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt,
            ContentSourcesCount = project.ContentSources.Count(cs => cs.IsActive),
            GeneratedContentsCount = project.GeneratedContents.Count
        };
    }

    private static string ExtractTextContent(string html)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        foreach (var node in doc.DocumentNode.SelectNodes("//script|//style|//noscript")?.ToList() ?? new List<HtmlAgilityPack.HtmlNode>())
            node.Remove();

        var text = doc.DocumentNode.SelectSingleNode("//body")?.InnerText ?? doc.DocumentNode.InnerText;
        text = System.Net.WebUtility.HtmlDecode(text);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length > 3000 ? text[..3000] : text;
    }

    private static string ExtractMetaTags(string html)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        var parts = new List<string>();
        var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
        if (!string.IsNullOrEmpty(title)) parts.Add($"title: {title}");

        var metas = doc.DocumentNode.SelectNodes("//meta[@name='description' or @property='og:description' or @property='og:title' or @name='keywords']");
        if (metas != null)
        {
            foreach (var m in metas)
            {
                var key = m.GetAttributeValue("name", null) ?? m.GetAttributeValue("property", null) ?? "meta";
                var val = m.GetAttributeValue("content", "").Trim();
                if (!string.IsNullOrEmpty(val)) parts.Add($"{key}: {val}");
            }
        }

        return string.Join("\n", parts);
    }

    private static string? ExtractLogoUrl(string html, string pageUrl)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        var ogImg = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(ogImg)) return MakeAbsolute(ogImg, pageUrl);

        var iconLink = doc.DocumentNode.SelectSingleNode("//link[@rel='icon' or @rel='shortcut icon' or @rel='apple-touch-icon']")?.GetAttributeValue("href", null);
        if (!string.IsNullOrWhiteSpace(iconLink)) return MakeAbsolute(iconLink, pageUrl);

        return null;
    }

    private static string? MakeAbsolute(string url, string baseUrl)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var abs)) return abs.ToString();
        if (Uri.TryCreate(new Uri(baseUrl), url, out var rel)) return rel.ToString();
        return null;
    }

    private static async Task<List<string>> FetchExtraPagesAsync(string baseUrl, string indexHtml, CancellationToken ct)
    {
        var results = new List<string>();
        try
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(indexHtml);
            var baseUri = new Uri(baseUrl);
            var candidates = new List<string>();

            var nav = doc.DocumentNode.SelectNodes("//a[@href]");
            if (nav != null)
            {
                foreach (var a in nav)
                {
                    var href = a.GetAttributeValue("href", null);
                    if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#") || href.StartsWith("mailto:") || href.StartsWith("tel:")) continue;
                    if (!Uri.TryCreate(baseUri, href, out var abs)) continue;
                    if (abs.Host != baseUri.Host) continue;
                    var path = abs.AbsolutePath.ToLowerInvariant();
                    if (path.Contains("blog") || path.Contains("guide") || path.Contains("feature") ||
                        path.Contains("product") || path.Contains("soluzioni") || path.Contains("servizi") ||
                        path.Contains("about") || path.Contains("chi-siamo"))
                    {
                        candidates.Add(abs.ToString());
                        if (candidates.Count >= 3) break;
                    }
                }
            }

            foreach (var url in candidates.Distinct().Take(3))
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, url);
                    req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AiMarketingAgencyBot/1.0)");
                    using var resp = await _http.SendAsync(req, ct);
                    if (!resp.IsSuccessStatusCode) continue;
                    var pageHtml = await resp.Content.ReadAsStringAsync(ct);
                    var text = ExtractTextContent(pageHtml);
                    if (text.Length > 2000) text = text[..2000];
                    results.Add($"[{url}]\n{text}");
                }
                catch { /* skip failures */ }
            }
        }
        catch { /* best-effort */ }
        return results;
    }

    private class ExtractedBrand
    {
        public string? Tone { get; set; }
        public string? Style { get; set; }
        public List<string>? Keywords { get; set; }
        public List<string>? ExamplePhrases { get; set; }
        public string? Language { get; set; }
        public string? AudienceDescription { get; set; }
        public List<string>? AudienceInterests { get; set; }
        public List<string>? AudiencePainPoints { get; set; }
        public string? ProjectContext { get; set; }
        public string? BlogPromptTemplate { get; set; }
        public string? SocialPromptTemplate { get; set; }
        public string? NewsletterPromptTemplate { get; set; }
    }
}
