using AiMarketingAgency.Application.Agencies.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Queries.GetAgencies;

public record GetAgenciesQuery : IRequest<List<AgencyDto>>;
