using AiMarketingAgency.Application.Agencies.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Agencies.Queries.GetAgencyById;

public record GetAgencyByIdQuery(Guid Id) : IRequest<AgencyDto?>;
