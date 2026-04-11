using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Schedules.Commands.CreateSchedule;
using AiMarketingAgency.Application.Schedules.Commands.DeleteSchedule;
using AiMarketingAgency.Application.Schedules.Commands.UpdateSchedule;
using AiMarketingAgency.Application.Schedules.Dtos;
using AiMarketingAgency.Application.Schedules.Queries.GetSchedulesByAgency;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulesController(IMediator mediator)
    {
        _mediator = mediator;
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
}
