using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Projects.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateProjectCommandHandler(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        var project = new Project
        {
            AgencyId = request.AgencyId,
            TenantId = _tenantContext.TenantId,
            Name = request.Name,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            LogoUrl = request.LogoUrl,
            BrandVoice = request.BrandVoice ?? new(),
            TargetAudience = request.TargetAudience ?? new(),
        };

        _context.Projects.Add(project);
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
            ContentSourcesCount = 0,
            GeneratedContentsCount = 0
        };
    }
}
