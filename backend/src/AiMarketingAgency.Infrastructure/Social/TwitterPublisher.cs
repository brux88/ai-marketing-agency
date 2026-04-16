using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Infrastructure.Social;

public class TwitterPublisher : ISocialPublishingService
{
    private readonly HttpClient _httpClient;

    public TwitterPublisher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PublishResult> PublishAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", connector.AccessToken);

            var tweetText = BuildTweetText(content);

            var payload = new { text = tweetText };
            var json = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.twitter.com/2/tweets", httpContent, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return new PublishResult(false, null, null, $"Twitter API error: {response.StatusCode} - {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            var tweetId = doc.RootElement.GetProperty("data").GetProperty("id").GetString();

            return new PublishResult(true, tweetId, $"https://twitter.com/i/web/status/{tweetId}", null);
        }
        catch (Exception ex)
        {
            return new PublishResult(false, null, null, ex.Message);
        }
    }

    private static string BuildTweetText(GeneratedContent content)
    {
        var text = SocialPostTextBuilder.Build(content);
        return text.Length > 280 ? text[..277] + "..." : text;
    }
}
