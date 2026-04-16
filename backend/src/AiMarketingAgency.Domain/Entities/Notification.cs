using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class Notification : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? JobId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Type { get; set; } = string.Empty; // job.completed, job.failed, content.generated, publish.success, etc.
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? Link { get; set; }
    public bool Read { get; set; }
    public DateTime? ReadAt { get; set; }
}
