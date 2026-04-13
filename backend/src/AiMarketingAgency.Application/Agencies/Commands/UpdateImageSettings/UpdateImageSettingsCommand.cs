using MediatR;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateImageSettings;

public record UpdateImageSettingsCommand(
    Guid AgencyId,
    bool EnableLogoOverlay,
    int LogoOverlayPosition,
    string? LogoUrl) : IRequest;
