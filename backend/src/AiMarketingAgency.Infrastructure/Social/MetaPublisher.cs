using System.Text;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Infrastructure.Social;

public class MetaPublisher : ISocialPublishingService
{
    private readonly HttpClient _httpClient;
    private readonly SocialPlatform _platform;

    public MetaPublisher(HttpClient httpClient, SocialPlatform platform)
    {
        _httpClient = httpClient;
        _platform = platform;
    }

    public async Task<PublishResult> PublishAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        try
        {
            return _platform switch
            {
                SocialPlatform.Instagram => await PublishToInstagramAsync(connector, content, ct),
                SocialPlatform.Facebook => await PublishToFacebookAsync(connector, content, ct),
                _ => new PublishResult(false, null, null, $"Unsupported Meta platform: {_platform}")
            };
        }
        catch (Exception ex)
        {
            return new PublishResult(false, null, null, ex.Message);
        }
    }

    private async Task<PublishResult> PublishToInstagramAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        var igUserId = connector.AccountId;
        var accessToken = connector.AccessToken;

        // Step 1: Create media container
        var createUrl = $"https://graph.facebook.com/v19.0/{igUserId}/media";
        var createParams = new Dictionary<string, string>
        {
            ["caption"] = $"{content.Title}\n\n{content.Body}",
            ["access_token"] = accessToken
        };

        if (!string.IsNullOrEmpty(content.ImageUrl))
            createParams["image_url"] = content.ImageUrl;

        var createResponse = await _httpClient.PostAsync(createUrl,
            new FormUrlEncodedContent(createParams), ct);
        var createBody = await createResponse.Content.ReadAsStringAsync(ct);

        if (!createResponse.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Instagram create error: {createBody}");

        using var createDoc = JsonDocument.Parse(createBody);
        var containerId = createDoc.RootElement.GetProperty("id").GetString();

        // Step 2: Publish the container
        var publishUrl = $"https://graph.facebook.com/v19.0/{igUserId}/media_publish";
        var publishParams = new Dictionary<string, string>
        {
            ["creation_id"] = containerId!,
            ["access_token"] = accessToken
        };

        var publishResponse = await _httpClient.PostAsync(publishUrl,
            new FormUrlEncodedContent(publishParams), ct);
        var publishBody = await publishResponse.Content.ReadAsStringAsync(ct);

        if (!publishResponse.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Instagram publish error: {publishBody}");

        using var publishDoc = JsonDocument.Parse(publishBody);
        var mediaId = publishDoc.RootElement.GetProperty("id").GetString();

        return new PublishResult(true, mediaId, $"https://www.instagram.com/p/{mediaId}/", null);
    }

    private async Task<PublishResult> PublishToFacebookAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        var pageId = connector.AccountId;
        var accessToken = connector.AccessToken;

        var url = $"https://graph.facebook.com/v19.0/{pageId}/feed";
        var postParams = new Dictionary<string, string>
        {
            ["message"] = $"{content.Title}\n\n{content.Body}",
            ["access_token"] = accessToken
        };

        if (!string.IsNullOrEmpty(content.ImageUrl))
        {
            url = $"https://graph.facebook.com/v19.0/{pageId}/photos";
            postParams["url"] = content.ImageUrl;
        }

        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(postParams), ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new PublishResult(false, null, null, $"Facebook API error: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        var postId = doc.RootElement.GetProperty("id").GetString();

        return new PublishResult(true, postId, $"https://www.facebook.com/{postId}", null);
    }
}
