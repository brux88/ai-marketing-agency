using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Schedules.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;

namespace AiMarketingAgency.Application.Schedules.Commands.CreateSchedule;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, ContentScheduleDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateScheduleCommandHandler(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ContentScheduleDto> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var time = TimeOnly.Parse(request.TimeOfDay);

        var schedule = new ContentSchedule
        {
            TenantId = _tenantContext.TenantId,
            AgencyId = request.AgencyId,
            ProjectId = request.ProjectId,
            Name = request.Name,
            Days = request.Days,
            TimeOfDay = time,
            TimeZone = request.TimeZone,
            AgentType = request.AgentType,
            Input = request.Input,
            NextRunAt = CalculateNextRun(request.Days, time, request.TimeZone)
        };

        _context.ContentSchedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContentScheduleDto
        {
            Id = schedule.Id,
            AgencyId = schedule.AgencyId,
            ProjectId = schedule.ProjectId,
            Name = schedule.Name,
            Days = schedule.Days,
            TimeOfDay = schedule.TimeOfDay.ToString("HH:mm"),
            TimeZone = schedule.TimeZone,
            AgentType = schedule.AgentType,
            Input = schedule.Input,
            IsActive = schedule.IsActive,
            NextRunAt = schedule.NextRunAt,
            CreatedAt = schedule.CreatedAt
        };
    }

    public static DateTime? CalculateNextRun(Domain.Enums.DayOfWeekFlag days, TimeOnly time, string timeZoneId)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            for (int i = 0; i < 8; i++)
            {
                var candidate = nowLocal.Date.AddDays(i);
                var dayFlag = candidate.DayOfWeek switch
                {
                    System.DayOfWeek.Monday => Domain.Enums.DayOfWeekFlag.Monday,
                    System.DayOfWeek.Tuesday => Domain.Enums.DayOfWeekFlag.Tuesday,
                    System.DayOfWeek.Wednesday => Domain.Enums.DayOfWeekFlag.Wednesday,
                    System.DayOfWeek.Thursday => Domain.Enums.DayOfWeekFlag.Thursday,
                    System.DayOfWeek.Friday => Domain.Enums.DayOfWeekFlag.Friday,
                    System.DayOfWeek.Saturday => Domain.Enums.DayOfWeekFlag.Saturday,
                    System.DayOfWeek.Sunday => Domain.Enums.DayOfWeekFlag.Sunday,
                    _ => Domain.Enums.DayOfWeekFlag.None
                };

                if (!days.HasFlag(dayFlag)) continue;

                var candidateDateTime = candidate.Add(time.ToTimeSpan());
                if (i == 0 && candidateDateTime <= nowLocal) continue;

                var candidateUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(candidateDateTime, DateTimeKind.Unspecified), tz);
                return candidateUtc;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
