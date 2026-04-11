using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Content.Commands.ApproveContent;
using AiMarketingAgency.Application.Content.Dtos;
using AiMarketingAgency.Application.Content.Queries.GetContent;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/content")]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ContentDto>>>> GetAll(
        Guid agencyId,
        [FromQuery] ContentType? type,
        [FromQuery] ContentStatus? status,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContentQuery(agencyId, type, status), ct);
        return Ok(ApiResponse<List<ContentDto>>.Ok(result));
    }

    [HttpPost("{contentId:guid}/approve")]
    public async Task<ActionResult> Approve(Guid contentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveContentCommand(contentId), ct);
        if (!result) return NotFound();
        return Ok(new { success = true });
    }
}
