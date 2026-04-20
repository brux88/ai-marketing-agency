using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Application.Email;
using AiMarketingAgency.Domain.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/public")]
[EnableCors("AllowPublic")]
public class PublicController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITelegramBotService _telegramBot;

    public PublicController(
        IAppDbContext context,
        IEmailNotificationService emailNotificationService,
        IPushNotificationService pushNotificationService,
        ITelegramBotService telegramBot)
    {
        _context = context;
        _emailNotificationService = emailNotificationService;
        _pushNotificationService = pushNotificationService;
        _telegramBot = telegramBot;
    }

    [HttpPost("newsletter/subscribe")]
    public async Task<ActionResult<ApiResponse<object>>> Subscribe(
        [FromBody] PublicSubscribeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.Fail("Email obbligatoria"));

        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _context.PlatformSubscribers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Email == email, ct);

        if (existing != null)
        {
            if (existing.IsActive)
                return Ok(ApiResponse<object>.Ok(null!));

            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            existing.Name = request.Name ?? existing.Name;
        }
        else
        {
            _context.PlatformSubscribers.Add(new PlatformSubscriber
            {
                Email = email,
                Name = request.Name,
                Source = request.Source ?? "landing-page",
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpPost("agencies/{agencyId:guid}/newsletter/subscribe")]
    public async Task<ActionResult<ApiResponse<object>>> SubscribeToAgencyNewsletter(
        Guid agencyId, [FromBody] AgencySubscribeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.Fail("Email obbligatoria"));

        var agency = await _context.Agencies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == agencyId, ct);

        if (agency == null)
            return NotFound(ApiResponse<object>.Fail("Agency non trovata"));

        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _context.NewsletterSubscribers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.AgencyId == agencyId && s.ProjectId == null && s.Email == email, ct);

        var wasReactivated = false;
        var isNew = false;

        if (existing != null)
        {
            if (existing.IsActive)
                return Ok(ApiResponse<object>.Ok(null!));

            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            existing.Name = request.Name ?? existing.Name;
            wasReactivated = true;
        }
        else
        {
            _context.NewsletterSubscribers.Add(new NewsletterSubscriber
            {
                AgencyId = agencyId,
                TenantId = agency.TenantId,
                Email = email,
                Name = request.Name,
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            });
            isNew = true;
        }

        await _context.SaveChangesAsync(ct);

        if (isNew || wasReactivated)
        {
            await FanOutNewSubscriberAsync(
                agencyId: agencyId,
                projectId: null,
                subscriberEmail: email,
                targetName: agency.Name,
                emailFlag: agency.NotifyEmailOnSubscribed,
                pushFlag: agency.NotifyPushOnSubscribed,
                telegramFlag: agency.NotifyTelegramOnSubscribed,
                ct: ct);
        }

        return Ok(ApiResponse<object>.Ok(null!));
    }

    // Legacy email-based unsubscribe (kept for backwards compat with existing emails).
    [HttpGet("agencies/{agencyId:guid}/newsletter/unsubscribe")]
    public async Task<ContentResult> UnsubscribeFromAgencyNewsletter(
        Guid agencyId, [FromQuery] string email, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var subscriber = await _context.NewsletterSubscribers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.AgencyId == agencyId && s.Email == email.Trim().ToLowerInvariant(), ct);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IsActive = false;
                subscriber.UnsubscribedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        return UnsubscribeConfirmationPage("Non riceverai piu le nostre newsletter. Ci dispiace vederti andare!");
    }

    // ── Project-level newsletter subscribe ──

    [HttpPost("agencies/{agencyId:guid}/projects/{projectId:guid}/newsletter/subscribe")]
    public async Task<ActionResult<ApiResponse<object>>> SubscribeToProjectNewsletter(
        Guid agencyId, Guid projectId, [FromBody] AgencySubscribeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.Fail("Email obbligatoria"));

        var project = await _context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null)
            return NotFound(ApiResponse<object>.Fail("Progetto non trovato"));

        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _context.NewsletterSubscribers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.AgencyId == agencyId && s.ProjectId == projectId && s.Email == email, ct);

        var wasReactivated = false;
        var isNew = false;

        if (existing != null)
        {
            if (existing.IsActive)
                return Ok(ApiResponse<object>.Ok(null!));
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            existing.Name = request.Name ?? existing.Name;
            wasReactivated = true;
        }
        else
        {
            _context.NewsletterSubscribers.Add(new NewsletterSubscriber
            {
                AgencyId = agencyId,
                TenantId = project.TenantId,
                ProjectId = projectId,
                Email = email,
                Name = request.Name,
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            });
            isNew = true;
        }

        await _context.SaveChangesAsync(ct);

        if (isNew || wasReactivated)
        {
            await FanOutNewSubscriberAsync(
                agencyId: agencyId,
                projectId: projectId,
                subscriberEmail: email,
                targetName: project.Name,
                emailFlag: project.NotifyEmailOnSubscribed,
                pushFlag: project.NotifyPushOnSubscribed,
                telegramFlag: project.NotifyTelegramOnSubscribed,
                ct: ct);
        }

        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("agencies/{agencyId:guid}/projects/{projectId:guid}/newsletter/unsubscribe")]
    public async Task<ContentResult> UnsubscribeFromProjectNewsletter(
        Guid agencyId, Guid projectId, [FromQuery] string email, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var subscriber = await _context.NewsletterSubscribers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.AgencyId == agencyId && s.ProjectId == projectId
                    && s.Email == email.Trim().ToLowerInvariant(), ct);
            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IsActive = false;
                subscriber.UnsubscribedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        return UnsubscribeConfirmationPage("Non riceverai piu le newsletter di questo progetto.");
    }

    // Token-based unsubscribe — target for the links embedded in newsletter emails.
    [HttpGet("newsletter/unsubscribe-token")]
    public async Task<ContentResult> UnsubscribeByToken([FromQuery] Guid token, CancellationToken ct)
    {
        if (token != Guid.Empty)
        {
            var subscriber = await _context.NewsletterSubscribers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token, ct);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IsActive = false;
                subscriber.UnsubscribedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        return UnsubscribeConfirmationPage("Non riceverai piu le nostre email. Ci dispiace vederti andare!");
    }

    [HttpGet("newsletter/unsubscribe")]
    public async Task<ContentResult> Unsubscribe([FromQuery] string email, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var subscriber = await _context.PlatformSubscribers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Email == email.Trim().ToLowerInvariant(), ct);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IsActive = false;
                subscriber.UnsubscribedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        return UnsubscribeConfirmationPage("Non riceverai piu le nostre email. Ci dispiace vederti andare!");
    }

    private static ContentResult UnsubscribeConfirmationPage(string message) => new()
    {
        ContentType = "text/html",
        Content = $"""
            <html><body style="font-family:-apple-system,BlinkMacSystemFont,sans-serif;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;background:#F2EFE8;color:#222">
            <div style="text-align:center;padding:2rem;background:#FBFAF5;border:1px solid #E4E1DA;border-radius:14px;max-width:460px">
            <h2 style="margin:0 0 8px;letter-spacing:-0.3px">Disiscrizione completata</h2>
            <p style="margin:0;color:#898780">{message}</p>
            </div></body></html>
            """
    };

    private async Task FanOutNewSubscriberAsync(
        Guid agencyId,
        Guid? projectId,
        string subscriberEmail,
        string targetName,
        bool emailFlag,
        bool pushFlag,
        bool telegramFlag,
        CancellationToken ct)
    {
        if (emailFlag)
        {
            try
            {
                var html = EmailTemplates.NewSubscriberNotification(subscriberEmail, targetName);
                await _emailNotificationService.SendEmailNotificationAsync(
                    agencyId, projectId,
                    $"Nuovo iscritto alla newsletter - {targetName}",
                    html, ct);
            }
            catch { /* swallow — notifications must not break subscribe */ }
        }

        if (pushFlag)
        {
            try
            {
                await _pushNotificationService.SendToProjectAsync(
                    agencyId, projectId, PushEventType.NewSubscriber,
                    "Nuovo iscritto",
                    $"{subscriberEmail} si e iscritto alla newsletter di {targetName}",
                    data: null, ct: ct);
            }
            catch { /* swallow */ }
        }

        if (telegramFlag)
        {
            try
            {
                await _telegramBot.NotifyAgencyAsync(
                    agencyId, projectId,
                    $"<b>Nuovo iscritto</b>\n{subscriberEmail} si e iscritto alla newsletter di <i>{targetName}</i>",
                    ct);
            }
            catch { /* swallow */ }
        }
    }
}

public record PublicSubscribeRequest(string Email, string? Name = null, string? Source = null);

public record AgencySubscribeRequest(string Email, string? Name = null);
