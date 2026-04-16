namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ITelegramBotService
{
    Task SendMessageAsync(Guid agencyId, Guid? projectId, long chatId, string message, CancellationToken ct = default);
    Task SendMessageWithButtonsAsync(Guid agencyId, Guid? projectId, long chatId, string message, IEnumerable<TelegramInlineButton> buttons, CancellationToken ct = default);
    Task SendPhotoAsync(Guid agencyId, Guid? projectId, long chatId, string photoUrl, string? caption, IEnumerable<TelegramInlineButton>? buttons = null, CancellationToken ct = default);
    Task NotifyAgencyAsync(Guid agencyId, Guid? projectId, string message, CancellationToken ct = default);
    Task NotifyAgencyWithContentAsync(Guid agencyId, Guid? projectId, string message, string? imageUrl, IEnumerable<TelegramInlineButton>? buttons = null, CancellationToken ct = default);
    Task<TelegramWebhookResult> RegisterWebhookAsync(string token, string webhookUrl, CancellationToken ct = default);
}

public record TelegramInlineButton(string Text, string CallbackData);

public record TelegramWebhookResult(bool Ok, string? Description, string? BotUsername);
