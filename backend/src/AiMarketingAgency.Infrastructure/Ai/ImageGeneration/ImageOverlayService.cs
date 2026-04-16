using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = System.IO.Path;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class ImageOverlayService : IImageOverlayService
{
    private static readonly HttpClient _httpClient = new();
    private readonly ILogger<ImageOverlayService> _logger;
    private readonly IConfiguration _configuration;

    public ImageOverlayService(ILogger<ImageOverlayService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> ApplyLogoOverlayAsync(string sourceImageUrl, string logoUrl, LogoPosition position, CancellationToken ct)
    {
        _logger.LogInformation(
            "Applying logo overlay: source={Source}, logo={Logo}, position={Position}",
            sourceImageUrl, logoUrl, position);

        var sourceBytes = await LoadImageBytesAsync(sourceImageUrl, ct);
        var logoBytes = await LoadImageBytesAsync(logoUrl, ct);

        using var sourceImage = Image.Load<Rgba32>(sourceBytes);
        using var logoImage = Image.Load<Rgba32>(logoBytes);

        var targetLogoWidth = (int)(sourceImage.Width * 0.22);
        var targetLogoHeight = (int)((double)targetLogoWidth / logoImage.Width * logoImage.Height);
        logoImage.Mutate(x => x.Resize(targetLogoWidth, targetLogoHeight));

        var padding = (int)(sourceImage.Width * 0.03);
        var point = position switch
        {
            LogoPosition.TopLeft => new Point(padding, padding),
            LogoPosition.TopRight => new Point(sourceImage.Width - targetLogoWidth - padding, padding),
            LogoPosition.BottomLeft => new Point(padding, sourceImage.Height - targetLogoHeight - padding),
            LogoPosition.BottomRight => new Point(sourceImage.Width - targetLogoWidth - padding, sourceImage.Height - targetLogoHeight - padding),
            _ => new Point(sourceImage.Width - targetLogoWidth - padding, sourceImage.Height - targetLogoHeight - padding)
        };

        var ovalPaddingX = (int)(targetLogoWidth * 0.35);
        var ovalPaddingY = (int)(targetLogoHeight * 0.45);
        var centerX = point.X + targetLogoWidth / 2f;
        var centerY = point.Y + targetLogoHeight / 2f;
        var radiusX = targetLogoWidth / 2f + ovalPaddingX;
        var radiusY = targetLogoHeight / 2f + ovalPaddingY;
        var ovalPath = new EllipsePolygon(centerX, centerY, radiusX, radiusY);

        var avgBrightness = SampleAverageBrightness(sourceImage, point, targetLogoWidth, targetLogoHeight);
        var isDark = avgBrightness <= 128;

        // Outer soft glow
        using var glowLayer = new Image<Rgba32>(sourceImage.Width, sourceImage.Height, new Rgba32(0, 0, 0, 0));
        var outerColor = isDark ? new Rgba32(255, 255, 255, 200) : new Rgba32(0, 0, 0, 200);
        glowLayer.Mutate(x => x.Fill(outerColor, ovalPath));
        glowLayer.Mutate(x => x.GaussianBlur(20));
        sourceImage.Mutate(x => x.DrawImage(glowLayer, new Point(0, 0), 1f));

        // Inner solid pill for strong contrast
        var innerPath = new EllipsePolygon(centerX, centerY, radiusX * 0.8f, radiusY * 0.8f);
        using var pillLayer = new Image<Rgba32>(sourceImage.Width, sourceImage.Height, new Rgba32(0, 0, 0, 0));
        var pillColor = isDark ? new Rgba32(255, 255, 255, 220) : new Rgba32(0, 0, 0, 220);
        pillLayer.Mutate(x => x.Fill(pillColor, innerPath));
        pillLayer.Mutate(x => x.GaussianBlur(6));
        sourceImage.Mutate(x => x.DrawImage(pillLayer, new Point(0, 0), 0.85f));

        sourceImage.Mutate(x => x.DrawImage(logoImage, point, 1f));

        return await SaveOverlayResult(sourceImage, ct);
    }

    public async Task<string> ApplyBrandBannerAsync(string sourceImageUrl, string logoUrl, string bannerHexColor, CancellationToken ct)
    {
        _logger.LogInformation(
            "Applying brand banner: source={Source}, logo={Logo}, color={Color}",
            sourceImageUrl, logoUrl, bannerHexColor);

        var sourceBytes = await LoadImageBytesAsync(sourceImageUrl, ct);
        var logoBytes = await LoadImageBytesAsync(logoUrl, ct);

        using var sourceImage = Image.Load<Rgba32>(sourceBytes);
        using var logoImage = Image.Load<Rgba32>(logoBytes);

        var bannerHeight = (int)(sourceImage.Height * 0.12);
        var finalHeight = sourceImage.Height + bannerHeight;

        var parsedColor = ParseHexColor(bannerHexColor) ?? new Rgba32(0, 51, 102, 255);

        using var canvas = new Image<Rgba32>(sourceImage.Width, finalHeight, parsedColor);
        canvas.Mutate(ctx => ctx.DrawImage(sourceImage, new Point(0, 0), 1f));

        var logoTargetHeight = (int)(bannerHeight * 0.7);
        var logoTargetWidth = (int)((double)logoTargetHeight / logoImage.Height * logoImage.Width);
        logoImage.Mutate(x => x.Resize(logoTargetWidth, logoTargetHeight));

        var logoX = (int)(bannerHeight * 0.3);
        var logoY = sourceImage.Height + (bannerHeight - logoTargetHeight) / 2;
        canvas.Mutate(ctx => ctx.DrawImage(logoImage, new Point(logoX, logoY), 1f));

        return await SaveOverlayResult(canvas, ct);
    }

    private static IPath BuildRoundedRect(float x, float y, float width, float height, float radius)
    {
        radius = Math.Min(radius, Math.Min(width, height) / 2f);
        var builder = new PathBuilder();
        builder.AddArc(new PointF(x + radius, y + radius), radius, radius, 0, 180, 270);
        builder.AddArc(new PointF(x + width - radius, y + radius), radius, radius, 0, 270, 360);
        builder.AddArc(new PointF(x + width - radius, y + height - radius), radius, radius, 0, 0, 90);
        builder.AddArc(new PointF(x + radius, y + height - radius), radius, radius, 0, 90, 180);
        builder.CloseFigure();
        return builder.Build();
    }

    private static byte SampleAverageBrightness(Image<Rgba32> image, Point topLeft, int width, int height)
    {
        long totalBrightness = 0;
        int samples = 0;
        var step = Math.Max(1, Math.Min(width, height) / 10);

        for (int py = Math.Max(0, topLeft.Y); py < Math.Min(image.Height, topLeft.Y + height); py += step)
        {
            for (int px = Math.Max(0, topLeft.X); px < Math.Min(image.Width, topLeft.X + width); px += step)
            {
                var pixel = image[px, py];
                totalBrightness += (pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000;
                samples++;
            }
        }

        return samples > 0 ? (byte)(totalBrightness / samples) : (byte)128;
    }

    private async Task<string> SaveOverlayResult(Image image, CancellationToken ct)
    {
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var outputDir = Path.Combine(webRoot, "generated-images");
        Directory.CreateDirectory(outputDir);
        var fileName = $"{Guid.NewGuid():N}.png";
        var outputPath = Path.Combine(outputDir, fileName);
        await image.SaveAsPngAsync(outputPath, ct);

        var publicBaseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/');
        var result = !string.IsNullOrWhiteSpace(publicBaseUrl)
            ? $"{publicBaseUrl}/generated-images/{fileName}"
            : $"/generated-images/{fileName}";
        _logger.LogInformation("Overlay applied successfully → {Result}", result);
        return result;
    }

    private static Rgba32? ParseHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        var cleaned = hex.TrimStart('#');
        if (cleaned.Length != 6 && cleaned.Length != 8) return null;
        try
        {
            var r = Convert.ToByte(cleaned.Substring(0, 2), 16);
            var g = Convert.ToByte(cleaned.Substring(2, 2), 16);
            var b = Convert.ToByte(cleaned.Substring(4, 2), 16);
            var a = cleaned.Length == 8 ? Convert.ToByte(cleaned.Substring(6, 2), 16) : (byte)255;
            return new Rgba32(r, g, b, a);
        }
        catch
        {
            return null;
        }
    }

    private async Task<byte[]> LoadImageBytesAsync(string urlOrPath, CancellationToken ct)
    {
        if (Uri.TryCreate(urlOrPath, UriKind.Absolute, out var abs)
            && (abs.Scheme == "http" || abs.Scheme == "https"))
        {
            return await _httpClient.GetByteArrayAsync(abs, ct);
        }

        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relative = urlOrPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var localPath = Path.Combine(webRoot, relative);
        if (File.Exists(localPath))
        {
            _logger.LogDebug("Resolved {Url} → local file {Path}", urlOrPath, localPath);
            return await File.ReadAllBytesAsync(localPath, ct);
        }

        _logger.LogWarning(
            "Could not resolve {Url} as absolute URL or local wwwroot file, attempting direct fetch",
            urlOrPath);
        return await _httpClient.GetByteArrayAsync(urlOrPath, ct);
    }
}
