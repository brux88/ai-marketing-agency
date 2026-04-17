using System.Text.Json.Serialization;
using AiMarketingAgency.Application.Common;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/agencies/{agencyId:guid}/telegram")]
[Authorize]
public class TelegramController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ITelegramBotService _telegramBot;

    public TelegramController(IAppDbContext context, ITenantContext tenantContext, ITelegramBotService telegramBot)
    {
        _context = context;
        _tenantContext = tenantContext;
        _telegramBot = telegramBot;
    }

    // Bot credentials (token stored on Agency for white-label)
    [HttpGet("bot")]
    public async Task<ActionResult<ApiResponse<TelegramBotInfoDto>>> GetBot(
        Guid agencyId, CancellationToken ct)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, ct);
        if (agency == null) return NotFound();

        return Ok(ApiResponse<TelegramBotInfoDto>.Ok(new TelegramBotInfoDto
        {
            HasToken = !string.IsNullOrEmpty(agency.TelegramBotToken),
            BotUsername = agency.TelegramBotUsername,
            WebhookUrl = $"{Request.Scheme}://{Request.Host}/api/v1/telegram/webhook/{agencyId}"
        }));
    }

    [HttpPut("bot")]
    public async Task<ActionResult<ApiResponse<TelegramBotInfoDto>>> SetBot(
        Guid agencyId, [FromBody] SetTelegramBotRequest request, CancellationToken ct)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, ct);
        if (agency == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Token))
            agency.TelegramBotToken = request.Token;
        agency.TelegramBotUsername = request.BotUsername;

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<TelegramBotInfoDto>.Ok(new TelegramBotInfoDto
        {
            HasToken = !string.IsNullOrEmpty(agency.TelegramBotToken),
            BotUsername = agency.TelegramBotUsername,
            WebhookUrl = $"{Request.Scheme}://{Request.Host}/api/v1/telegram/webhook/{agencyId}"
        }));
    }

    [HttpDelete("bot")]
    public async Task<ActionResult> ClearBot(Guid agencyId, CancellationToken ct)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, ct);
        if (agency == null) return NotFound();

        agency.TelegramBotToken = null;
        agency.TelegramBotUsername = null;
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true });
    }

    // Per-project Telegram bot (optional override)
    [HttpGet("project/{projectId:guid}/bot")]
    public async Task<ActionResult<ApiResponse<TelegramBotInfoDto>>> GetProjectBot(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null) return NotFound();

        return Ok(ApiResponse<TelegramBotInfoDto>.Ok(new TelegramBotInfoDto
        {
            HasToken = !string.IsNullOrEmpty(project.TelegramBotToken),
            BotUsername = project.TelegramBotUsername,
            WebhookUrl = $"{Request.Scheme}://{Request.Host}/api/v1/telegram/webhook/{agencyId}"
        }));
    }

    [HttpPut("project/{projectId:guid}/bot")]
    public async Task<ActionResult<ApiResponse<TelegramBotInfoDto>>> SetProjectBot(
        Guid agencyId, Guid projectId, [FromBody] SetTelegramBotRequest request, CancellationToken ct)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Token))
            project.TelegramBotToken = request.Token;
        project.TelegramBotUsername = request.BotUsername;

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<TelegramBotInfoDto>.Ok(new TelegramBotInfoDto
        {
            HasToken = !string.IsNullOrEmpty(project.TelegramBotToken),
            BotUsername = project.TelegramBotUsername,
            WebhookUrl = $"{Request.Scheme}://{Request.Host}/api/v1/telegram/webhook/{agencyId}"
        }));
    }

    [HttpDelete("project/{projectId:guid}/bot")]
    public async Task<ActionResult> ClearProjectBot(Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null) return NotFound();

        project.TelegramBotToken = null;
        project.TelegramBotUsername = null;
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true });
    }

    // Connected chats
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TelegramConnectionDto>>>> GetConnections(
        Guid agencyId, CancellationToken ct)
    {
        var connections = await _context.TelegramConnections
            .Where(c => c.AgencyId == agencyId)
            .Select(c => new TelegramConnectionDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                ProjectName = c.Project != null ? c.Project.Name : null,
                ChatId = c.ChatId,
                ChatTitle = c.ChatTitle,
                Username = c.Username,
                NotifyOnContentGenerated = c.NotifyOnContentGenerated,
                NotifyOnApprovalNeeded = c.NotifyOnApprovalNeeded,
                NotifyOnPublished = c.NotifyOnPublished,
                AllowCommands = c.AllowCommands,
                IsActive = c.IsActive
            })
            .OrderBy(c => c.ProjectId == null ? 0 : 1)
            .ThenBy(c => c.ProjectName)
            .ToListAsync(ct);

        return Ok(ApiResponse<List<TelegramConnectionDto>>.Ok(connections));
    }

    [HttpPost("connect")]
    public async Task<ActionResult<ApiResponse<TelegramConnectionDto>>> Connect(
        Guid agencyId, [FromBody] ConnectTelegramRequest request, CancellationToken ct)
    {
        var connection = new TelegramConnection
        {
            TenantId = _tenantContext.TenantId,
            AgencyId = agencyId,
            ProjectId = request.ProjectId,
            ChatId = request.ChatId,
            ChatTitle = request.ChatTitle,
            Username = request.Username,
            NotifyOnContentGenerated = request.NotifyOnContentGenerated,
            NotifyOnApprovalNeeded = request.NotifyOnApprovalNeeded,
            NotifyOnPublished = request.NotifyOnPublished,
            AllowCommands = request.AllowCommands,
            IsActive = true
        };

        _context.TelegramConnections.Add(connection);
        await _context.SaveChangesAsync(ct);

        await _telegramBot.SendMessageAsync(agencyId, request.ProjectId, request.ChatId,
            "<b>Connesso ad AI Marketing Agency!</b>\n\nRiceverai notifiche per questa agenzia.", ct);

        string? projectName = null;
        if (connection.ProjectId.HasValue)
        {
            projectName = await _context.Projects
                .Where(p => p.Id == connection.ProjectId.Value)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(ApiResponse<TelegramConnectionDto>.Ok(new TelegramConnectionDto
        {
            Id = connection.Id,
            ProjectId = connection.ProjectId,
            ProjectName = projectName,
            ChatId = connection.ChatId,
            ChatTitle = connection.ChatTitle,
            Username = connection.Username,
            NotifyOnContentGenerated = connection.NotifyOnContentGenerated,
            NotifyOnApprovalNeeded = connection.NotifyOnApprovalNeeded,
            NotifyOnPublished = connection.NotifyOnPublished,
            AllowCommands = connection.AllowCommands,
            IsActive = connection.IsActive
        }));
    }

    [HttpDelete("{connectionId:guid}")]
    public async Task<ActionResult> Disconnect(Guid agencyId, Guid connectionId, CancellationToken ct)
    {
        var connection = await _context.TelegramConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.AgencyId == agencyId, ct);

        if (connection == null) return NotFound();

        _context.TelegramConnections.Remove(connection);
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true });
    }

    [HttpPost("test")]
    public async Task<ActionResult> TestMessage(Guid agencyId, CancellationToken ct)
    {
        await _telegramBot.NotifyAgencyAsync(agencyId, null, "<b>Test</b>\n\nLa connessione Telegram funziona correttamente!", ct);
        return Ok(new { success = true });
    }

    [HttpPost("register-webhook")]
    public async Task<ActionResult<ApiResponse<TelegramWebhookRegistrationDto>>> RegisterAgencyWebhook(
        Guid agencyId, CancellationToken ct)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, ct);
        if (agency == null) return NotFound();
        if (string.IsNullOrWhiteSpace(agency.TelegramBotToken))
            return BadRequest(ApiResponse<TelegramWebhookRegistrationDto>.Fail("Token non configurato"));

        var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/v1/telegram/webhook/{agencyId}";
        var result = await _telegramBot.RegisterWebhookAsync(agency.TelegramBotToken, webhookUrl, ct);
        if (result.Ok && !string.IsNullOrWhiteSpace(result.BotUsername))
        {
            agency.TelegramBotUsername = result.BotUsername;
            await _context.SaveChangesAsync(ct);
        }
        return Ok(ApiResponse<TelegramWebhookRegistrationDto>.Ok(new TelegramWebhookRegistrationDto
        {
            Ok = result.Ok,
            Description = result.Description,
            BotUsername = result.BotUsername,
            WebhookUrl = webhookUrl
        }));
    }

    [HttpPost("project/{projectId:guid}/register-webhook")]
    public async Task<ActionResult<ApiResponse<TelegramWebhookRegistrationDto>>> RegisterProjectWebhook(
        Guid agencyId, Guid projectId, CancellationToken ct)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);
        if (project == null) return NotFound();
        if (string.IsNullOrWhiteSpace(project.TelegramBotToken))
            return BadRequest(ApiResponse<TelegramWebhookRegistrationDto>.Fail("Token di progetto non configurato"));

        var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/v1/telegram/webhook/{agencyId}";
        var result = await _telegramBot.RegisterWebhookAsync(project.TelegramBotToken, webhookUrl, ct);
        if (result.Ok && !string.IsNullOrWhiteSpace(result.BotUsername))
        {
            project.TelegramBotUsername = result.BotUsername;
            await _context.SaveChangesAsync(ct);
        }
        return Ok(ApiResponse<TelegramWebhookRegistrationDto>.Ok(new TelegramWebhookRegistrationDto
        {
            Ok = result.Ok,
            Description = result.Description,
            BotUsername = result.BotUsername,
            WebhookUrl = webhookUrl
        }));
    }
}

// Webhook endpoint for Telegram bot updates (unauthenticated, per-agency route)
[ApiController]
[Route("api/v1/telegram/webhook")]
public class TelegramWebhookController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly ITelegramBotService _telegramBot;
    private readonly ILogger<TelegramWebhookController> _logger;

    public TelegramWebhookController(IAppDbContext context, ITelegramBotService telegramBot, ILogger<TelegramWebhookController> logger)
    {
        _context = context;
        _telegramBot = telegramBot;
        _logger = logger;
    }

    [HttpPost("{agencyId:guid}")]
    public async Task<ActionResult> HandleUpdate(
        Guid agencyId, [FromBody] TelegramUpdate update, CancellationToken ct)
    {
        // Handle inline button callbacks (approve/reject buttons)
        if (update?.CallbackQuery != null)
        {
            var cbData = update.CallbackQuery.Data ?? "";
            var cbChatId = update.CallbackQuery.Message?.Chat?.Id ?? 0;
            if (cbChatId == 0) return Ok();

            await HandleCommand(agencyId, cbChatId, cbData, ct: ct);
            return Ok();
        }

        if (update?.Message?.Text == null) return Ok();

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text.Trim();

        _logger.LogInformation("Telegram update for agency {AgencyId} from chat {ChatId}: {Text}", agencyId, chatId, text);

        await HandleCommand(agencyId, chatId, text, update.Message.Chat.Title, update.Message.From?.Username, ct);
        return Ok();
    }

    private async Task HandleCommand(Guid agencyId, long chatId, string text, string? chatTitle = null, string? username = null, CancellationToken ct = default)
    {
        _logger.LogInformation("HandleCommand: agency={AgencyId} chat={ChatId} text={Text}", agencyId, chatId, text);

        if (text.StartsWith("approve_") || text.StartsWith("/approve_"))
        {
            var contentIdStr = text.Replace("/approve_", "").Replace("approve_", "");
            if (Guid.TryParse(contentIdStr, out var contentId))
            {
                var content = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == contentId && c.AgencyId == agencyId, ct);

                if (content != null && content.Status == Domain.Enums.ContentStatus.InReview)
                {
                    content.Status = Domain.Enums.ContentStatus.Approved;
                    content.ApprovedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(ct);

                    await CalendarAutoScheduler.TryScheduleAsync(_context, content, _logger, ct);

                    await _telegramBot.SendMessageAsync(agencyId, content.ProjectId, chatId, $"✅ Contenuto '<b>{content.Title}</b>' approvato e aggiunto al calendario!", ct);
                }
                else if (content != null)
                {
                    await _telegramBot.SendMessageAsync(agencyId, content.ProjectId, chatId,
                        $"⚠️ Contenuto '<b>{content.Title}</b>' non è in attesa di approvazione (stato: {content.Status}).", ct);
                }
            }
        }
        else if (text.StartsWith("reject_") || text.StartsWith("/reject_"))
        {
            var contentIdStr = text.Replace("/reject_", "").Replace("reject_", "");
            if (Guid.TryParse(contentIdStr, out var contentId))
            {
                var content = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == contentId && c.AgencyId == agencyId, ct);

                if (content != null && content.Status == Domain.Enums.ContentStatus.InReview)
                {
                    content.Status = Domain.Enums.ContentStatus.Rejected;
                    await _context.SaveChangesAsync(ct);
                    await _telegramBot.SendMessageAsync(agencyId, content.ProjectId, chatId, $"❌ Contenuto '<b>{content.Title}</b>' rifiutato.", ct);
                }
            }
        }
        else if (text == "/status" || text == "status")
        {
            var pendingCount = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .CountAsync(c => c.AgencyId == agencyId && c.Status == Domain.Enums.ContentStatus.InReview, ct);

            await _telegramBot.SendMessageAsync(agencyId, null, chatId,
                $"<b>📊 Status Agenzia</b>\n\nContenuti in attesa di approvazione: {pendingCount}", ct);
        }
        else if (text == "/programmati" || text == "programmati")
        {
            var scheduled = await _context.CalendarEntries
                .IgnoreQueryFilters()
                .Include(e => e.Content)
                .Where(e => e.AgencyId == agencyId && e.Status == Domain.Enums.CalendarEntryStatus.Scheduled)
                .OrderBy(e => e.ScheduledAt)
                .Take(10)
                .ToListAsync(ct);

            if (scheduled.Count == 0)
            {
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, "📅 Nessun post programmato.", ct);
            }
            else
            {
                foreach (var e in scheduled)
                {
                    var platform = e.Platform?.ToString() ?? "N/A";
                    var date = e.ScheduledAt.ToString("dd/MM/yyyy HH:mm");
                    var title = e.Content?.Title ?? "—";
                    var msg = $"📅 <b>{title}</b>\n\n🕐 {date}\n📱 {platform}";
                    var btns = new List<TelegramInlineButton>
                    {
                        new("🚀 Pubblica ora", $"publish_{e.Id}"),
                        new("🗑 Rimuovi", $"removecal_{e.Id}")
                    };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, e.Content?.ProjectId, chatId, msg, btns, ct);
                }
            }
        }
        else if (text.StartsWith("publish_") || text.StartsWith("/publish_"))
        {
            var entryIdStr = text.Replace("/publish_", "").Replace("publish_", "");
            if (Guid.TryParse(entryIdStr, out var entryId))
            {
                var entry = await _context.CalendarEntries
                    .IgnoreQueryFilters()
                    .Include(e => e.Content)
                    .FirstOrDefaultAsync(e => e.Id == entryId && e.AgencyId == agencyId, ct);

                if (entry == null)
                {
                    await _telegramBot.SendMessageAsync(agencyId, null, chatId, "⚠️ Voce calendario non trovata.", ct);
                }
                else if (entry.Status != Domain.Enums.CalendarEntryStatus.Scheduled)
                {
                    await _telegramBot.SendMessageAsync(agencyId, null, chatId,
                        $"⚠️ Voce non pubblicabile (stato: {entry.Status}).", ct);
                }
                else if (entry.Platform != null)
                {
                    var mediator = HttpContext.RequestServices.GetRequiredService<MediatR.IMediator>();
                    var tenantCtx = HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                    tenantCtx.SetTenant(entry.TenantId, Guid.Empty);

                    var result = await mediator.Send(
                        new Application.SocialConnectors.Commands.PublishContent.PublishContentCommand(
                            entry.AgencyId, entry.ContentId, entry.Platform.Value), ct);

                    entry.Status = result.Success ? Domain.Enums.CalendarEntryStatus.Published : Domain.Enums.CalendarEntryStatus.Failed;
                    entry.PublishedAt = result.Success ? DateTime.UtcNow : null;
                    entry.ErrorMessage = result.Success ? null : result.Error;
                    entry.PostUrl = result.Success ? result.PostUrl : entry.PostUrl;
                    await _context.SaveChangesAsync(ct);

                    var title = entry.Content?.Title ?? "Contenuto";
                    if (result.Success)
                        await _telegramBot.SendMessageAsync(agencyId, entry.Content?.ProjectId, chatId,
                            $"📢 <b>Pubblicato su {entry.Platform.Value}!</b>\n{title}\n{result.PostUrl}", ct);
                    else
                        await _telegramBot.SendMessageAsync(agencyId, entry.Content?.ProjectId, chatId,
                            $"❌ <b>Pubblicazione fallita su {entry.Platform.Value}</b>\n{title}\n{result.Error}", ct);
                }
                else
                {
                    await _telegramBot.SendMessageAsync(agencyId, null, chatId,
                        "⚠️ Nessuna piattaforma associata a questa voce.", ct);
                }
            }
        }
        else if (text.StartsWith("removecal_") || text.StartsWith("/removecal_"))
        {
            var entryIdStr = text.Replace("/removecal_", "").Replace("removecal_", "");
            if (Guid.TryParse(entryIdStr, out var entryId))
            {
                var entry = await _context.CalendarEntries
                    .IgnoreQueryFilters()
                    .Include(e => e.Content)
                    .FirstOrDefaultAsync(e => e.Id == entryId && e.AgencyId == agencyId, ct);

                if (entry != null && entry.Status == Domain.Enums.CalendarEntryStatus.Scheduled)
                {
                    _context.CalendarEntries.Remove(entry);
                    await _context.SaveChangesAsync(ct);
                    await _telegramBot.SendMessageAsync(agencyId, entry.Content?.ProjectId, chatId,
                        $"🗑 Programmazione rimossa: {entry.Content?.Title ?? "—"}", ct);
                }
            }
        }
        else if (text == "/daapprovare" || text == "daapprovare")
        {
            var pending = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .Where(c => c.AgencyId == agencyId && c.Status == Domain.Enums.ContentStatus.InReview)
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .ToListAsync(ct);

            if (pending.Count == 0)
            {
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, "✅ Nessun contenuto da approvare. Tutto aggiornato!", ct);
            }
            else
            {
                await _telegramBot.SendMessageAsync(agencyId, null, chatId,
                    $"📋 <b>Contenuti da approvare</b> ({pending.Count})\n", ct);
                foreach (var c in pending)
                {
                    var body = c.Body.Length > 300 ? c.Body[..300] + "..." : c.Body;
                    var caption = $"📝 <b>{c.Title}</b>\n\n{body}\n\n⭐ Score: {c.OverallScore:F1}/10";
                    var btns = new List<TelegramInlineButton>
                    {
                        new("✅ Approva", $"approve_{c.Id}"),
                        new("❌ Rifiuta", $"reject_{c.Id}")
                    };
                    if (!string.IsNullOrEmpty(c.ImageUrl))
                    {
                        await _telegramBot.SendPhotoAsync(agencyId, c.ProjectId, chatId, c.ImageUrl, caption, btns, ct);
                    }
                    else
                    {
                        await _telegramBot.SendMessageWithButtonsAsync(agencyId, c.ProjectId, chatId, caption, btns, ct);
                    }
                }
            }
        }
        else if (text == "/generati" || text == "generati")
        {
            var recent = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .Where(c => c.AgencyId == agencyId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .ToListAsync(ct);

            if (recent.Count == 0)
            {
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, "📝 Nessun contenuto generato.", ct);
            }
            else
            {
                var lines = new List<string> { $"📝 <b>Ultimi contenuti generati</b> ({recent.Count})\n" };
                foreach (var c in recent)
                {
                    var date = c.CreatedAt.ToString("dd/MM HH:mm");
                    lines.Add($"• <b>{date}</b> [{c.Status}] {c.Title}");
                }
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, string.Join("\n", lines), ct);
            }
        }
        else if (text == "/approvati" || text == "approvati")
        {
            var approved = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .Where(c => c.AgencyId == agencyId && c.Status == Domain.Enums.ContentStatus.Approved)
                .OrderByDescending(c => c.ApprovedAt)
                .Take(10)
                .ToListAsync(ct);

            if (approved.Count == 0)
            {
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, "✅ Nessun contenuto approvato.", ct);
            }
            else
            {
                var lines = new List<string> { $"✅ <b>Contenuti approvati</b> ({approved.Count})\n" };
                foreach (var c in approved)
                {
                    var date = (c.ApprovedAt ?? c.CreatedAt).ToString("dd/MM HH:mm");
                    lines.Add($"• <b>{date}</b> {c.Title}");
                }
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, string.Join("\n", lines), ct);
            }
        }
        else if (text == "/menu" || text == "menu")
        {
            var projects = await _context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.AgencyId == agencyId)
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

            var buttons = new List<TelegramInlineButton>
            {
                new("📋 Da approvare", "daapprovare"),
                new("📊 Status Agenzia", "status"),
                new("⚙️ Tutti gli Scheduler", "scheduler"),
                new("📅 Tutti i programmati", "programmati"),
                new("📝 Tutti i generati", "generati"),
                new("✅ Tutti gli approvati", "approvati")
            };
            foreach (var p in projects)
                buttons.Add(new TelegramInlineButton($"📁 {p.Name}", $"project_{p.Id}"));

            await _telegramBot.SendMessageWithButtonsAsync(agencyId, null, chatId,
                "<b>🤖 Menu AI Marketing Agency</b>\n\nScegli un'opzione o un progetto:", buttons, ct);
        }
        else if (text.StartsWith("project_") && !text.Contains("_programmati") && !text.Contains("_approvati") && !text.Contains("_generati") && !text.Contains("_pubblicati") && !text.Contains("_scheduler") && !text.Contains("_daapprovare"))
        {
            var projectIdStr = text.Replace("project_", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var project = await _context.Projects
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId, ct);

                if (project != null)
                {
                    var buttons = new List<TelegramInlineButton>
                    {
                        new("📋 Da approvare", $"project_{projectId}_daapprovare"),
                        new("⚙️ Scheduler", $"project_{projectId}_scheduler"),
                        new("📅 Programmati", $"project_{projectId}_programmati"),
                        new("📝 Generati", $"project_{projectId}_generati"),
                        new("✅ Approvati", $"project_{projectId}_approvati"),
                        new("📢 Pubblicati", $"project_{projectId}_pubblicati"),
                        new("⬅️ Menu", "menu")
                    };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId,
                        $"<b>📁 {project.Name}</b>\n\nScegli cosa visualizzare:", buttons, ct);
                }
            }
        }
        else if (text.Contains("_programmati") && text.StartsWith("project_"))
        {
            var projectIdStr = text.Replace("project_", "").Replace("_programmati", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var scheduled = await _context.CalendarEntries
                    .IgnoreQueryFilters()
                    .Include(e => e.Content)
                    .Where(e => e.AgencyId == agencyId && e.Content!.ProjectId == projectId && e.Status == Domain.Enums.CalendarEntryStatus.Scheduled)
                    .OrderBy(e => e.ScheduledAt)
                    .Take(10)
                    .ToListAsync(ct);

                var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == projectId, ct);
                var projectName = project?.Name ?? "Progetto";

                if (scheduled.Count == 0)
                {
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, $"📅 Nessun post programmato in <b>{projectName}</b>.", btns, ct);
                }
                else
                {
                    foreach (var e in scheduled)
                    {
                        var platform = e.Platform?.ToString() ?? "N/A";
                        var date = e.ScheduledAt.ToString("dd/MM/yyyy HH:mm");
                        var title = e.Content?.Title ?? "—";
                        var msg = $"📅 <b>{title}</b>\n\n🕐 {date}\n📱 {platform}\n📁 {projectName}";
                        var btns = new List<TelegramInlineButton>
                        {
                            new("🚀 Pubblica ora", $"publish_{e.Id}"),
                            new("🗑 Rimuovi", $"removecal_{e.Id}"),
                            new("⬅️ Torna al progetto", $"project_{projectId}")
                        };
                        await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, msg, btns, ct);
                    }
                }
            }
        }
        else if (text.Contains("_generati") && text.StartsWith("project_"))
        {
            var projectIdStr = text.Replace("project_", "").Replace("_generati", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var contents = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .ToListAsync(ct);

                var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == projectId, ct);
                var projectName = project?.Name ?? "Progetto";

                if (contents.Count == 0)
                {
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, $"📝 Nessun contenuto in <b>{projectName}</b>.", btns, ct);
                }
                else
                {
                    var lines = new List<string> { $"📝 <b>Contenuti — {projectName}</b> ({contents.Count})\n" };
                    foreach (var c in contents)
                        lines.Add($"• <b>{c.CreatedAt:dd/MM HH:mm}</b> [{c.Status}] {c.Title}");
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, string.Join("\n", lines), btns, ct);
                }
            }
        }
        else if (text.Contains("_daapprovare") && text.StartsWith("project_"))
        {
            var projectIdStr = text.Replace("project_", "").Replace("_daapprovare", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var pending = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId && c.Status == Domain.Enums.ContentStatus.InReview)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .ToListAsync(ct);

                var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == projectId, ct);
                var projectName = project?.Name ?? "Progetto";

                if (pending.Count == 0)
                {
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId,
                        $"✅ Nessun contenuto da approvare in <b>{projectName}</b>.", btns, ct);
                }
                else
                {
                    await _telegramBot.SendMessageAsync(agencyId, projectId, chatId,
                        $"📋 <b>Da approvare — {projectName}</b> ({pending.Count})\n", ct);
                    foreach (var c in pending)
                    {
                        var body = c.Body.Length > 300 ? c.Body[..300] + "..." : c.Body;
                        var caption = $"📝 <b>{c.Title}</b>\n\n{body}\n\n⭐ Score: {c.OverallScore:F1}/10";
                        var btns = new List<TelegramInlineButton>
                        {
                            new("✅ Approva", $"approve_{c.Id}"),
                            new("❌ Rifiuta", $"reject_{c.Id}"),
                            new("⬅️ Torna al progetto", $"project_{projectId}")
                        };
                        if (!string.IsNullOrEmpty(c.ImageUrl))
                        {
                            await _telegramBot.SendPhotoAsync(agencyId, projectId, chatId, c.ImageUrl, caption, btns, ct);
                        }
                        else
                        {
                            await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, caption, btns, ct);
                        }
                    }
                }
            }
        }
        else if (text.Contains("_approvati") && text.StartsWith("project_"))
        {
            var projectIdStr = text.Replace("project_", "").Replace("_approvati", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var approved = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId && c.Status == Domain.Enums.ContentStatus.Approved)
                    .OrderByDescending(c => c.ApprovedAt)
                    .Take(10)
                    .ToListAsync(ct);

                var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == projectId, ct);
                var projectName = project?.Name ?? "Progetto";

                if (approved.Count == 0)
                {
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, $"✅ Nessun contenuto approvato in <b>{projectName}</b>.", btns, ct);
                }
                else
                {
                    var lines = new List<string> { $"✅ <b>Approvati — {projectName}</b> ({approved.Count})\n" };
                    foreach (var c in approved)
                        lines.Add($"• <b>{(c.ApprovedAt ?? c.CreatedAt):dd/MM HH:mm}</b> {c.Title}");
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, string.Join("\n", lines), btns, ct);
                }
            }
        }
        else if (text.Contains("_pubblicati") && text.StartsWith("project_"))
        {
            var projectIdStr = text.Replace("project_", "").Replace("_pubblicati", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var published = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .Where(c => c.AgencyId == agencyId && c.ProjectId == projectId && c.Status == Domain.Enums.ContentStatus.Published)
                    .OrderByDescending(c => c.PublishedAt)
                    .Take(10)
                    .ToListAsync(ct);

                var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == projectId, ct);
                var projectName = project?.Name ?? "Progetto";

                if (published.Count == 0)
                {
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, $"📢 Nessun contenuto pubblicato in <b>{projectName}</b>.", btns, ct);
                }
                else
                {
                    var lines = new List<string> { $"📢 <b>Pubblicati — {projectName}</b> ({published.Count})\n" };
                    foreach (var c in published)
                    {
                        var date = (c.PublishedAt ?? c.CreatedAt).ToString("dd/MM HH:mm");
                        lines.Add($"• <b>{date}</b> {c.Title}");
                    }
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, string.Join("\n", lines), btns, ct);
                }
            }
        }
        else if (text.Contains("_scheduler") && text.StartsWith("project_"))
        {
            var projectIdStr = text.Replace("project_", "").Replace("_scheduler", "");
            if (Guid.TryParse(projectIdStr, out var projectId))
            {
                var schedules = await _context.ContentSchedules
                    .IgnoreQueryFilters()
                    .Include(s => s.Project)
                    .Where(s => s.AgencyId == agencyId && s.ProjectId == projectId)
                    .OrderBy(s => s.Name)
                    .ToListAsync(ct);

                var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == projectId, ct);
                var projectName = project?.Name ?? "Progetto";

                if (schedules.Count == 0)
                {
                    var btns = new List<TelegramInlineButton> { new("⬅️ Torna al progetto", $"project_{projectId}") };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId,
                        $"⚙️ Nessuno scheduler per <b>{projectName}</b>.", btns, ct);
                }
                else
                {
                    foreach (var s in schedules)
                    {
                        var typeLabel = s.ScheduleType == Domain.Enums.ScheduleType.Publication ? "📢 Pubblicazione" : "📝 Generazione";
                        var statusIcon = s.IsActive ? "🟢" : "🔴";
                        var nextRun = s.NextRunAt?.ToString("dd/MM/yyyy HH:mm") ?? "—";
                        var lastRun = s.LastRunAt?.ToString("dd/MM/yyyy HH:mm") ?? "Mai";

                        var msg = $"{statusIcon} <b>{s.Name}</b>\n\n"
                            + $"Tipo: {typeLabel}\n"
                            + $"Progetto: {projectName}\n"
                            + $"Orario: {s.TimeOfDay:hh\\:mm} ({s.TimeZone})\n"
                            + $"Ultima esecuzione: {lastRun}\n"
                            + $"Prossima esecuzione: {nextRun}";

                        var btns = new List<TelegramInlineButton>
                        {
                            new("🚀 Avvia ora", $"runnow_{s.Id}"),
                            new(s.IsActive ? "⏸ Pausa" : "▶️ Attiva", $"togglesched_{s.Id}"),
                            new("⬅️ Torna al progetto", $"project_{projectId}")
                        };
                        await _telegramBot.SendMessageWithButtonsAsync(agencyId, projectId, chatId, msg, btns, ct);
                    }
                }
            }
        }
        else if (text == "/scheduler" || text == "scheduler")
        {
            _logger.LogInformation("Scheduler command hit for agency {AgencyId}", agencyId);
            var schedules = await _context.ContentSchedules
                .IgnoreQueryFilters()
                .Include(s => s.Project)
                .Where(s => s.AgencyId == agencyId)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);

            _logger.LogInformation("Found {Count} schedules", schedules.Count);

            if (schedules.Count == 0)
            {
                await _telegramBot.SendMessageAsync(agencyId, null, chatId, "⚙️ Nessuno scheduler configurato.", ct);
            }
            else
            {
                foreach (var s in schedules)
                {
                    var typeLabel = s.ScheduleType == Domain.Enums.ScheduleType.Publication ? "📢 Pubblicazione" : "📝 Generazione";
                    var statusIcon = s.IsActive ? "🟢" : "🔴";
                    var projectName = s.Project?.Name ?? "Tutta l'agenzia";
                    var nextRun = s.NextRunAt?.ToString("dd/MM/yyyy HH:mm") ?? "—";
                    var lastRun = s.LastRunAt?.ToString("dd/MM/yyyy HH:mm") ?? "Mai";

                    var msg = $"{statusIcon} <b>{s.Name}</b>\n\n"
                        + $"Tipo: {typeLabel}\n"
                        + $"Progetto: {projectName}\n"
                        + $"Orario: {s.TimeOfDay:hh\\:mm} ({s.TimeZone})\n"
                        + $"Ultima esecuzione: {lastRun}\n"
                        + $"Prossima esecuzione: {nextRun}";

                    var btns = new List<TelegramInlineButton>
                    {
                        new("🚀 Avvia ora", $"runnow_{s.Id}"),
                        new(s.IsActive ? "⏸ Pausa" : "▶️ Attiva", $"togglesched_{s.Id}")
                    };
                    await _telegramBot.SendMessageWithButtonsAsync(agencyId, s.ProjectId, chatId, msg, btns, ct);
                }
            }
        }
        else if (text.StartsWith("runnow_") || text.StartsWith("/runnow_"))
        {
            var schedIdStr = text.Replace("/runnow_", "").Replace("runnow_", "");
            if (Guid.TryParse(schedIdStr, out var schedId))
            {
                var schedule = await _context.ContentSchedules
                    .IgnoreQueryFilters()
                    .Include(s => s.Agency)
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == schedId && s.AgencyId == agencyId, ct);

                if (schedule == null)
                {
                    await _telegramBot.SendMessageAsync(agencyId, null, chatId, "⚠️ Scheduler non trovato.", ct);
                }
                else
                {
                    await _telegramBot.SendMessageAsync(agencyId, schedule.ProjectId, chatId,
                        $"⏳ Avvio <b>{schedule.Name}</b> in corso...", ct);

                    var tenantCtx = HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                    tenantCtx.SetTenant(schedule.TenantId, Guid.Empty);

                    if (schedule.ScheduleType == Domain.Enums.ScheduleType.Publication)
                    {
                        // For publication, we trigger the CalendarPublishBackgroundService logic inline
                        // Find scheduled calendar entries and publish them
                        var dueEntries = await _context.CalendarEntries
                            .IgnoreQueryFilters()
                            .Include(e => e.Content)
                            .Where(e => e.AgencyId == agencyId
                                && e.Status == Domain.Enums.CalendarEntryStatus.Scheduled
                                && (schedule.ProjectId == null || e.Content!.ProjectId == schedule.ProjectId))
                            .Take(10)
                            .ToListAsync(ct);

                        if (dueEntries.Count == 0)
                        {
                            await _telegramBot.SendMessageAsync(agencyId, schedule.ProjectId, chatId,
                                "📢 Nessun post da pubblicare al momento. Genera prima dei contenuti.", ct);
                        }
                        else
                        {
                            var mediator = HttpContext.RequestServices.GetRequiredService<MediatR.IMediator>();
                            int ok = 0, fail = 0;
                            foreach (var entry in dueEntries.Where(e => e.Platform != null))
                            {
                                try
                                {
                                    var result = await mediator.Send(
                                        new Application.SocialConnectors.Commands.PublishContent.PublishContentCommand(
                                            entry.AgencyId, entry.ContentId, entry.Platform!.Value), ct);
                                    entry.Status = result.Success ? Domain.Enums.CalendarEntryStatus.Published : Domain.Enums.CalendarEntryStatus.Failed;
                                    entry.PublishedAt = result.Success ? DateTime.UtcNow : null;
                                    entry.ErrorMessage = result.Success ? null : result.Error;
                                    entry.PostUrl = result.Success ? result.PostUrl : entry.PostUrl;
                                    if (result.Success) ok++; else fail++;
                                }
                                catch { fail++; }
                            }
                            await _context.SaveChangesAsync(ct);
                            await _telegramBot.SendMessageAsync(agencyId, schedule.ProjectId, chatId,
                                $"📢 Pubblicazione completata: {ok} riuscit{(ok == 1 ? "a" : "e")}, {fail} fallite.", ct);
                        }
                    }
                    else
                    {
                        var job = new AgentJob
                        {
                            TenantId = schedule.TenantId,
                            AgencyId = schedule.AgencyId,
                            AgentType = schedule.AgentType,
                            Status = Domain.Enums.JobStatus.Queued,
                            Input = schedule.Input,
                            ProjectId = schedule.ProjectId,
                            ScheduleId = schedule.Id
                        };
                        _context.AgentJobs.Add(job);
                        await _context.SaveChangesAsync(ct);

                        var jobQueue = HttpContext.RequestServices.GetRequiredService<IBackgroundJobQueue>();
                        jobQueue.Enqueue(job.Id, schedule.TenantId);

                        await _telegramBot.SendMessageAsync(agencyId, schedule.ProjectId, chatId,
                            $"🚀 <b>{schedule.Name}</b> avviato! Riceverai una notifica quando completato.", ct);
                    }
                }
            }
        }
        else if (text.StartsWith("togglesched_") || text.StartsWith("/togglesched_"))
        {
            var schedIdStr = text.Replace("/togglesched_", "").Replace("togglesched_", "");
            if (Guid.TryParse(schedIdStr, out var schedId))
            {
                var schedule = await _context.ContentSchedules
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id == schedId && s.AgencyId == agencyId, ct);

                if (schedule != null)
                {
                    schedule.IsActive = !schedule.IsActive;
                    await _context.SaveChangesAsync(ct);
                    var status = schedule.IsActive ? "🟢 attivato" : "🔴 in pausa";
                    await _telegramBot.SendMessageAsync(agencyId, schedule.ProjectId, chatId,
                        $"⚙️ <b>{schedule.Name}</b> {status}", ct);
                }
            }
        }
        else if (text == "/help" || text == "/start")
        {
            if (text == "/start")
            {
                var alreadyConnected = await _context.TelegramConnections
                    .IgnoreQueryFilters()
                    .AnyAsync(c => c.AgencyId == agencyId && c.ChatId == chatId, ct);

                if (!alreadyConnected)
                {
                    var agency = await _context.Agencies
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(a => a.Id == agencyId, ct);

                    if (agency != null)
                    {
                        _context.TelegramConnections.Add(new TelegramConnection
                        {
                            TenantId = agency.TenantId,
                            AgencyId = agencyId,
                            ChatId = chatId,
                            ChatTitle = chatTitle ?? "",
                            Username = username ?? "",
                            NotifyOnContentGenerated = true,
                            NotifyOnApprovalNeeded = true,
                            NotifyOnPublished = true,
                            AllowCommands = true,
                            IsActive = true
                        });
                        await _context.SaveChangesAsync(ct);
                    }
                }
            }

            var help = "<b>🤖 AI Marketing Agency Bot</b>\n\n"
                + "Comandi disponibili:\n"
                + "/menu — Menu interattivo con progetti\n"
                + "/daapprovare — Contenuti da approvare (con bottoni)\n"
                + "/scheduler — Vedi e gestisci gli scheduler\n"
                + "/status — Contenuti in attesa di approvazione\n"
                + "/programmati — Post programmati con azione pubblica\n"
                + "/generati — Ultimi contenuti generati\n"
                + "/approvati — Contenuti approvati\n"
                + "/help — Mostra questo messaggio";
            await _telegramBot.SendMessageAsync(agencyId, null, chatId, help, ct);
        }
    }
}

// Telegram webhook models
public class TelegramUpdate
{
    [JsonPropertyName("update_id")]
    public int UpdateId { get; set; }
    [JsonPropertyName("message")]
    public TelegramMessage? Message { get; set; }
    [JsonPropertyName("callback_query")]
    public TelegramCallbackQuery? CallbackQuery { get; set; }
}

public class TelegramCallbackQuery
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("from")]
    public TelegramUser? From { get; set; }
    [JsonPropertyName("message")]
    public TelegramMessage? Message { get; set; }
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

public class TelegramMessage
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }
    [JsonPropertyName("chat")]
    public TelegramChat Chat { get; set; } = null!;
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("from")]
    public TelegramUser? From { get; set; }
}

public class TelegramChat
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class TelegramUser
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
}

public record ConnectTelegramRequest(
    long ChatId,
    string? ChatTitle,
    string? Username,
    Guid? ProjectId = null,
    bool NotifyOnContentGenerated = true,
    bool NotifyOnApprovalNeeded = true,
    bool NotifyOnPublished = true,
    bool AllowCommands = true);

public record SetTelegramBotRequest(string? Token, string? BotUsername);

public class TelegramBotInfoDto
{
    public bool HasToken { get; set; }
    public string? BotUsername { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
}

public class TelegramWebhookRegistrationDto
{
    public bool Ok { get; set; }
    public string? Description { get; set; }
    public string? BotUsername { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
}

public class TelegramConnectionDto
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public string? Username { get; set; }
    public bool NotifyOnContentGenerated { get; set; }
    public bool NotifyOnApprovalNeeded { get; set; }
    public bool NotifyOnPublished { get; set; }
    public bool AllowCommands { get; set; }
    public bool IsActive { get; set; }
}
