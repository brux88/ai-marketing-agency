using AiMarketingAgency.Application.ContentSources.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.ContentSources.Queries.GetContentSourcesByAgency;

public record GetContentSourcesByAgencyQuery(Guid AgencyId) : IRequest<List<ContentSourceDto>>;
