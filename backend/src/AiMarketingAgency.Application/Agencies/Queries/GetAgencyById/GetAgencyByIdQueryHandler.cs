using AiMarketingAgency.Application.Agencies.Dtos;
using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agencies.Queries.GetAgencyById;

public class GetAgencyByIdQueryHandler : IRequestHandler<GetAgencyByIdQuery, AgencyDto?>
{
    private readonly IAppDbContext _context;

    public GetAgencyByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<AgencyDto?> Handle(GetAgencyByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Agencies
            .AsNoTracking()
            .Where(a => a.Id == request.Id && a.IsActive)
            .Select(a => new AgencyDto
            {
                Id = a.Id,
                Name = a.Name,
                ProductName = a.ProductName,
                Description = a.Description,
                WebsiteUrl = a.WebsiteUrl,
                LogoUrl = a.LogoUrl,
                BrandVoice = a.BrandVoice,
                TargetAudience = a.TargetAudience,
                DefaultLlmProviderKeyId = a.DefaultLlmProviderKeyId,
                ImageLlmProviderKeyId = a.ImageLlmProviderKeyId,
                ApprovalMode = a.ApprovalMode,
                AutoApproveMinScore = a.AutoApproveMinScore,
                EnableLogoOverlay = a.EnableLogoOverlay,
                LogoOverlayPosition = a.LogoOverlayPosition,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                ContentSourcesCount = a.ContentSources.Count(cs => cs.IsActive),
                GeneratedContentsCount = a.GeneratedContents.Count
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
