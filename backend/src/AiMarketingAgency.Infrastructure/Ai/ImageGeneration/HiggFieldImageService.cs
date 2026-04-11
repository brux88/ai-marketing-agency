using System.Net.Http.Json;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class HiggFieldImageService : IImageGenerationService
{
    private readonly string _apiKey;
    private readonly string? _apiKeySecret;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;

    public HiggFieldImageService(string apiKey, string? apiKeySecret = null, string? baseUrl = null, ILogger? logger = null)
    {
        _apiKey = apiKey;
        _apiKeySecret = apiKeySecret;
        _baseUrl = baseUrl?.TrimEnd('/') ?? "https://platform.higgsfield.ai";
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        _logger = logger;
    }

    public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, ImageGenerationOptions options, CancellationToken ct)
    {
        // Higgsfield uses "Authorization: Key KEY_ID:KEY_SECRET"
        var authValue = !string.IsNullOrEmpty(_apiKeySecret)
            ? $"{_apiKey}:{_apiKeySecret}"
            : _apiKey;

        var requestBody = new
        {
            prompt,
            aspect_ratio = GetAspectRatio(options.Width, options.Height),
            safety_tolerance = 2
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/flux-pro/kontext/max/text-to-image");
        request.Headers.Add("Authorization", $"Key {authValue}");
        request.Content = JsonContent.Create(requestBody);

        _logger?.LogInformation("Higgsfield: Submitting image generation request for prompt: {Prompt}", prompt.Length > 80 ? prompt[..80] + "..." : prompt);

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogWarning("Higgsfield: Initial request failed with {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Higgsfield API error {response.StatusCode}: {responseBody}");
        }

        var json = JsonDocument.Parse(responseBody).RootElement;

        // Check if response is already completed (synchronous)
        if (json.TryGetProperty("images", out var imagesImmediate) && imagesImmediate.GetArrayLength() > 0)
        {
            var imageUrl = imagesImmediate[0].GetProperty("url").GetString()!;
            _logger?.LogInformation("Higgsfield: Image generated immediately: {Url}", imageUrl[..Math.Min(80, imageUrl.Length)]);
            return new ImageGenerationResult(imageUrl, prompt);
        }

        // Async: poll for completion
        var requestId = json.TryGetProperty("request_id", out var reqId) ? reqId.GetString()
            : json.TryGetProperty("id", out var idProp) ? idProp.GetString()
            : null;

        if (requestId == null)
        {
            _logger?.LogWarning("Higgsfield: No request_id in response: {Body}", responseBody);
            throw new InvalidOperationException($"Higgsfield: No request_id in response: {responseBody}");
        }

        _logger?.LogInformation("Higgsfield: Polling for request {RequestId}...", requestId);

        // Poll status up to 60 times (3 minutes with 3s intervals)
        for (var i = 0; i < 60; i++)
        {
            await Task.Delay(3000, ct);

            var statusRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/requests/{requestId}/status");
            statusRequest.Headers.Add("Authorization", $"Key {authValue}");

            var statusResponse = await _httpClient.SendAsync(statusRequest, ct);
            var statusBody = await statusResponse.Content.ReadAsStringAsync(ct);

            if (!statusResponse.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Higgsfield: Status poll failed with {StatusCode}: {Body}", statusResponse.StatusCode, statusBody);
                continue;
            }

            var statusJson = JsonDocument.Parse(statusBody).RootElement;
            var status = statusJson.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;

            if (status == "completed")
            {
                if (statusJson.TryGetProperty("images", out var images) && images.GetArrayLength() > 0)
                {
                    var imageUrl = images[0].GetProperty("url").GetString()!;
                    _logger?.LogInformation("Higgsfield: Image ready after {Polls} polls: {Url}", i + 1, imageUrl[..Math.Min(80, imageUrl.Length)]);
                    return new ImageGenerationResult(imageUrl, prompt);
                }

                // Try alternate response format: results array
                if (statusJson.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    var url = firstResult.TryGetProperty("url", out var urlProp) ? urlProp.GetString()
                        : firstResult.TryGetProperty("raw", out var rawProp) && rawProp.TryGetProperty("url", out var rawUrl) ? rawUrl.GetString()
                        : null;
                    if (url != null)
                    {
                        _logger?.LogInformation("Higgsfield: Image ready (results format): {Url}", url[..Math.Min(80, url.Length)]);
                        return new ImageGenerationResult(url, prompt);
                    }
                }

                _logger?.LogWarning("Higgsfield: Completed but no image URL found in: {Body}", statusBody);
                throw new InvalidOperationException("Higgsfield completed but no image URL in response");
            }
            else if (status == "failed" || status == "error")
            {
                _logger?.LogWarning("Higgsfield: Generation failed: {Body}", statusBody);
                throw new InvalidOperationException($"Higgsfield image generation failed: {statusBody}");
            }

            // Still processing, continue polling
            if (i % 5 == 0)
                _logger?.LogInformation("Higgsfield: Still processing... (poll {Poll}, status: {Status})", i + 1, status);
        }

        throw new TimeoutException("Higgsfield image generation timed out after 3 minutes");
    }

    private static string GetAspectRatio(int width, int height)
    {
        if (width == height) return "1:1";
        if (width > height) return "16:9";
        return "9:16";
    }
}
