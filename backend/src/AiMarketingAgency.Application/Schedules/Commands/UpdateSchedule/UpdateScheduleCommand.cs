using AiMarketingAgency.Application.Schedules.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.Schedules.Commands.UpdateSchedule;

public record UpdateScheduleCommand : IRequest<ContentScheduleDto>
{
    public Guid Id { get; init; }
    public Guid AgencyId { get; init; }
    public Guid? ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DayOfWeekFlag Days { get; init; }
    public string TimeOfDay { get; init; } = "09:00";
    public string TimeZone { get; init; } = "Europe/Rome";
    public AgentType AgentType { get; init; }
    public string? Input { get; init; }
    public bool IsActive { get; init; }
}
