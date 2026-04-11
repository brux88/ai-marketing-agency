using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Infrastructure.Social;

public class LinkedInPublisher : ISocialPublishingService
{
    private readonly HttpClient _httpClient;

    public LinkedInPublisher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PublishResult> PublishAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", connector.AccessToken);
            _httpClient.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

            var authorUrn = $"urn:li:person:{connector.AccountId}";
            var postText = $"{content.Title}\n\n{content.Body}";

            object payload;
            if (!string.IsNullOrEmpty(content.ImageUrl))
            {
                payload = new
                {
                    author = authorUrn,
                    lifecycleState = "PUBLISHED",
                    specificContent = new
                    {
                        comLinkedinUgcShareContent = new
                        {
                            shareCommentary = new { text = postText },
                            shareMediaCategory = "ARTICLE",
                            media = new[]
                            {
                                new
                                {
                                    status = "READY",
                                    originalUrl = content.ImageUrl
                                }
                            }
                        }
                    },
                    visibility = new
                    {
                        comLinkedinUgcMemberNetworkVisibility = "PUBLIC"
                    }
                };
            }
            else
            {
                payload = new
                {
                    author = authorUrn,
                    lifecycleState = "PUBLISHED",
                    specificContent = new
                    {
                        comLinkedinUgcShareContent = new
                        {
                            shareCommentary = new { text = postText },
                            shareMediaCategory = "NONE"
                        }
                    },
                    visibility = new
                    {
                        comLinkedinUgcMemberNetworkVisibility = "PUBLIC"
                    }
                };
            }

            var json = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.linkedin.com/v2/ugcPosts", httpContent, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return new PublishResult(false, null, null, $"LinkedIn API error: {response.StatusCode} - {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            var postId = doc.RootElement.GetProperty("id").GetString();

            return new PublishResult(true, postId, $"https://www.linkedin.com/feed/update/{postId}/", null);
        }
        catch (Exception ex)
        {
            return new PublishResult(false, null, null, ex.Message);
        }
    }
}
