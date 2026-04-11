using AiMarketingAgency.Application.Projects.Dtos;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;

namespace AiMarketingAgency.Application.Projects.Commands.UpdateProject;

public record UpdateProjectCommand : IRequest<ProjectDto>
{
    public Guid ProjectId { get; init; }
    public Guid AgencyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? LogoUrl { get; init; }
    public BrandVoice? BrandVoice { get; init; }
    public TargetAudience? TargetAudience { get; init; }
}
