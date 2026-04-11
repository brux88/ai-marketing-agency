using AiMarketingAgency.Application.Newsletter.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Newsletter.Queries.GetSubscribers;

public record GetSubscribersQuery(Guid AgencyId) : IRequest<List<SubscriberDto>>;
