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
        var jobId = await _mediator.Send(BuildCommand(agencyId, AgentType.ContentWriter, request), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    [HttpPost("social-manager/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunSocialManager(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(BuildCommand(agencyId, AgentType.SocialManager, request), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    [HttpPost("newsletter/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunNewsletter(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(BuildCommand(agencyId, AgentType.Newsletter, request), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    [HttpPost("analytics/run")]
    public async Task<ActionResult<ApiResponse<object>>> RunAnalytics(Guid agencyId, [FromBody] RunAgentRequest? request, CancellationToken ct)
    {
        var jobId = await _mediator.Send(BuildCommand(agencyId, AgentType.Analytics, request), ct);
        return Ok(ApiResponse<object>.Ok(new { jobId }));
    }

    private static RunAgentCommand BuildCommand(Guid agencyId, AgentType type, RunAgentRequest? request) =>
        new(
            agencyId,
            type,
            request?.Input,
            request?.ProjectId,
            request?.ImageMode ?? ImageGenerationMode.Single,
            request?.ImageCount ?? 1);
}

public record RunAgentRequest(
    string? Input,
    Guid? ProjectId = null,
    ImageGenerationMode ImageMode = ImageGenerationMode.Single,
    int ImageCount = 1);
