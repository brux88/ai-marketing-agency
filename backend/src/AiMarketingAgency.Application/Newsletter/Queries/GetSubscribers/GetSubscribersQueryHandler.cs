using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Newsletter.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Queries.GetSubscribers;

public class GetSubscribersQueryHandler : IRequestHandler<GetSubscribersQuery, List<SubscriberDto>>
{
    private readonly IAppDbContext _context;

    public GetSubscribersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriberDto>> Handle(GetSubscribersQuery request, CancellationToken cancellationToken)
    {
        return await _context.NewsletterSubscribers
            .Where(s => s.AgencyId == request.AgencyId)
            .OrderByDescending(s => s.SubscribedAt)
            .Select(s => new SubscriberDto
            {
                Id = s.Id,
                Email = s.Email,
                Name = s.Name,
                IsActive = s.IsActive,
                SubscribedAt = s.SubscribedAt
            })
            .ToListAsync(cancellationToken);
    }
}
