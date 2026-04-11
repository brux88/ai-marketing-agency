using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Schedules.Commands.CreateSchedule;
using AiMarketingAgency.Application.Schedules.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Schedules.Commands.UpdateSchedule;

public class UpdateScheduleCommandHandler : IRequestHandler<UpdateScheduleCommand, ContentScheduleDto>
{
    private readonly IAppDbContext _context;

    public UpdateScheduleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContentScheduleDto> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.ContentSchedules
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Schedule not found.");

        var time = TimeOnly.Parse(request.TimeOfDay);

        schedule.Name = request.Name;
        schedule.ProjectId = request.ProjectId;
        schedule.Days = request.Days;
        schedule.TimeOfDay = time;
        schedule.TimeZone = request.TimeZone;
        schedule.AgentType = request.AgentType;
        schedule.Input = request.Input;
        schedule.IsActive = request.IsActive;
        schedule.NextRunAt = request.IsActive
            ? CreateScheduleCommandHandler.CalculateNextRun(request.Days, time, request.TimeZone)
            : null;

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
            LastRunAt = schedule.LastRunAt,
            NextRunAt = schedule.NextRunAt,
            CreatedAt = schedule.CreatedAt
        };
    }
}
