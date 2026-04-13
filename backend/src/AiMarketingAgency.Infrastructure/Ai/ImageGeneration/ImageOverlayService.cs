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
        var sourceBytes = await _httpClient.GetByteArrayAsync(sourceImageUrl, ct);
        var logoBytes = await _httpClient.GetByteArrayAsync(logoUrl, ct);

        using var sourceImage = Image.Load(sourceBytes);
        using var logoImage = Image.Load(logoBytes);

        var targetLogoWidth = (int)(sourceImage.Width * 0.15);
        var targetLogoHeight = (int)((double)targetLogoWidth / logoImage.Width * logoImage.Height);
        logoImage.Mutate(x => x.Resize(targetLogoWidth, targetLogoHeight));

        var padding = (int)(sourceImage.Width * 0.025);
        var point = position switch
        {
            LogoPosition.TopLeft => new Point(padding, padding),
            LogoPosition.TopRight => new Point(sourceImage.Width - targetLogoWidth - padding, padding),
            LogoPosition.BottomLeft => new Point(padding, sourceImage.Height - targetLogoHeight - padding),
            LogoPosition.BottomRight => new Point(sourceImage.Width - targetLogoWidth - padding, sourceImage.Height - targetLogoHeight - padding),
            _ => new Point(sourceImage.Width - targetLogoWidth - padding, sourceImage.Height - targetLogoHeight - padding)
        };

        sourceImage.Mutate(x => x.DrawImage(logoImage, point, 0.9f));

        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var outputDir = Path.Combine(webRoot, "generated-images");
        Directory.CreateDirectory(outputDir);
        var fileName = $"{Guid.NewGuid():N}.png";
        var outputPath = Path.Combine(outputDir, fileName);
        await sourceImage.SaveAsPngAsync(outputPath, ct);

        return $"/generated-images/{fileName}";
    }
}
