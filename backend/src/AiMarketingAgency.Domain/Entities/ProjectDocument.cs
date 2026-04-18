using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class ProjectDocument : BaseEntity, ITenantScoped, ISoftDeletable
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ExtractedText { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
