using AiMarketingAgency.Application.Approvals.Commands.RejectContent;
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
        [FromQuery] Guid? projectId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContentQuery(agencyId, type, status, projectId), ct);
        return Ok(ApiResponse<List<ContentDto>>.Ok(result));
    }

    [HttpGet("{contentId:guid}")]
    public async Task<ActionResult<ApiResponse<ContentDto>>> GetById(
        Guid agencyId, Guid contentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContentQuery(agencyId), ct);
        var content = result.FirstOrDefault(c => c.Id == contentId);
        if (content == null) return NotFound();
        return Ok(ApiResponse<ContentDto>.Ok(content));
    }

    [HttpPut("{contentId:guid}")]
    public async Task<ActionResult<ApiResponse<ContentDto>>> Update(
        Guid agencyId, Guid contentId,
        [FromBody] UpdateContentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AiMarketingAgency.Application.Content.Commands.UpdateContent.UpdateContentCommand(
                agencyId, contentId, request.Title, request.Body), ct);
        return Ok(ApiResponse<ContentDto>.Ok(result));
    }

    [HttpPost("{contentId:guid}/approve")]
    public async Task<ActionResult> Approve(Guid agencyId, Guid contentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveContentCommand(contentId), ct);
        if (!result) return NotFound();
        return Ok(new { success = true });
    }

    [HttpPost("{contentId:guid}/reject")]
    public async Task<ActionResult> Reject(Guid agencyId, Guid contentId, CancellationToken ct)
    {
        await _mediator.Send(new RejectContentCommand(contentId, agencyId), ct);
        return Ok(new { success = true });
    }

    [HttpDelete("{contentId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid agencyId, Guid contentId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AiMarketingAgency.Application.Content.Commands.DeleteContent.DeleteContentCommand(
                agencyId, contentId), ct);
        if (!result) return NotFound();
        return Ok(ApiResponse<object>.Ok(null));
    }
}

public record UpdateContentRequest(string Title, string Body);
