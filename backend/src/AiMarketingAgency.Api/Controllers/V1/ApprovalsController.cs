using AiMarketingAgency.Application.Approvals.Commands.ApproveContent;
using AiMarketingAgency.Application.Approvals.Commands.RejectContent;
using AiMarketingAgency.Application.Approvals.Dtos;
using AiMarketingAgency.Application.Approvals.Queries.GetPendingApprovals;
using AiMarketingAgency.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/approvals")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApprovalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PendingApprovalDto>>>> GetPending(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingApprovalsQuery(agencyId), ct);
        return Ok(ApiResponse<List<PendingApprovalDto>>.Ok(result));
    }

    [HttpPut("{contentId:guid}/approve")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(Guid agencyId, Guid contentId, CancellationToken ct)
    {
        await _mediator.Send(new ApproveContentCommand(contentId, agencyId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{contentId:guid}/reject")]
    public async Task<ActionResult<ApiResponse<object>>> Reject(Guid agencyId, Guid contentId, CancellationToken ct)
    {
        await _mediator.Send(new RejectContentCommand(contentId, agencyId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}
