using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public CalendarController(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("projects/{projectId:guid}/calendar")]
    public async Task<ActionResult<ApiResponse<List<CalendarEntryDto>>>> GetProjectCalendar(
        Guid agencyId,
        Guid projectId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var query = _context.CalendarEntries
            .Where(e => e.AgencyId == agencyId && e.Content.ProjectId == projectId);

        if (from.HasValue) query = query.Where(e => e.ScheduledAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.ScheduledAt <= to.Value);

        var items = await query
            .OrderBy(e => e.ScheduledAt)
            .Select(e => new CalendarEntryDto
            {
                Id = e.Id,
                ContentId = e.ContentId,
                ContentTitle = e.Content.Title,
                ContentType = e.Content.ContentType.ToString(),
                Platform = e.Platform.HasValue ? e.Platform.Value.ToString() : null,
                ScheduledAt = e.ScheduledAt,
                PublishedAt = e.PublishedAt,
                Status = e.Status.ToString(),
                ErrorMessage = e.ErrorMessage,
                PostUrl = e.PostUrl
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<CalendarEntryDto>>.Ok(items));
    }

    [HttpPost("content/{contentId:guid}/schedule")]
    public async Task<ActionResult<ApiResponse<CalendarEntryDto>>> ScheduleContent(
        Guid agencyId,
        Guid contentId,
        [FromBody] ScheduleContentRequest request,
        CancellationToken ct = default)
    {
        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == contentId && c.AgencyId == agencyId, ct)
            ?? throw new KeyNotFoundException("Content not found.");

        if (request.Platform.HasValue)
        {
            var alreadyExists = await _context.CalendarEntries
                .AnyAsync(e => e.ContentId == contentId
                               && e.Platform == request.Platform.Value
                               && e.Status != CalendarEntryStatus.Failed, ct);
            if (alreadyExists)
                return BadRequest(ApiResponse<object>.Fail($"Esiste già un evento per {request.Platform.Value}."));
        }

        var entry = new EditorialCalendarEntry
        {
            AgencyId = agencyId,
            TenantId = content.TenantId,
            ContentId = contentId,
            Platform = request.Platform,
            ScheduledAt = request.ScheduledAt,
            Status = CalendarEntryStatus.Scheduled
        };
        _context.CalendarEntries.Add(entry);
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<CalendarEntryDto>.Ok(new CalendarEntryDto
        {
            Id = entry.Id,
            ContentId = entry.ContentId,
            ContentTitle = content.Title,
            ContentType = content.ContentType.ToString(),
            Platform = entry.Platform?.ToString(),
            ScheduledAt = entry.ScheduledAt,
            PublishedAt = entry.PublishedAt,
            Status = entry.Status.ToString()
        }));
    }

    [HttpPost("calendar/{entryId:guid}/publish-now")]
    public async Task<ActionResult<ApiResponse<object>>> PublishNow(
        Guid agencyId,
        Guid entryId,
        CancellationToken ct = default)
    {
        var entry = await _context.CalendarEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.AgencyId == agencyId, ct)
            ?? throw new KeyNotFoundException("Calendar entry not found.");

        if (entry.Platform == null)
            return BadRequest(ApiResponse<object>.Fail("Calendar entry has no platform set."));

        try
        {
            var result = await _mediator.Send(
                new PublishContentCommand(agencyId, entry.ContentId, entry.Platform.Value),
                ct);

            entry.Status = result.Success ? CalendarEntryStatus.Published : CalendarEntryStatus.Failed;
            entry.PublishedAt = result.Success ? DateTime.UtcNow : null;
            entry.ErrorMessage = result.Success ? null : result.Error;
            entry.PostUrl = result.Success ? result.PostUrl : entry.PostUrl;
            await _context.SaveChangesAsync(ct);

            return Ok(ApiResponse<object>.Ok(new { success = result.Success, message = result.Error, postUrl = result.PostUrl }));
        }
        catch (Exception ex)
        {
            entry.Status = CalendarEntryStatus.Failed;
            entry.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(new { success = false, message = ex.Message }));
        }
    }

    [HttpDelete("calendar/{entryId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEntry(
        Guid agencyId, Guid entryId, CancellationToken ct = default)
    {
        var entry = await _context.CalendarEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.AgencyId == agencyId, ct);
        if (entry == null) return NotFound();
        _context.CalendarEntries.Remove(entry);
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}

public class CalendarEntryDto
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public string ContentTitle { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? PostUrl { get; set; }
}

public record ScheduleContentRequest(DateTime ScheduledAt, SocialPlatform? Platform);
