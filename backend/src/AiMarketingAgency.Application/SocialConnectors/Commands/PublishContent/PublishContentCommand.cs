using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;

public record PublishContentCommand(Guid AgencyId, Guid ContentId, SocialPlatform Platform) : IRequest<PublishResult>;
