using AiMarketingAgency.Application.Schedules.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.Schedules.Commands.CreateSchedule;

public record CreateScheduleCommand : IRequest<ContentScheduleDto>
{
    public Guid AgencyId { get; init; }
    public Guid? ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DayOfWeekFlag Days { get; init; } = DayOfWeekFlag.Weekdays;
    public string TimeOfDay { get; init; } = "09:00";
    public string TimeZone { get; init; } = "Europe/Rome";
    public ScheduleType ScheduleType { get; init; } = ScheduleType.Generation;
    public AgentType AgentType { get; init; }
    public int? PublishContentType { get; init; }
    public int? MaxPostsPerPlatform { get; init; }
    public string? Input { get; init; }
    public string? EnabledSocialPlatforms { get; init; }
    public ApprovalMode? ApprovalMode { get; init; }
    public int? AutoApproveMinScore { get; init; }
    public bool? AutoScheduleOnApproval { get; init; }
}
