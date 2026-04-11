using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Projects.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IAppDbContext _context;

    public UpdateProjectCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.ContentSources)
            .Include(p => p.GeneratedContents)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.AgencyId == request.AgencyId && p.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        project.Name = request.Name;
        project.Description = request.Description;
        project.WebsiteUrl = request.WebsiteUrl;
        project.LogoUrl = request.LogoUrl;
        project.BrandVoice = request.BrandVoice ?? project.BrandVoice;
        project.TargetAudience = request.TargetAudience ?? project.TargetAudience;

        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto
        {
            Id = project.Id,
            AgencyId = project.AgencyId,
            Name = project.Name,
            Description = project.Description,
            WebsiteUrl = project.WebsiteUrl,
            LogoUrl = project.LogoUrl,
            BrandVoice = project.BrandVoice,
            TargetAudience = project.TargetAudience,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt,
            ContentSourcesCount = project.ContentSources.Count(cs => cs.IsActive),
            GeneratedContentsCount = project.GeneratedContents.Count
        };
    }
}
