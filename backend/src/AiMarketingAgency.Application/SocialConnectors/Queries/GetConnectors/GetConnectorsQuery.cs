using AiMarketingAgency.Application.SocialConnectors.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.SocialConnectors.Queries.GetConnectors;

public record GetConnectorsQuery(Guid AgencyId) : IRequest<List<SocialConnectorDto>>;
