using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Projects.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Projects.Queries.GetProjectsByAgency;

public class GetProjectsByAgencyQueryHandler : IRequestHandler<GetProjectsByAgencyQuery, List<ProjectDto>>
{
    private readonly IAppDbContext _context;

    public GetProjectsByAgencyQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsByAgencyQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.AgencyId == request.AgencyId && p.IsActive)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                AgencyId = p.AgencyId,
                Name = p.Name,
                Description = p.Description,
                WebsiteUrl = p.WebsiteUrl,
                LogoUrl = p.LogoUrl,
                BrandVoice = p.BrandVoice,
                TargetAudience = p.TargetAudience,
                IsActive = p.IsActive,
                BlogPromptTemplate = p.BlogPromptTemplate,
                SocialPromptTemplate = p.SocialPromptTemplate,
                NewsletterPromptTemplate = p.NewsletterPromptTemplate,
                ExtractedContext = p.ExtractedContext,
                ExtractedContextAt = p.ExtractedContextAt,
                CreatedAt = p.CreatedAt,
                ContentSourcesCount = p.ContentSources.Count(cs => cs.IsActive),
                GeneratedContentsCount = p.GeneratedContents.Count,
                ApprovalMode = p.ApprovalMode,
                AutoApproveMinScore = p.AutoApproveMinScore
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
