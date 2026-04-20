using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Projects.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IAppDbContext _context;

    public GetProjectByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.ProjectId && p.AgencyId == request.AgencyId && p.IsActive)
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
                CreatedAt = p.CreatedAt,
                ContentSourcesCount = p.ContentSources.Count(cs => cs.IsActive),
                GeneratedContentsCount = p.GeneratedContents.Count,
                BlogPromptTemplate = p.BlogPromptTemplate,
                SocialPromptTemplate = p.SocialPromptTemplate,
                NewsletterPromptTemplate = p.NewsletterPromptTemplate,
                ExtractedContext = p.ExtractedContext,
                ExtractedContextAt = p.ExtractedContextAt,
                ApprovalMode = p.ApprovalMode,
                AutoApproveMinScore = p.AutoApproveMinScore,
                AutoScheduleOnApproval = p.AutoScheduleOnApproval,
                EnableLogoOverlay = p.EnableLogoOverlay,
                LogoOverlayPosition = p.LogoOverlayPosition,
                LogoOverlayMode = p.LogoOverlayMode,
                BrandBannerColor = p.BrandBannerColor,
                EnabledSocialPlatforms = p.EnabledSocialPlatforms,
                NotifyEmailOnGeneration = p.NotifyEmailOnGeneration,
                NotifyEmailOnPublication = p.NotifyEmailOnPublication,
                NotifyEmailOnApprovalNeeded = p.NotifyEmailOnApprovalNeeded,
                NotificationEmail = p.NotificationEmail,
                NotifyPushOnGeneration = p.NotifyPushOnGeneration,
                NotifyPushOnPublication = p.NotifyPushOnPublication,
                NotifyPushOnApprovalNeeded = p.NotifyPushOnApprovalNeeded,
                NotifyEmailOnSubscribed = p.NotifyEmailOnSubscribed,
                NotifyPushOnSubscribed = p.NotifyPushOnSubscribed,
                NotifyTelegramOnSubscribed = p.NotifyTelegramOnSubscribed,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
