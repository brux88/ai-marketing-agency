using System.Net.Http.Json;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class CustomImageService : IImageGenerationService
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private static readonly HttpClient _httpClient = new();

    public CustomImageService(string apiKey, string baseUrl)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
    }

    public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, ImageGenerationOptions options, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/images/generations");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(new
        {
            model = "dall-e-3",
            prompt,
            n = 1,
            size = $"{options.Width}x{options.Height}",
            quality = "standard",
            response_format = "url"
        });

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var imageUrl = json.GetProperty("data")[0].GetProperty("url").GetString()!;
        var revisedPrompt = json.GetProperty("data")[0].GetProperty("revised_prompt").GetString() ?? prompt;

        return new ImageGenerationResult(imageUrl, revisedPrompt);
    }
}
