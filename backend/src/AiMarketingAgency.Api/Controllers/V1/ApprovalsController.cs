using AiMarketingAgency.Application.Approvals.Commands.ApproveContent;
using AiMarketingAgency.Application.Approvals.Commands.RejectContent;
using AiMarketingAgency.Application.Approvals.Dtos;
using AiMarketingAgency.Application.Approvals.Queries.GetPendingApprovals;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/approvals")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public ApprovalsController(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
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

    [HttpDelete("{contentId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid agencyId, Guid contentId, CancellationToken ct)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == contentId && c.AgencyId == agencyId, ct);
        if (content == null) return NotFound();

        var calendarEntries = await _context.CalendarEntries
            .Where(e => e.ContentId == contentId)
            .ToListAsync(ct);
        _context.CalendarEntries.RemoveRange(calendarEntries);
        _context.GeneratedContents.Remove(content);
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<ApprovalHistoryDto>>>> GetHistory(
        Guid agencyId,
        [FromQuery] string? filter,
        [FromQuery] int take = 100,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var query = _context.GeneratedContents
            .Where(c => c.AgencyId == agencyId &&
                (c.Status == ContentStatus.Approved ||
                 c.Status == ContentStatus.Rejected ||
                 c.Status == ContentStatus.Published));

        if (string.Equals(filter, "approved", StringComparison.OrdinalIgnoreCase))
            query = query.Where(c => c.Status == ContentStatus.Approved || c.Status == ContentStatus.Published);
        else if (string.Equals(filter, "rejected", StringComparison.OrdinalIgnoreCase))
            query = query.Where(c => c.Status == ContentStatus.Rejected);

        var items = await query
            .Include(c => c.Project)
            .OrderByDescending(c => c.ApprovedAt ?? c.CreatedAt)
            .Take(take)
            .Select(c => new ApprovalHistoryDto
            {
                Id = c.Id,
                Title = c.Title,
                Body = c.Body,
                ContentType = (int)c.ContentType,
                Status = (int)c.Status,
                ProjectId = c.ProjectId,
                ProjectName = c.Project != null ? c.Project.Name : null,
                OverallScore = c.OverallScore,
                AutoApproved = c.AutoApproved,
                ImageUrl = c.ImageUrl,
                CreatedAt = c.CreatedAt,
                ApprovedAt = c.ApprovedAt,
                PublishedAt = c.PublishedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<ApprovalHistoryDto>>.Ok(items));
    }
}

public class ApprovalHistoryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int ContentType { get; set; }
    public int Status { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public decimal OverallScore { get; set; }
    public bool AutoApproved { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
