using System.Net.Http.Json;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class HiggFieldImageService : IImageGenerationService
{
    private readonly string _apiKey;
    private readonly string? _apiKeySecret;
    private readonly string _baseUrl;
    private static readonly HttpClient _httpClient = new();

    public HiggFieldImageService(string apiKey, string? apiKeySecret = null, string? baseUrl = null)
    {
        _apiKey = apiKey;
        _apiKeySecret = apiKeySecret;
        _baseUrl = baseUrl ?? "https://api.higgfield.ai/v1";
    }

    public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, ImageGenerationOptions options, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/images/generate");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        if (!string.IsNullOrEmpty(_apiKeySecret))
        {
            request.Headers.Add("X-Api-Secret", _apiKeySecret);
        }

        request.Content = JsonContent.Create(new
        {
            prompt,
            width = options.Width,
            height = options.Height,
            style = options.Style ?? "natural"
        });

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var imageUrl = json.GetProperty("image_url").GetString()!;

        return new ImageGenerationResult(imageUrl, prompt);
    }
}
