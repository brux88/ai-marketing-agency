using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.SocialConnectors.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.SocialConnectors.Queries.GetConnectors;

public class GetConnectorsQueryHandler : IRequestHandler<GetConnectorsQuery, List<SocialConnectorDto>>
{
    private readonly IAppDbContext _context;

    public GetConnectorsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SocialConnectorDto>> Handle(GetConnectorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.SocialConnectors
            .Where(c => c.AgencyId == request.AgencyId)
            .OrderBy(c => c.ProjectId == null ? 0 : 1)
            .ThenBy(c => c.Project!.Name)
            .ThenBy(c => c.Platform)
            .Select(c => new SocialConnectorDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                ProjectName = c.Project != null ? c.Project.Name : null,
                Platform = c.Platform,
                AccountId = c.AccountId,
                AccountName = c.AccountName,
                ProfileImageUrl = c.ProfileImageUrl,
                IsActive = c.IsActive,
                TokenExpiresAt = c.TokenExpiresAt,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
