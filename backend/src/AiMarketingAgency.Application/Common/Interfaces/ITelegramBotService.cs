namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ITelegramBotService
{
    Task SendMessageAsync(long chatId, string message, CancellationToken ct = default);
    Task SendContentForApprovalAsync(long chatId, Guid contentId, string title, string preview, decimal score, CancellationToken ct = default);
    Task SendPublishNotificationAsync(long chatId, string title, string platform, string? postUrl, CancellationToken ct = default);
    Task NotifyAgencyAsync(Guid agencyId, string message, CancellationToken ct = default);
}
