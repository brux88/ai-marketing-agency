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

    // Email connector config
    [HttpGet("config")]
    public async Task<ActionResult<ApiResponse<EmailConnectorDto?>>> GetConfig(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEmailConnectorQuery(agencyId), ct);
        return Ok(ApiResponse<EmailConnectorDto?>.Ok(result));
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
