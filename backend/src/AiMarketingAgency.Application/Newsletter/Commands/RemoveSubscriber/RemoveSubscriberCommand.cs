using MediatR;

namespace AiMarketingAgency.Application.Newsletter.Commands.RemoveSubscriber;

public record RemoveSubscriberCommand(Guid AgencyId, Guid SubscriberId) : IRequest;
