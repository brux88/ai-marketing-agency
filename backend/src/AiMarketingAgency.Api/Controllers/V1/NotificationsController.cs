using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IAppDbContext _context;

    public NotificationsController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<NotificationListDto>>> List(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var query = _context.Notifications.AsQueryable();
        if (unreadOnly) query = query.Where(n => !n.Read);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(Math.Clamp(take, 1, 200))
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                AgencyId = n.AgencyId,
                JobId = n.JobId,
                ProjectId = n.ProjectId,
                Type = n.Type,
                Title = n.Title,
                Body = n.Body,
                Link = n.Link,
                Read = n.Read,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync(ct);

        var unreadCount = await _context.Notifications.CountAsync(n => !n.Read, ct);

        return Ok(ApiResponse<NotificationListDto>.Ok(new NotificationListDto
        {
            Items = items,
            UnreadCount = unreadCount
        }));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(Guid id, CancellationToken ct)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notification == null) return NotFound();
        notification.Read = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllRead(CancellationToken ct)
    {
        var unread = await _context.Notifications.Where(n => !n.Read).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.Read = true;
            n.ReadAt = now;
        }
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }
}

public class NotificationListDto
{
    public List<NotificationDto> Items { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public Guid? JobId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? Link { get; set; }
    public bool Read { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
