using System.Net.Http.Json;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Infrastructure.Ai.VideoGeneration;

public class HiggFieldVideoService : IVideoGenerationService
{
    private readonly string _apiKey;
    private readonly string? _apiKeySecret;
    private readonly string _baseUrl;
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(5) };

    public HiggFieldVideoService(string apiKey, string? apiKeySecret = null, string? baseUrl = null)
    {
        _apiKey = apiKey;
        _apiKeySecret = apiKeySecret;
        _baseUrl = baseUrl ?? "https://api.higgfield.ai/v1";
    }

    public async Task<VideoGenerationResult> GenerateVideoAsync(string prompt, VideoGenerationOptions options, CancellationToken ct)
    {
        // Step 1: Submit video generation task
        var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/videos/generate");
        submitRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        if (!string.IsNullOrEmpty(_apiKeySecret))
        {
            submitRequest.Headers.Add("X-Api-Secret", _apiKeySecret);
        }

        submitRequest.Content = JsonContent.Create(new
        {
            prompt,
            width = options.Width,
            height = options.Height,
            duration = options.DurationSeconds,
            style = options.Style ?? "natural"
        });

        var submitResponse = await _httpClient.SendAsync(submitRequest, ct);
        submitResponse.EnsureSuccessStatusCode();

        var submitJson = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(ct);

        // Check if response has a task_id (async) or direct video_url (sync)
        if (submitJson.TryGetProperty("video_url", out var directUrl))
        {
            return new VideoGenerationResult(
                directUrl.GetString()!,
                prompt,
                options.DurationSeconds);
        }

        // Async: poll for completion
        var taskId = submitJson.GetProperty("task_id").GetString()!;
        var maxAttempts = 60; // 5 minutes max

        for (int i = 0; i < maxAttempts; i++)
        {
            await Task.Delay(5000, ct); // Poll every 5 seconds

            var statusRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/videos/status/{taskId}");
            statusRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            if (!string.IsNullOrEmpty(_apiKeySecret))
            {
                statusRequest.Headers.Add("X-Api-Secret", _apiKeySecret);
            }

            var statusResponse = await _httpClient.SendAsync(statusRequest, ct);
            statusResponse.EnsureSuccessStatusCode();

            var statusJson = await statusResponse.Content.ReadFromJsonAsync<JsonElement>(ct);
            var status = statusJson.GetProperty("status").GetString();

            if (status == "completed")
            {
                var videoUrl = statusJson.GetProperty("video_url").GetString()!;
                var duration = statusJson.TryGetProperty("duration", out var dur) ? dur.GetInt32() : options.DurationSeconds;
                return new VideoGenerationResult(videoUrl, prompt, duration);
            }

            if (status == "failed")
            {
                var error = statusJson.TryGetProperty("error", out var err) ? err.GetString() : "Video generation failed";
                throw new InvalidOperationException($"Video generation failed: {error}");
            }
        }

        throw new TimeoutException("Video generation timed out after 5 minutes");
    }
}
