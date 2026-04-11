using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Application.Projects.Dtos;

public class ProjectDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BrandVoice BrandVoice { get; set; } = new();
    public TargetAudience TargetAudience { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ContentSourcesCount { get; set; }
    public int GeneratedContentsCount { get; set; }
}
