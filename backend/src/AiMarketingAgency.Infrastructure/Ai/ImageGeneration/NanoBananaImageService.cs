using System.Net.Http.Json;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class NanoBananaImageService : IImageGenerationService
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private static readonly HttpClient _httpClient = new();

    public NanoBananaImageService(string apiKey, string? baseUrl = null)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl ?? "https://api.nanobanana.ai/v1";
    }

    public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, ImageGenerationOptions options, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/images/generate");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
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
