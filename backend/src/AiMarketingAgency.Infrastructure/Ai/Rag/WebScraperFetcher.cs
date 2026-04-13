using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai.Rag;

public class WebScraperFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebScraperFetcher> _logger;

    public WebScraperFetcher(HttpClient httpClient, ILogger<WebScraperFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<FetchedArticle>> FetchAsync(string url, CancellationToken ct = default)
    {
        var articles = new List<FetchedArticle>();
        try
        {
            var html = await _httpClient.GetStringAsync(url, ct);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove script and style tags
            foreach (var node in doc.DocumentNode.SelectNodes("//script|//style|//nav|//footer|//header") ?? Enumerable.Empty<HtmlNode>())
                node.Remove();

            var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim()
                ?? doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim()
                ?? "Web Page";

            // Extract main content from article or main tags, fallback to body
            var mainNode = doc.DocumentNode.SelectSingleNode("//article")
                ?? doc.DocumentNode.SelectSingleNode("//main")
                ?? doc.DocumentNode.SelectSingleNode("//body");

            var text = mainNode?.InnerText?.Trim() ?? "";

            // Clean up whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

            if (text.Length > 100)
            {
                articles.Add(new FetchedArticle
                {
                    Title = title,
                    Content = text[..Math.Min(text.Length, 10000)],
                    Url = url,
                    PublishedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scrape website: {Url}", url);
        }
        return articles;
    }
}
