namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IImageGenerationService
{
    Task<ImageGenerationResult> GenerateImageAsync(string prompt, ImageGenerationOptions options, CancellationToken ct);
}

public record ImageGenerationResult(string ImageUrl, string RevisedPrompt);
public record ImageGenerationOptions(int Width = 1024, int Height = 1024, string? Style = null);
