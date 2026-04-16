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

    private string? FallbackBotToken => _configuration["Telegram:BotToken"];

    // Resolution order: project bot → agency bot → global fallback
    private async Task<string?> ResolveTokenAsync(Guid agencyId, Guid? projectId, CancellationToken ct)
    {
        if (projectId.HasValue)
        {
            var projectToken = await _context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.Id == projectId.Value)
                .Select(p => p.TelegramBotToken)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(projectToken))
                return projectToken;
        }

        var agencyToken = await _context.Agencies
            .IgnoreQueryFilters()
            .Where(a => a.Id == agencyId)
            .Select(a => a.TelegramBotToken)
            .FirstOrDefaultAsync(ct);

        return !string.IsNullOrWhiteSpace(agencyToken) ? agencyToken : FallbackBotToken;
    }

    public async Task SendMessageAsync(Guid agencyId, Guid? projectId, long chatId, string message, CancellationToken ct = default)
    {
        var token = await ResolveTokenAsync(agencyId, projectId, ct);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No Telegram bot token configured for agency {AgencyId} project {ProjectId}.", agencyId, projectId);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            await client.PostAsJsonAsync($"https://api.telegram.org/bot{token}/sendMessage", new
            {
                chat_id = chatId,
                text = message,
                parse_mode = "HTML"
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Telegram message to chat {ChatId} for agency {AgencyId}", chatId, agencyId);
        }
    }

    public async Task<TelegramWebhookResult> RegisterWebhookAsync(string token, string webhookUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new TelegramWebhookResult(false, "Token mancante", null);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var setResp = await client.PostAsJsonAsync($"https://api.telegram.org/bot{token}/setWebhook", new
            {
                url = webhookUrl,
                allowed_updates = new[] { "message", "callback_query" }
            }, ct);
            var setJson = await setResp.Content.ReadFromJsonAsync<TelegramApiResponse>(cancellationToken: ct);
            if (setJson == null || !setJson.Ok)
                return new TelegramWebhookResult(false, setJson?.Description ?? "setWebhook failed", null);

            var meResp = await client.GetAsync($"https://api.telegram.org/bot{token}/getMe", ct);
            var meJson = await meResp.Content.ReadFromJsonAsync<TelegramGetMeResponse>(cancellationToken: ct);
            var botUsername = meJson?.Result?.Username;

            return new TelegramWebhookResult(true, setJson.Description ?? "Webhook registered", botUsername);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register Telegram webhook");
            return new TelegramWebhookResult(false, ex.Message, null);
        }
    }

    private class TelegramApiResponse
    {
        public bool Ok { get; set; }
        public string? Description { get; set; }
    }

    private class TelegramGetMeResponse
    {
        public bool Ok { get; set; }
        public TelegramMe? Result { get; set; }
    }

    private class TelegramMe
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
    }

    public async Task NotifyAgencyAsync(Guid agencyId, Guid? projectId, string message, CancellationToken ct = default)
    {
        // Per-project first with agency-default fallback (same pattern as newsletter/social)
        var connections = await _context.TelegramConnections
            .IgnoreQueryFilters()
            .Where(c => c.AgencyId == agencyId && c.IsActive
                        && (c.ProjectId == projectId || c.ProjectId == null))
            .ToListAsync(ct);

        // If we have any project-specific matches, drop the agency defaults
        var projectSpecific = connections.Where(c => c.ProjectId == projectId && projectId != null).ToList();
        var effective = projectSpecific.Any() ? projectSpecific : connections.Where(c => c.ProjectId == null).ToList();

        foreach (var conn in effective)
        {
            await SendMessageAsync(agencyId, projectId, conn.ChatId, message, ct);
        }
    }
}
