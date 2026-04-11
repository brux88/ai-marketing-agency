using MediatR;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.DisconnectPlatform;

public record DisconnectPlatformCommand(Guid AgencyId, Guid ConnectorId) : IRequest;
