using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Schedules.Dtos;

public class ContentScheduleDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string Name { get; set; } = string.Empty;
    public DayOfWeekFlag Days { get; set; }
    public string TimeOfDay { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public AgentType AgentType { get; set; }
    public string? Input { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
