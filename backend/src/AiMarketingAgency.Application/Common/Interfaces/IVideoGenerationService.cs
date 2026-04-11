namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IVideoGenerationService
{
    Task<VideoGenerationResult> GenerateVideoAsync(string prompt, VideoGenerationOptions options, CancellationToken ct);
}

public record VideoGenerationResult(string VideoUrl, string RevisedPrompt, int DurationSeconds);

public record VideoGenerationOptions(
    int Width = 1280,
    int Height = 720,
    int DurationSeconds = 15,
    string? Style = null);
