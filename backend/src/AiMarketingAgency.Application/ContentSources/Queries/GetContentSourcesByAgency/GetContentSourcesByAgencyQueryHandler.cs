using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.ContentSources.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.ContentSources.Queries.GetContentSourcesByAgency;

public class GetContentSourcesByAgencyQueryHandler : IRequestHandler<GetContentSourcesByAgencyQuery, List<ContentSourceDto>>
{
    private readonly IAppDbContext _context;

    public GetContentSourcesByAgencyQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContentSourceDto>> Handle(GetContentSourcesByAgencyQuery request, CancellationToken cancellationToken)
    {
        return await _context.ContentSources
            .AsNoTracking()
            .Where(cs => cs.AgencyId == request.AgencyId && cs.IsActive)
            .Select(cs => new ContentSourceDto
            {
                Id = cs.Id,
                AgencyId = cs.AgencyId,
                ProjectId = cs.ProjectId,
                Type = cs.Type,
                Url = cs.Url,
                Name = cs.Name,
                Config = cs.Config,
                IsActive = cs.IsActive,
                LastFetchedAt = cs.LastFetchedAt,
                CreatedAt = cs.CreatedAt,
            })
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
