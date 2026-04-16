using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Schedules.Commands.CreateSchedule;
using AiMarketingAgency.Application.Schedules.Commands.DeleteSchedule;
using AiMarketingAgency.Application.Schedules.Commands.UpdateSchedule;
using AiMarketingAgency.Application.Schedules.Dtos;
using AiMarketingAgency.Application.Schedules.Queries.GetSchedulesByAgency;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public SchedulesController(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ContentScheduleDto>>>> GetAll(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSchedulesByAgencyQuery(agencyId), ct);
        return Ok(ApiResponse<List<ContentScheduleDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ContentScheduleDto>>> Create(
        Guid agencyId, [FromBody] CreateScheduleCommand command, CancellationToken ct)
    {
        var cmd = command with { AgencyId = agencyId };
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetAll), new { agencyId }, ApiResponse<ContentScheduleDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContentScheduleDto>>> Update(
        Guid agencyId, Guid id, [FromBody] UpdateScheduleCommand command, CancellationToken ct)
    {
        var cmd = command with { Id = id, AgencyId = agencyId };
        var result = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<ContentScheduleDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid agencyId, Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteScheduleCommand(id, agencyId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPost("{id:guid}/run-now")]
    public async Task<ActionResult<ApiResponse<object>>> RunNow(Guid agencyId, Guid id, CancellationToken ct)
    {
        var schedule = await _context.ContentSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.AgencyId == agencyId, ct);
        if (schedule == null) return NotFound();

        schedule.NextRunAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(new { triggered = true }));
    }

    [HttpGet("{id:guid}/executions")]
    public async Task<ActionResult<ApiResponse<List<ScheduleExecutionDto>>>> GetExecutions(
        Guid agencyId, Guid id, CancellationToken ct)
    {
        var executions = await _context.AgentJobs
            .Where(j => j.AgencyId == agencyId && j.ScheduleId == id)
            .OrderByDescending(j => j.CreatedAt)
            .Take(20)
            .Select(j => new ScheduleExecutionDto
            {
                Id = j.Id,
                Status = j.Status.ToString(),
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt,
                CreatedAt = j.CreatedAt,
                ErrorMessage = j.ErrorMessage,
                Output = j.Output,
                GeneratedContents = j.GeneratedContents
                    .Select(c => new ScheduleExecutionContentDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Status = c.Status.ToString()
                    }).ToList()
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<ScheduleExecutionDto>>.Ok(executions));
    }
}

public class ScheduleExecutionDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Output { get; set; }
    public List<ScheduleExecutionContentDto> GeneratedContents { get; set; } = new();
}

public class ScheduleExecutionContentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
