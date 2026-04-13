using System.Net.Http.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Telegram;

public class TelegramBotService : ITelegramBotService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(
        IHttpClientFactory httpClientFactory,
        IAppDbContext context,
        IConfiguration configuration,
        ILogger<TelegramBotService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    private string BotToken => _configuration["Telegram:BotToken"] ?? "";
    private string BaseUrl => $"https://api.telegram.org/bot{BotToken}";

    public async Task SendMessageAsync(long chatId, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(BotToken)) return;

        try
        {
            var client = _httpClientFactory.CreateClient();
            await client.PostAsJsonAsync($"{BaseUrl}/sendMessage", new
            {
                chat_id = chatId,
                text = message,
                parse_mode = "HTML"
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Telegram message to chat {ChatId}", chatId);
        }
    }

    public async Task SendContentForApprovalAsync(long chatId, Guid contentId, string title, string preview, decimal score, CancellationToken ct = default)
    {
        var message = $"<b>Nuovo contenuto da approvare</b>\n\n" +
                      $"<b>{title}</b>\n" +
                      $"Score: {score}/10\n\n" +
                      $"{preview[..Math.Min(preview.Length, 300)]}...\n\n" +
                      $"/approve_{contentId:N}\n" +
                      $"/reject_{contentId:N}";

        await SendMessageAsync(chatId, message, ct);
    }

    public async Task SendPublishNotificationAsync(long chatId, string title, string platform, string? postUrl, CancellationToken ct = default)
    {
        var message = $"<b>Contenuto pubblicato!</b>\n\n" +
                      $"<b>{title}</b>\n" +
                      $"Piattaforma: {platform}\n" +
                      (postUrl != null ? $"Link: {postUrl}" : "");

        await SendMessageAsync(chatId, message, ct);
    }

    public async Task NotifyAgencyAsync(Guid agencyId, string message, CancellationToken ct = default)
    {
        var connections = await _context.TelegramConnections
            .Where(c => c.AgencyId == agencyId && c.IsActive)
            .ToListAsync(ct);

        foreach (var conn in connections)
        {
            await SendMessageAsync(conn.ChatId, message, ct);
        }
    }
}
