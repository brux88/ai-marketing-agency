using AiMarketingAgency.Application.SocialConnectors.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.ConnectPlatform;

public record ConnectPlatformCommand : IRequest<SocialConnectorDto>
{
    public Guid AgencyId { get; init; }
    public Guid? ProjectId { get; init; }
    public SocialPlatform Platform { get; init; }
    public string AccessToken { get; init; } = string.Empty;
    public string? RefreshToken { get; init; }
    public string? AccountId { get; init; }
    public string? AccountName { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime? TokenExpiresAt { get; init; }
}
