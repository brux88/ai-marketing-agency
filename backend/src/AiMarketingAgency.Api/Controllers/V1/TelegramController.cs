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
        if (update?.Message?.Text == null) return Ok();

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text;

        _logger.LogInformation("Telegram update for agency {AgencyId} from chat {ChatId}: {Text}", agencyId, chatId, text);

        if (text.StartsWith("/approve_"))
        {
            var contentIdStr = text.Replace("/approve_", "");
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
                    await _telegramBot.SendMessageAsync(agencyId, content.ProjectId, chatId, $"Contenuto '<b>{content.Title}</b>' approvato!", ct);
                }
            }
        }
        else if (text.StartsWith("/reject_"))
        {
            var contentIdStr = text.Replace("/reject_", "");
            if (Guid.TryParse(contentIdStr, out var contentId))
            {
                var content = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == contentId && c.AgencyId == agencyId, ct);

                if (content != null && content.Status == Domain.Enums.ContentStatus.InReview)
                {
                    content.Status = Domain.Enums.ContentStatus.Rejected;
                    await _context.SaveChangesAsync(ct);
                    await _telegramBot.SendMessageAsync(agencyId, content.ProjectId, chatId, $"Contenuto '<b>{content.Title}</b>' rifiutato.", ct);
                }
            }
        }
        else if (text == "/status")
        {
            var pendingCount = await _context.GeneratedContents
                .IgnoreQueryFilters()
                .CountAsync(c => c.AgencyId == agencyId && c.Status == Domain.Enums.ContentStatus.InReview, ct);

            await _telegramBot.SendMessageAsync(agencyId, null, chatId,
                $"<b>Status Agenzia</b>\n\nContenuti in attesa di approvazione: {pendingCount}", ct);
        }

        return Ok();
    }
}

// Telegram webhook models
public class TelegramUpdate
{
    public int UpdateId { get; set; }
    public TelegramMessage? Message { get; set; }
}

public class TelegramMessage
{
    public int MessageId { get; set; }
    public TelegramChat Chat { get; set; } = null!;
    public string? Text { get; set; }
    public TelegramUser? From { get; set; }
}

public class TelegramChat
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
}

public class TelegramUser
{
    public long Id { get; set; }
    public string? Username { get; set; }
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
