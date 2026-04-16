using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        if (string.IsNullOrEmpty(token)) return;

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
            _logger.LogWarning(ex, "Failed to send Telegram message to chat {ChatId}", chatId);
        }
    }

    private static string BuildKeyboardJson(IEnumerable<TelegramInlineButton> buttons)
    {
        var rows = buttons.Select(b =>
            $"[{{\"text\":{JsonSerializer.Serialize(b.Text)},\"callback_data\":{JsonSerializer.Serialize(b.CallbackData)}}}]");
        return "{\"inline_keyboard\":[" + string.Join(",", rows) + "]}";
    }

    private async Task<bool> PostTelegramJsonAsync(string token, string method, string json, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync($"https://api.telegram.org/bot{token}/{method}", content, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Telegram API {Method} failed: {Status} {Body}", method, resp.StatusCode, body);
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task SendMessageWithButtonsAsync(Guid agencyId, Guid? projectId, long chatId, string message, IEnumerable<TelegramInlineButton> buttons, CancellationToken ct = default)
    {
        var token = await ResolveTokenAsync(agencyId, projectId, ct);
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var json = $"{{\"chat_id\":{chatId},\"text\":{JsonSerializer.Serialize(message)},\"parse_mode\":\"HTML\",\"reply_markup\":{BuildKeyboardJson(buttons)}}}";
            await PostTelegramJsonAsync(token, "sendMessage", json, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Telegram message with buttons to chat {ChatId}", chatId);
        }
    }

    public async Task SendPhotoAsync(Guid agencyId, Guid? projectId, long chatId, string photoUrl, string? caption, IEnumerable<TelegramInlineButton>? buttons = null, CancellationToken ct = default)
    {
        var token = await ResolveTokenAsync(agencyId, projectId, ct);
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var replyMarkupJson = buttons?.Any() == true ? BuildKeyboardJson(buttons) : null;
            bool ok;

            if (photoUrl.StartsWith("/"))
            {
                var webRoot = _configuration["WebRootPath"] ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                var filePath = Path.Combine(webRoot, photoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(filePath))
                {
                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(chatId.ToString()), "chat_id");
                    form.Add(new StringContent(caption ?? ""), "caption");
                    form.Add(new StringContent("HTML"), "parse_mode");
                    if (replyMarkupJson != null)
                        form.Add(new StringContent(replyMarkupJson), "reply_markup");
                    var fileBytes = await File.ReadAllBytesAsync(filePath, ct);
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                    form.Add(fileContent, "photo", Path.GetFileName(filePath));
                    var resp = await client.PostAsync($"https://api.telegram.org/bot{token}/sendPhoto", form, ct);
                    ok = resp.IsSuccessStatusCode;
                    if (!ok)
                    {
                        var body = await resp.Content.ReadAsStringAsync(ct);
                        _logger.LogWarning("Telegram sendPhoto (upload) failed: {Status} {Body}", resp.StatusCode, body);
                    }
                }
                else
                {
                    _logger.LogWarning("Local image not found: {Path}", filePath);
                    ok = false;
                }
            }
            else
            {
                var json = $"{{\"chat_id\":{chatId},\"photo\":{JsonSerializer.Serialize(photoUrl)},\"caption\":{JsonSerializer.Serialize(caption ?? "")},\"parse_mode\":\"HTML\"{(replyMarkupJson != null ? $",\"reply_markup\":{replyMarkupJson}" : "")}}}";
                ok = await PostTelegramJsonAsync(token, "sendPhoto", json, ct);
            }

            if (!ok && !string.IsNullOrEmpty(caption))
            {
                if (buttons?.Any() == true)
                    await SendMessageWithButtonsAsync(agencyId, projectId, chatId, caption, buttons, ct);
                else
                    await SendMessageAsync(agencyId, projectId, chatId, caption, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Telegram photo to chat {ChatId}", chatId);
            if (!string.IsNullOrEmpty(caption))
            {
                if (buttons?.Any() == true)
                    await SendMessageWithButtonsAsync(agencyId, projectId, chatId, caption, buttons, ct);
                else
                    await SendMessageAsync(agencyId, projectId, chatId, caption, ct);
            }
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

    private async Task<List<Domain.Entities.TelegramConnection>> GetEffectiveConnectionsAsync(Guid agencyId, Guid? projectId, CancellationToken ct)
    {
        var connections = await _context.TelegramConnections
            .IgnoreQueryFilters()
            .Where(c => c.AgencyId == agencyId && c.IsActive
                        && (c.ProjectId == projectId || c.ProjectId == null))
            .ToListAsync(ct);

        var projectSpecific = connections.Where(c => c.ProjectId == projectId && projectId != null).ToList();
        return projectSpecific.Any() ? projectSpecific : connections.Where(c => c.ProjectId == null).ToList();
    }

    public async Task NotifyAgencyAsync(Guid agencyId, Guid? projectId, string message, CancellationToken ct = default)
    {
        var effective = await GetEffectiveConnectionsAsync(agencyId, projectId, ct);
        foreach (var conn in effective)
            await SendMessageAsync(agencyId, projectId, conn.ChatId, message, ct);
    }

    public async Task NotifyAgencyWithContentAsync(Guid agencyId, Guid? projectId, string message, string? imageUrl, IEnumerable<TelegramInlineButton>? buttons = null, CancellationToken ct = default)
    {
        var effective = await GetEffectiveConnectionsAsync(agencyId, projectId, ct);
        foreach (var conn in effective)
        {
            if (!string.IsNullOrEmpty(imageUrl))
                await SendPhotoAsync(agencyId, projectId, conn.ChatId, imageUrl, message, buttons, ct);
            else if (buttons?.Any() == true)
                await SendMessageWithButtonsAsync(agencyId, projectId, conn.ChatId, message, buttons, ct);
            else
                await SendMessageAsync(agencyId, projectId, conn.ChatId, message, ct);
        }
    }
}
