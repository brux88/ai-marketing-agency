using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.Interfaces;

namespace AiMarketingAgency.Domain.Entities;

public class ContentSchedule : BaseEntity, ITenantScoped
{
    public Guid AgencyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DayOfWeekFlag Days { get; set; } = DayOfWeekFlag.Weekdays;
    public TimeOnly TimeOfDay { get; set; } = new(9, 0);
    public string TimeZone { get; set; } = "Europe/Rome";
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Generation;
    public AgentType AgentType { get; set; }
    public int? PublishContentType { get; set; }
    public int? MaxPostsPerPlatform { get; set; }
    public string? Input { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }

    // Per-schedule overrides (fallback to project/agency defaults when null)
    public string? EnabledSocialPlatforms { get; set; }
    public ApprovalMode? ApprovalMode { get; set; }
    public int? AutoApproveMinScore { get; set; }
    public bool? AutoScheduleOnApproval { get; set; }

    // Navigation
    public Agency Agency { get; set; } = null!;
    public Project? Project { get; set; }
}
