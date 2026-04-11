using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class ImageOverlayService : IImageOverlayService
{
    private static readonly HttpClient _httpClient = new();

    public async Task<string> ApplyLogoOverlayAsync(string sourceImageUrl, string logoUrl, LogoPosition position, CancellationToken ct)
    {
        // Download both images
        var sourceBytes = await _httpClient.GetByteArrayAsync(sourceImageUrl, ct);
        var logoBytes = await _httpClient.GetByteArrayAsync(logoUrl, ct);

        using var sourceImage = Image.Load(sourceBytes);
        using var logoImage = Image.Load(logoBytes);

        // Resize logo to ~12% of source width
        var targetLogoWidth = (int)(sourceImage.Width * 0.12);
        var targetLogoHeight = (int)((double)targetLogoWidth / logoImage.Width * logoImage.Height);
        logoImage.Mutate(x => x.Resize(targetLogoWidth, targetLogoHeight));

        // Calculate position with padding
        var padding = (int)(sourceImage.Width * 0.02);
        var point = position switch
        {
            LogoPosition.TopLeft => new Point(padding, padding),
            LogoPosition.TopRight => new Point(sourceImage.Width - targetLogoWidth - padding, padding),
            LogoPosition.BottomLeft => new Point(padding, sourceImage.Height - targetLogoHeight - padding),
            LogoPosition.BottomRight => new Point(sourceImage.Width - targetLogoWidth - padding, sourceImage.Height - targetLogoHeight - padding),
            _ => new Point(sourceImage.Width - targetLogoWidth - padding, sourceImage.Height - targetLogoHeight - padding)
        };

        // Composite logo onto source
        sourceImage.Mutate(x => x.DrawImage(logoImage, point, 0.85f));

        // Save to temp file and return path
        var outputDir = Path.Combine(Path.GetTempPath(), "aimarketing", "images");
        Directory.CreateDirectory(outputDir);
        var outputPath = Path.Combine(outputDir, $"{Guid.NewGuid()}.png");
        await sourceImage.SaveAsPngAsync(outputPath, ct);

        return outputPath;
    }
}
