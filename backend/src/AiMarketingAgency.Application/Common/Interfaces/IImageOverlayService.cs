using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IImageOverlayService
{
    Task<string> ApplyLogoOverlayAsync(string sourceImageUrl, string logoUrl, LogoPosition position, CancellationToken ct);
}
