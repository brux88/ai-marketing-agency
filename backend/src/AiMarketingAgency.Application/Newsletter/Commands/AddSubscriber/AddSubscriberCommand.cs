using AiMarketingAgency.Application.Newsletter.Dtos;
using MediatR;

namespace AiMarketingAgency.Application.Newsletter.Commands.AddSubscriber;

public record AddSubscriberCommand(Guid AgencyId, string Email, string? Name, Guid? ProjectId = null) : IRequest<SubscriberDto>;
