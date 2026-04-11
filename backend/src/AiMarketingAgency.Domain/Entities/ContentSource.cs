using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class ContentSource : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public ContentSourceType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Config { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastFetchedAt { get; set; }
    public Guid? ProjectId { get; set; }

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Project? Project { get; set; }
}
