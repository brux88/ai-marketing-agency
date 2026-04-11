using AiMarketingAgency.Application.Agents.Commands.RunAgent;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("content-writer/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunContentWriter(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(new RunAgentCommand(agencyId, AgentType.ContentWriter, request?.Input, request?.ProjectId), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    [HttpPost("social-manager/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunSocialManager(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(new RunAgentCommand(agencyId, AgentType.SocialManager, request?.Input, request?.ProjectId), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    [HttpPost("newsletter/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunNewsletter(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(new RunAgentCommand(agencyId, AgentType.Newsletter, request?.Input, request?.ProjectId), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    [HttpPost("analytics/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunAnalytics(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(new RunAgentCommand(agencyId, AgentType.Analytics, request?.Input, request?.ProjectId), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }
}

public record RunAgentRequest(string? Input, Guid? ProjectId = null);
