using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Newsletter.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Commands.AddSubscriber;

public class AddSubscriberCommandHandler : IRequestHandler<AddSubscriberCommand, SubscriberDto>
{
    private readonly IAppDbContext _context;

    public AddSubscriberCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriberDto> Handle(AddSubscriberCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.AgencyId == request.AgencyId && s.Email == request.Email, cancellationToken);

        if (existing != null)
        {
            existing.Name = request.Name ?? existing.Name;
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
        }
        else
        {
            existing = new NewsletterSubscriber
            {
                AgencyId = request.AgencyId,
                Email = request.Email,
                Name = request.Name,
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            };
            _context.NewsletterSubscribers.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriberDto
        {
            Id = existing.Id,
            Email = existing.Email,
            Name = existing.Name,
            IsActive = existing.IsActive,
            SubscribedAt = existing.SubscribedAt
        };
    }
}
