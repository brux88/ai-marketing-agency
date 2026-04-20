using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Projects.Commands.CreateProject;
using AiMarketingAgency.Application.Projects.Commands.DeleteProject;
using AiMarketingAgency.Application.Projects.Commands.ExtractBrandFromWebsite;
using AiMarketingAgency.Application.Projects.Commands.UpdateProject;
using AiMarketingAgency.Application.Projects.Dtos;
using AiMarketingAgency.Application.Projects.Queries.GetProjectById;
using AiMarketingAgency.Application.Projects.Queries.GetProjectsByAgency;
using AiMarketingAgency.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;

    public ProjectsController(IMediator mediator, IAppDbContext context, IConfiguration configuration)
    {
        _mediator = mediator;
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetAll(Guid agencyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectsByAgencyQuery(agencyId), ct);
        return Ok(ApiResponse<List<ProjectDto>>.Ok(result));
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(agencyId, projectId), ct);
        if (result == null) return NotFound();
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create(Guid agencyId, [FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var command = new CreateProjectCommand
        {
            AgencyId = agencyId,
            Name = request.Name,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            LogoUrl = request.LogoUrl,
            BrandVoice = request.BrandVoice,
            TargetAudience = request.TargetAudience
        };

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { agencyId, projectId = result.Id }, ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(Guid agencyId, Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var command = new UpdateProjectCommand
        {
            ProjectId = projectId,
            AgencyId = agencyId,
            Name = request.Name,
            Description = request.Description,
            WebsiteUrl = request.WebsiteUrl,
            LogoUrl = request.LogoUrl,
            BrandVoice = request.BrandVoice,
            TargetAudience = request.TargetAudience,
            ApprovalMode = request.ApprovalMode,
            AutoApproveMinScore = request.AutoApproveMinScore
        };

        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid agencyId, Guid projectId,
        [FromQuery] string? password,
        CancellationToken ct)
    {
        var adminPwd = _configuration["AdminPassword"] ?? "Admin123!";
        if (string.IsNullOrEmpty(password) || password != adminPwd)
            return BadRequest(ApiResponse<object>.Fail("Password admin richiesta per eliminare un progetto."));

        await _mediator.Send(new DeleteProjectCommand(agencyId, projectId), ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{projectId:guid}/approval-mode")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateApprovalMode(
        Guid agencyId, Guid projectId,
        [FromBody] UpdateProjectApprovalModeRequest request,
        CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId && p.IsActive, ct);
        if (project == null) return NotFound();

        project.ApprovalMode = request.ApprovalMode;
        project.AutoApproveMinScore = request.AutoApproveMinScore;
        project.AutoScheduleOnApproval = request.AutoScheduleOnApproval;
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{projectId:guid}/image-settings")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateImageSettings(
        Guid agencyId, Guid projectId,
        [FromBody] UpdateProjectImageSettingsRequest request,
        CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId && p.IsActive, ct);
        if (project == null) return NotFound();

        project.EnableLogoOverlay = request.EnableLogoOverlay;
        project.LogoOverlayPosition = request.LogoOverlayPosition;
        project.LogoUrl = request.LogoUrl;
        project.LogoOverlayMode = request.LogoOverlayMode;
        project.BrandBannerColor = request.BrandBannerColor;
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{projectId:guid}/social-platforms")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSocialPlatforms(
        Guid agencyId, Guid projectId,
        [FromBody] UpdateProjectSocialPlatformsRequest request,
        CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId && p.IsActive, ct);
        if (project == null) return NotFound();

        project.EnabledSocialPlatforms = request.EnabledSocialPlatforms;
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("{projectId:guid}/email-notifications")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEmailNotifications(
        Guid agencyId, Guid projectId,
        [FromBody] UpdateProjectEmailNotificationsRequest request,
        CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId && p.IsActive, ct);
        if (project == null) return NotFound();

        project.NotifyEmailOnGeneration = request.NotifyEmailOnGeneration;
        project.NotifyEmailOnPublication = request.NotifyEmailOnPublication;
        project.NotifyEmailOnApprovalNeeded = request.NotifyEmailOnApprovalNeeded;
        project.NotificationEmail = request.NotificationEmail;
        if (request.NotifyPushOnGeneration.HasValue) project.NotifyPushOnGeneration = request.NotifyPushOnGeneration.Value;
        if (request.NotifyPushOnPublication.HasValue) project.NotifyPushOnPublication = request.NotifyPushOnPublication.Value;
        if (request.NotifyPushOnApprovalNeeded.HasValue) project.NotifyPushOnApprovalNeeded = request.NotifyPushOnApprovalNeeded.Value;
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPost("{projectId:guid}/logo-upload")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object>>> UploadLogo(
        Guid agencyId, Guid projectId,
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

        var fileName = $"proj_{projectId:N}_{Guid.NewGuid():N}{ext}";
        await using var stream = file.OpenReadStream();
        var publicUrl = await fileStorage.UploadAsync(stream, fileName, file.ContentType, ct);
        return Ok(ApiResponse<object>.Ok(new { url = publicUrl }));
    }

    [HttpGet("{projectId:guid}/cost-stats")]
    public async Task<ActionResult<ApiResponse<ProjectCostStatsDto>>> GetCostStats(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        // Include soft-deleted content: costs were already incurred.
        var contents = await _context.GeneratedContents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId)
            .Select(c => new { c.AiGenerationCostUsd, c.AiImageCostUsd, c.CreatedAt, c.ContentType })
            .ToListAsync(ct);

        var totalText = contents.Sum(c => c.AiGenerationCostUsd ?? 0m);
        var totalImage = contents.Sum(c => c.AiImageCostUsd ?? 0m);
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var last30Text = contents.Where(c => c.CreatedAt >= cutoff).Sum(c => c.AiGenerationCostUsd ?? 0m);
        var last30Image = contents.Where(c => c.CreatedAt >= cutoff).Sum(c => c.AiImageCostUsd ?? 0m);

        return Ok(ApiResponse<ProjectCostStatsDto>.Ok(new ProjectCostStatsDto
        {
            TotalContents = contents.Count,
            TotalTextCostUsd = totalText,
            TotalImageCostUsd = totalImage,
            Last30DaysTextCostUsd = last30Text,
            Last30DaysImageCostUsd = last30Image
        }));
    }

    [HttpPost("{projectId:guid}/reset-analytics")]
    public async Task<ActionResult<ApiResponse<object>>> ResetAnalytics(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var contentIds = await _context.GeneratedContents
            .IgnoreQueryFilters()
            .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (contentIds.Count > 0)
        {
            var calendarEntries = await _context.CalendarEntries
                .Where(e => contentIds.Contains(e.ContentId))
                .ToListAsync(ct);
            _context.CalendarEntries.RemoveRange(calendarEntries);

            var notifications = await _context.Notifications
                .IgnoreQueryFilters()
                .Where(n => n.ProjectId == projectId)
                .ToListAsync(ct);
            _context.Notifications.RemoveRange(notifications);

            var contents = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId)
                .ToListAsync(ct);
            _context.GeneratedContents.RemoveRange(contents);

            await _context.SaveChangesAsync(ct);
        }

        return Ok(ApiResponse<object>.Ok(new { reset = contentIds.Count }));
    }

    [HttpPost("{projectId:guid}/extract-brand")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> ExtractBrand(Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ExtractBrandFromWebsiteCommand(agencyId, projectId), ct);
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpPut("{projectId:guid}/prompts")]
    public async Task<ActionResult<ApiResponse<ProjectPromptsDto>>> UpdatePrompts(
        Guid agencyId, Guid projectId, [FromBody] UpdatePromptsRequest request, CancellationToken ct)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null) return NotFound();

        project.BlogPromptTemplate = request.BlogPromptTemplate;
        project.SocialPromptTemplate = request.SocialPromptTemplate;
        project.NewsletterPromptTemplate = request.NewsletterPromptTemplate;
        if (request.ExtractedContext != null)
            project.ExtractedContext = request.ExtractedContext;
        if (request.LogoUrl != null)
            project.LogoUrl = request.LogoUrl;

        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<ProjectPromptsDto>.Ok(new ProjectPromptsDto
        {
            BlogPromptTemplate = project.BlogPromptTemplate,
            SocialPromptTemplate = project.SocialPromptTemplate,
            NewsletterPromptTemplate = project.NewsletterPromptTemplate,
            ExtractedContext = project.ExtractedContext,
            ExtractedContextAt = project.ExtractedContextAt,
            LogoUrl = project.LogoUrl
        }));
    }

    [HttpGet("{projectId:guid}/prompts")]
    public async Task<ActionResult<ApiResponse<ProjectPromptsDto>>> GetPrompts(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null) return NotFound();

        return Ok(ApiResponse<ProjectPromptsDto>.Ok(new ProjectPromptsDto
        {
            BlogPromptTemplate = project.BlogPromptTemplate,
            SocialPromptTemplate = project.SocialPromptTemplate,
            NewsletterPromptTemplate = project.NewsletterPromptTemplate,
            ExtractedContext = project.ExtractedContext,
            ExtractedContextAt = project.ExtractedContextAt,
            LogoUrl = project.LogoUrl
        }));
    }

    // ── Project Documents (RAG context) ──────────────────────────────

    [HttpGet("{projectId:guid}/documents")]
    public async Task<ActionResult<ApiResponse<List<ProjectDocumentDto>>>> GetDocuments(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var docs = await _context.ProjectDocuments
            .AsNoTracking()
            .Where(d => d.AgencyId == agencyId && d.ProjectId == projectId && d.IsActive)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new ProjectDocumentDto
            {
                Id = d.Id,
                Name = d.Name,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                FileSizeBytes = d.FileSizeBytes,
                HasExtractedText = !string.IsNullOrEmpty(d.ExtractedText),
                ExtractedTextLength = d.ExtractedText != null ? d.ExtractedText.Length : 0,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<ProjectDocumentDto>>.Ok(docs));
    }

    [HttpPost("{projectId:guid}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<ApiResponse<ProjectDocumentDto>>> UploadDocument(
        Guid agencyId, Guid projectId,
        IFormFile file,
        [FromForm] string? name,
        [FromServices] IFileStorageService fileStorage,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<ProjectDocumentDto>.Fail("File is required."));

        var allowed = new[] { ".txt", ".md", ".pdf", ".csv", ".json" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(ApiResponse<ProjectDocumentDto>.Fail("Only TXT, MD, PDF, CSV, JSON files are allowed."));

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId && p.IsActive, ct);
        if (project == null) return NotFound();

        // Upload to blob storage
        var blobName = $"docs/{projectId:N}/{Guid.NewGuid():N}{ext}";
        await using var stream = file.OpenReadStream();
        var publicUrl = await fileStorage.UploadAsync(stream, blobName, file.ContentType, ct);

        // Extract text content
        string? extractedText = null;
        try
        {
            if (ext is ".txt" or ".md" or ".csv" or ".json")
            {
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                extractedText = await reader.ReadToEndAsync(ct);
            }
            // PDF extraction would require a library like PdfPig; for now store null and
            // the user can see it wasn't extracted. We handle it gracefully.
        }
        catch
        {
            // Text extraction is best-effort
        }

        // If stream wasn't seekable (e.g. after upload), re-read from file
        if (extractedText == null && ext is ".txt" or ".md" or ".csv" or ".json")
        {
            try
            {
                using var ms = new MemoryStream();
                using var rereadStream = file.OpenReadStream();
                await rereadStream.CopyToAsync(ms, ct);
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                extractedText = await reader.ReadToEndAsync(ct);
            }
            catch { /* best effort */ }
        }

        // Truncate to avoid storing excessively large text (max ~200KB chars)
        if (extractedText != null && extractedText.Length > 200_000)
            extractedText = extractedText[..200_000];

        var doc = new Domain.Entities.ProjectDocument
        {
            AgencyId = agencyId,
            TenantId = project.TenantId,
            ProjectId = projectId,
            Name = name ?? Path.GetFileNameWithoutExtension(file.FileName),
            FileName = file.FileName,
            FileUrl = publicUrl,
            FileSizeBytes = file.Length,
            ExtractedText = extractedText
        };

        _context.ProjectDocuments.Add(doc);
        await _context.SaveChangesAsync(ct);

        var dto = new ProjectDocumentDto
        {
            Id = doc.Id,
            Name = doc.Name,
            FileName = doc.FileName,
            FileUrl = doc.FileUrl,
            FileSizeBytes = doc.FileSizeBytes,
            HasExtractedText = !string.IsNullOrEmpty(doc.ExtractedText),
            ExtractedTextLength = doc.ExtractedText?.Length ?? 0,
            CreatedAt = doc.CreatedAt
        };

        return Ok(ApiResponse<ProjectDocumentDto>.Ok(dto));
    }

    [HttpDelete("{projectId:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(
        Guid agencyId, Guid projectId, Guid documentId,
        [FromServices] IFileStorageService fileStorage,
        CancellationToken ct)
    {
        var doc = await _context.ProjectDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.AgencyId == agencyId && d.ProjectId == projectId, ct);
        if (doc == null) return NotFound();

        try { await fileStorage.DeleteAsync(doc.FileUrl, ct); } catch { /* best effort */ }

        doc.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(null));
    }
}

public record UpdatePromptsRequest(
    string? BlogPromptTemplate,
    string? SocialPromptTemplate,
    string? NewsletterPromptTemplate,
    string? ExtractedContext = null,
    string? LogoUrl = null);

public class ProjectPromptsDto
{
    public string? BlogPromptTemplate { get; set; }
    public string? SocialPromptTemplate { get; set; }
    public string? NewsletterPromptTemplate { get; set; }
    public string? ExtractedContext { get; set; }
    public DateTime? ExtractedContextAt { get; set; }
    public string? LogoUrl { get; set; }
}

public record CreateProjectRequest(
    string Name,
    string? Description = null,
    string? WebsiteUrl = null,
    string? LogoUrl = null,
    BrandVoice? BrandVoice = null,
    TargetAudience? TargetAudience = null);

public record UpdateProjectRequest(
    string Name,
    string? Description = null,
    string? WebsiteUrl = null,
    string? LogoUrl = null,
    BrandVoice? BrandVoice = null,
    TargetAudience? TargetAudience = null,
    AiMarketingAgency.Domain.Enums.ApprovalMode? ApprovalMode = null,
    int? AutoApproveMinScore = null);

public record UpdateProjectApprovalModeRequest(
    AiMarketingAgency.Domain.Enums.ApprovalMode? ApprovalMode,
    int? AutoApproveMinScore,
    bool? AutoScheduleOnApproval);

public record UpdateProjectImageSettingsRequest(
    bool? EnableLogoOverlay,
    int? LogoOverlayPosition,
    string? LogoUrl,
    int? LogoOverlayMode = null,
    string? BrandBannerColor = null);

public record UpdateProjectSocialPlatformsRequest(string? EnabledSocialPlatforms);

public record UpdateProjectEmailNotificationsRequest(
    bool NotifyEmailOnGeneration,
    bool NotifyEmailOnPublication,
    bool NotifyEmailOnApprovalNeeded,
    string? NotificationEmail,
    bool? NotifyPushOnGeneration = null,
    bool? NotifyPushOnPublication = null,
    bool? NotifyPushOnApprovalNeeded = null);

public class ProjectCostStatsDto
{
    public int TotalContents { get; set; }
    public decimal TotalTextCostUsd { get; set; }
    public decimal TotalImageCostUsd { get; set; }
    public decimal Last30DaysTextCostUsd { get; set; }
    public decimal Last30DaysImageCostUsd { get; set; }
    public decimal TotalCostUsd => TotalTextCostUsd + TotalImageCostUsd;
    public decimal Last30DaysCostUsd => Last30DaysTextCostUsd + Last30DaysImageCostUsd;
}

public class ProjectDocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool HasExtractedText { get; set; }
    public int ExtractedTextLength { get; set; }
    public DateTime CreatedAt { get; set; }
}
