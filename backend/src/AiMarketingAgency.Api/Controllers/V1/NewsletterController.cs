using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Newsletter.Commands.AddSubscriber;
using AiMarketingAgency.Application.Newsletter.Commands.ConfigureEmail;
using AiMarketingAgency.Application.Newsletter.Commands.RemoveSubscriber;
using AiMarketingAgency.Application.Newsletter.Commands.SendNewsletter;
using AiMarketingAgency.Application.Newsletter.Dtos;
using AiMarketingAgency.Application.Newsletter.Queries.GetEmailConnector;
using AiMarketingAgency.Application.Newsletter.Queries.GetSubscribers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/newsletter")]
[Authorize]
public class NewsletterController : ControllerBase
{
    private readonly IMediator _mediator;

    public NewsletterController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Email connector config (list of connectors — one agency default + optional per-project)
    [HttpGet("config")]
    public async Task<ActionResult<ApiResponse<List<EmailConnectorDto>>>> GetConfig(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEmailConnectorQuery(agencyId), ct);
        return Ok(ApiResponse<List<EmailConnectorDto>>.Ok(result));
    }

    [HttpPost("config")]
    public async Task<ActionResult<ApiResponse<EmailConnectorDto>>> ConfigureEmail(
        Guid agencyId, [FromBody] ConfigureEmailCommand command, CancellationToken ct)
    {
        var cmd = command with { AgencyId = agencyId };
        var result = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<EmailConnectorDto>.Ok(result));
    }

    // Subscribers
    [HttpGet("subscribers")]
    public async Task<ActionResult<ApiResponse<List<SubscriberDto>>>> GetSubscribers(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscribersQuery(agencyId), ct);
        return Ok(ApiResponse<List<SubscriberDto>>.Ok(result));
    }

    [HttpPost("subscribers")]
    public async Task<ActionResult<ApiResponse<SubscriberDto>>> AddSubscriber(
        Guid agencyId, [FromBody] AddSubscriberRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddSubscriberCommand(agencyId, body.Email, body.Name), ct);
        return Ok(ApiResponse<SubscriberDto>.Ok(result));
    }

    [HttpDelete("subscribers/{subscriberId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveSubscriber(
        Guid agencyId, Guid subscriberId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveSubscriberCommand(agencyId, subscriberId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    // ── Project-level subscribers ──

    [HttpGet("projects/{projectId:guid}/subscribers")]
    public async Task<ActionResult<ApiResponse<List<SubscriberDto>>>> GetProjectSubscribers(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscribersQuery(agencyId, projectId), ct);
        return Ok(ApiResponse<List<SubscriberDto>>.Ok(result));
    }

    [HttpPost("projects/{projectId:guid}/subscribers")]
    public async Task<ActionResult<ApiResponse<SubscriberDto>>> AddProjectSubscriber(
        Guid agencyId, Guid projectId, [FromBody] AddSubscriberRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddSubscriberCommand(agencyId, body.Email, body.Name, projectId), ct);
        return Ok(ApiResponse<SubscriberDto>.Ok(result));
    }

    [HttpDelete("projects/{projectId:guid}/subscribers/{subscriberId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveProjectSubscriber(
        Guid agencyId, Guid projectId, Guid subscriberId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveSubscriberCommand(agencyId, subscriberId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    // Send newsletter
    [HttpPost("send/{contentId:guid}")]
    public async Task<ActionResult<ApiResponse<EmailSendResult>>> SendNewsletter(
        Guid agencyId, Guid contentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendNewsletterCommand(agencyId, contentId), ct);
        return Ok(ApiResponse<EmailSendResult>.Ok(result));
    }
}

public record AddSubscriberRequest(string Email, string? Name);
