namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ITelegramBotService
{
    Task SendMessageAsync(Guid agencyId, Guid? projectId, long chatId, string message, CancellationToken ct = default);
    Task NotifyAgencyAsync(Guid agencyId, Guid? projectId, string message, CancellationToken ct = default);
    Task<TelegramWebhookResult> RegisterWebhookAsync(string token, string webhookUrl, CancellationToken ct = default);
}

public record TelegramWebhookResult(bool Ok, string? Description, string? BotUsername);
