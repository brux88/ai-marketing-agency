using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using AiMarketingAgency.Application.Agencies.Commands.CreateAgency;
using AiMarketingAgency.Application.Agencies.Commands.UpdateApprovalMode;
using AiMarketingAgency.Application.Agencies.Commands.UpdateBrandVoice;
using AiMarketingAgency.Application.Agencies.Commands.UpdateDefaultLlm;
using AiMarketingAgency.Application.Agencies.Commands.UpdateImageSettings;
using AiMarketingAgency.Application.Agencies.Commands.UpdateTargetAudience;
using AiMarketingAgency.Application.Agencies.Dtos;
using AiMarketingAgency.Application.Agencies.Queries.GetAgencies;
using AiMarketingAgency.Application.Agencies.Queries.GetAgencyById;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AgenciesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public AgenciesController(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgencyDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAgenciesQuery(), ct);
        return Ok(ApiResponse<List<AgencyDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAgencyByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(ApiResponse<AgencyDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AgencyDto>>> Create([FromBody] CreateAgencyCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AgencyDto>.Ok(result));
    }

    [HttpPut("{id:guid}/brand-voice")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateBrandVoice(Guid id, [FromBody] UpdateBrandVoiceRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateBrandVoiceCommand(id, request.BrandVoice), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/target-audience")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTargetAudience(Guid id, [FromBody] UpdateTargetAudienceRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateTargetAudienceCommand(id, request.TargetAudience), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/approval-mode")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateApprovalMode(Guid id, [FromBody] UpdateApprovalModeRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateApprovalModeCommand(id, request.ApprovalMode, request.AutoApproveMinScore, request.AutoScheduleOnApproval), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/default-llm")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDefaultLlm(Guid id, [FromBody] UpdateDefaultLlmRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDefaultLlmCommand(id, request.DefaultLlmProviderKeyId, request.ImageLlmProviderKeyId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{id:guid}/image-settings")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateImageSettings(Guid id, [FromBody] UpdateImageSettingsRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateImageSettingsCommand(id, request.EnableLogoOverlay, request.LogoOverlayPosition, request.LogoUrl, request.LogoOverlayMode, request.BrandBannerColor), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpGet("{id:guid}/cost-stats")]
    public async Task<ActionResult<ApiResponse<AgencyCostStatsDto>>> GetCostStats(Guid id, CancellationToken ct)
    {
        // Include soft-deleted content: costs were already incurred.
        var contents = await _context.GeneratedContents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.AgencyId == id)
            .Select(c => new { c.AiGenerationCostUsd, c.AiImageCostUsd, c.CreatedAt, c.ProjectId })
            .ToListAsync(ct);

        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.AgencyId == id && p.IsActive)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(ct);

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var byProject = contents
            .Where(c => c.ProjectId.HasValue)
            .GroupBy(c => c.ProjectId!.Value)
            .Select(g => new ProjectCostBreakdown
            {
                ProjectId = g.Key,
                ProjectName = projects.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "—",
                Contents = g.Count(),
                TextCostUsd = g.Sum(x => x.AiGenerationCostUsd ?? 0m),
                ImageCostUsd = g.Sum(x => x.AiImageCostUsd ?? 0m)
            })
            .OrderByDescending(b => b.TextCostUsd + b.ImageCostUsd)
            .ToList();

        return Ok(ApiResponse<AgencyCostStatsDto>.Ok(new AgencyCostStatsDto
        {
            TotalContents = contents.Count,
            TotalTextCostUsd = contents.Sum(c => c.AiGenerationCostUsd ?? 0m),
            TotalImageCostUsd = contents.Sum(c => c.AiImageCostUsd ?? 0m),
            Last30DaysTextCostUsd = contents.Where(c => c.CreatedAt >= cutoff).Sum(c => c.AiGenerationCostUsd ?? 0m),
            Last30DaysImageCostUsd = contents.Where(c => c.CreatedAt >= cutoff).Sum(c => c.AiImageCostUsd ?? 0m),
            Projects = byProject
        }));
    }

    [HttpPost("{id:guid}/reset-analytics")]
    public async Task<ActionResult<ApiResponse<object>>> ResetAnalytics(
        Guid id, [FromBody] ResetAnalyticsRequest request, CancellationToken ct)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agency == null) return NotFound();

        const string adminPassword = "Test123!";
        if (request.Password != adminPassword)
            return BadRequest(ApiResponse<object>.Fail("Password admin non valida."));

        var contentIds = await _context.GeneratedContents
            .IgnoreQueryFilters()
            .Where(c => c.AgencyId == id)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (contentIds.Count > 0)
        {
            var calendarEntries = await _context.CalendarEntries
                .IgnoreQueryFilters()
                .Where(e => contentIds.Contains(e.ContentId))
                .ToListAsync(ct);
            _context.CalendarEntries.RemoveRange(calendarEntries);

            var notifications = await _context.Notifications
                .IgnoreQueryFilters()
                .Where(n => n.AgencyId == id)
                .ToListAsync(ct);
            _context.Notifications.RemoveRange(notifications);

            var contents = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .Where(c => c.AgencyId == id)
                .ToListAsync(ct);
            _context.GeneratedContents.RemoveRange(contents);

            var jobs = await _context.AgentJobs
                .IgnoreQueryFilters()
                .Where(j => j.AgencyId == id)
                .ToListAsync(ct);
            _context.AgentJobs.RemoveRange(jobs);

            await _context.SaveChangesAsync(ct);
        }

        return Ok(ApiResponse<object>.Ok(new { reset = contentIds.Count }));
    }

    [HttpPut("{id:guid}/notification-settings")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateNotificationSettings(
        Guid id,
        [FromBody] UpdateAgencyNotificationSettingsRequest request,
        CancellationToken ct)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agency == null) return NotFound();

        if (request.NotificationEmail != null) agency.NotificationEmail = request.NotificationEmail;
        if (request.TelegramNotificationsEnabled.HasValue) agency.TelegramNotificationsEnabled = request.TelegramNotificationsEnabled.Value;
        if (request.NotifyEmailOnSubscribed.HasValue) agency.NotifyEmailOnSubscribed = request.NotifyEmailOnSubscribed.Value;
        if (request.NotifyPushOnSubscribed.HasValue) agency.NotifyPushOnSubscribed = request.NotifyPushOnSubscribed.Value;
        if (request.NotifyTelegramOnSubscribed.HasValue) agency.NotifyTelegramOnSubscribed = request.NotifyTelegramOnSubscribed.Value;

        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPost("{id:guid}/logo-upload")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object>>> UploadLogo(
        Guid id,
        IFormFile file,
        [FromServices] IFileStorageService fileStorage,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("File is required."));

        var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(ApiResponse<object>.Fail("Only PNG/JPG/WEBP allowed."));

        var fileName = $"agency_{id:N}_{Guid.NewGuid():N}{ext}";
        await using var stream = file.OpenReadStream();
        var publicUrl = await fileStorage.UploadAsync(stream, fileName, file.ContentType, ct);
        return Ok(ApiResponse<object>.Ok(new { url = publicUrl }));
    }
}

public record UpdateBrandVoiceRequest(BrandVoice BrandVoice);
public record UpdateTargetAudienceRequest(TargetAudience TargetAudience);
public record UpdateApprovalModeRequest(ApprovalMode ApprovalMode, int AutoApproveMinScore, bool AutoScheduleOnApproval = true);
public record UpdateDefaultLlmRequest(Guid? DefaultLlmProviderKeyId, Guid? ImageLlmProviderKeyId);
public record UpdateImageSettingsRequest(bool EnableLogoOverlay, int LogoOverlayPosition, string? LogoUrl, int LogoOverlayMode = 0, string? BrandBannerColor = null);
public record ResetAnalyticsRequest(string Password);
public record UpdateAgencyNotificationSettingsRequest(
    string? NotificationEmail = null,
    bool? TelegramNotificationsEnabled = null,
    bool? NotifyEmailOnSubscribed = null,
    bool? NotifyPushOnSubscribed = null,
    bool? NotifyTelegramOnSubscribed = null);

public class AgencyCostStatsDto
{
    public int TotalContents { get; set; }
    public decimal TotalTextCostUsd { get; set; }
    public decimal TotalImageCostUsd { get; set; }
    public decimal Last30DaysTextCostUsd { get; set; }
    public decimal Last30DaysImageCostUsd { get; set; }
    public decimal TotalCostUsd => TotalTextCostUsd + TotalImageCostUsd;
    public decimal Last30DaysCostUsd => Last30DaysTextCostUsd + Last30DaysImageCostUsd;
    public List<ProjectCostBreakdown> Projects { get; set; } = new();
}

public class ProjectCostBreakdown
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int Contents { get; set; }
    public decimal TextCostUsd { get; set; }
    public decimal ImageCostUsd { get; set; }
    public decimal TotalCostUsd => TextCostUsd + ImageCostUsd;
}
