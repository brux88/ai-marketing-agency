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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TelegramConnectionDto>>>> GetConnections(
        Guid agencyId, CancellationToken ct)
    {
        var connections = await _context.TelegramConnections
            .Where(c => c.AgencyId == agencyId)
            .Select(c => new TelegramConnectionDto
            {
                Id = c.Id,
                ChatId = c.ChatId,
                ChatTitle = c.ChatTitle,
                Username = c.Username,
                NotifyOnContentGenerated = c.NotifyOnContentGenerated,
                NotifyOnApprovalNeeded = c.NotifyOnApprovalNeeded,
                NotifyOnPublished = c.NotifyOnPublished,
                AllowCommands = c.AllowCommands,
                IsActive = c.IsActive
            })
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

        // Send welcome message
        await _telegramBot.SendMessageAsync(request.ChatId,
            "<b>Connesso ad AI Marketing Agency!</b>\n\nRiceverai notifiche per questa agenzia.", ct);

        return Ok(ApiResponse<TelegramConnectionDto>.Ok(new TelegramConnectionDto
        {
            Id = connection.Id,
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
        await _telegramBot.NotifyAgencyAsync(agencyId, "<b>Test</b>\n\nLa connessione Telegram funziona correttamente!", ct);
        return Ok(new { success = true });
    }
}

// Webhook endpoint for Telegram bot updates (unauthenticated)
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

    [HttpPost]
    public async Task<ActionResult> HandleUpdate([FromBody] TelegramUpdate update, CancellationToken ct)
    {
        if (update?.Message?.Text == null) return Ok();

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text;

        _logger.LogInformation("Telegram update from chat {ChatId}: {Text}", chatId, text);

        // Handle commands
        if (text.StartsWith("/approve_"))
        {
            var contentIdStr = text.Replace("/approve_", "");
            if (Guid.TryParse(contentIdStr, out var contentId))
            {
                var content = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == contentId, ct);

                if (content != null && content.Status == Domain.Enums.ContentStatus.InReview)
                {
                    content.Status = Domain.Enums.ContentStatus.Approved;
                    content.ApprovedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(ct);
                    await _telegramBot.SendMessageAsync(chatId, $"Contenuto '<b>{content.Title}</b>' approvato!", ct);
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
                    .FirstOrDefaultAsync(c => c.Id == contentId, ct);

                if (content != null && content.Status == Domain.Enums.ContentStatus.InReview)
                {
                    content.Status = Domain.Enums.ContentStatus.Rejected;
                    await _context.SaveChangesAsync(ct);
                    await _telegramBot.SendMessageAsync(chatId, $"Contenuto '<b>{content.Title}</b>' rifiutato.", ct);
                }
            }
        }
        else if (text == "/status")
        {
            var connection = await _context.TelegramConnections
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.ChatId == chatId && c.IsActive, ct);

            if (connection != null)
            {
                var pendingCount = await _context.GeneratedContents
                    .IgnoreQueryFilters()
                    .CountAsync(c => c.AgencyId == connection.AgencyId && c.Status == Domain.Enums.ContentStatus.InReview, ct);

                await _telegramBot.SendMessageAsync(chatId,
                    $"<b>Status Agenzia</b>\n\nContenuti in attesa di approvazione: {pendingCount}", ct);
            }
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
    bool NotifyOnContentGenerated = true,
    bool NotifyOnApprovalNeeded = true,
    bool NotifyOnPublished = true,
    bool AllowCommands = true);

public class TelegramConnectionDto
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public string? Username { get; set; }
    public bool NotifyOnContentGenerated { get; set; }
    public bool NotifyOnApprovalNeeded { get; set; }
    public bool NotifyOnPublished { get; set; }
    public bool AllowCommands { get; set; }
    public bool IsActive { get; set; }
}
