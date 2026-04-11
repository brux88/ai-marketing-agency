using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.SocialConnectors.Commands.ConnectPlatform;
using AiMarketingAgency.Application.SocialConnectors.Commands.DisconnectPlatform;
using AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;
using AiMarketingAgency.Application.SocialConnectors.Dtos;
using AiMarketingAgency.Application.SocialConnectors.Queries.GetConnectors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/connectors")]
[Authorize]
public class SocialConnectorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SocialConnectorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SocialConnectorDto>>>> GetAll(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetConnectorsQuery(agencyId), ct);
        return Ok(ApiResponse<List<SocialConnectorDto>>.Ok(result));
    }

    [HttpPost("connect")]
    public async Task<ActionResult<ApiResponse<SocialConnectorDto>>> Connect(
        Guid agencyId, [FromBody] ConnectPlatformCommand command, CancellationToken ct)
    {
        var cmd = command with { AgencyId = agencyId };
        var result = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<SocialConnectorDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Disconnect(Guid agencyId, Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DisconnectPlatformCommand(agencyId, id), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPost("{connectorId:guid}/publish/{contentId:guid}")]
    public async Task<ActionResult<ApiResponse<PublishResult>>> Publish(
        Guid agencyId, Guid connectorId, Guid contentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishContentCommand(agencyId, connectorId, contentId), ct);
        return Ok(ApiResponse<PublishResult>.Ok(result));
    }
}
