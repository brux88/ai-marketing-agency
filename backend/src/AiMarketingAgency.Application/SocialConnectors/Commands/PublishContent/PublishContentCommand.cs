using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;

public record PublishContentCommand(Guid AgencyId, Guid ConnectorId, Guid ContentId) : IRequest<PublishResult>;
