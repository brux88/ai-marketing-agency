using AiMarketingAgency.Domain.Interfaces;
using AiMarketingAgency.Domain.ValueObjects;

namespace AiMarketingAgency.Domain.Entities;

public class Project : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BrandVoice BrandVoice { get; set; } = new();
    public TargetAudience TargetAudience { get; set; } = new();
    public bool IsActive { get; set; } = true;

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<ContentSource> ContentSources { get; set; } = new List<ContentSource>();
    public ICollection<GeneratedContent> GeneratedContents { get; set; } = new List<GeneratedContent>();
    public ICollection<AgentJob> AgentJobs { get; set; } = new List<AgentJob>();
}
