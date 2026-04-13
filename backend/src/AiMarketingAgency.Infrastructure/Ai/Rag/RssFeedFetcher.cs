using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai.Rag;

public class RssFeedFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RssFeedFetcher> _logger;

    public RssFeedFetcher(HttpClient httpClient, ILogger<RssFeedFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<FetchedArticle>> FetchAsync(string url, CancellationToken ct = default)
    {
        var articles = new List<FetchedArticle>();
        try
        {
            var response = await _httpClient.GetStringAsync(url, ct);
            using var reader = XmlReader.Create(new StringReader(response));
            var feed = SyndicationFeed.Load(reader);

            foreach (var item in feed.Items.Take(20))
            {
                var content = item.Summary?.Text ?? item.Title?.Text ?? "";
                var link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? url;

                articles.Add(new FetchedArticle
                {
                    Title = item.Title?.Text ?? "Untitled",
                    Content = StripHtml(content),
                    Url = link,
                    PublishedAt = item.PublishDate.UtcDateTime
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch RSS feed: {Url}", url);
        }
        return articles;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.InnerText.Trim();
    }
}

public class FetchedArticle
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}
