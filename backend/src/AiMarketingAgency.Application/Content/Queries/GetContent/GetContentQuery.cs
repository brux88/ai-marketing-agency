using AiMarketingAgency.Application.Content.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.Content.Queries.GetContent;

public record GetContentQuery(Guid AgencyId, ContentType? TypeFilter = null, ContentStatus? StatusFilter = null) : IRequest<List<ContentDto>>;
