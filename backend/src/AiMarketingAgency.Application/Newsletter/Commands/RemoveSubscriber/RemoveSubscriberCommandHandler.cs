using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Commands.RemoveSubscriber;

public class RemoveSubscriberCommandHandler : IRequestHandler<RemoveSubscriberCommand>
{
    private readonly IAppDbContext _context;

    public RemoveSubscriberCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _context.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.Id == request.SubscriberId && s.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscriber not found.");

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
