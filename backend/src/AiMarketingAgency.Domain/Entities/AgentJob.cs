using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class AgentJob : BaseEntity, ITenantScoped
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public AgentType AgentType { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public string? Input { get; set; }
    public string? Output { get; set; }
    public int TokensUsed { get; set; }
    public decimal CostEstimate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ScheduleId { get; set; }
    public ImageGenerationMode ImageMode { get; set; } = ImageGenerationMode.Single;
    public int ImageCount { get; set; } = 1;

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Project? Project { get; set; }
    public ContentSchedule? Schedule { get; set; }
    public ICollection<GeneratedContent> GeneratedContents { get; set; } = new List<GeneratedContent>();
}
