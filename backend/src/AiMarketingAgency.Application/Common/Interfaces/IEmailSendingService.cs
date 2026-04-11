using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IEmailSendingService
{
    Task<EmailSendResult> SendNewsletterAsync(
        EmailConnector config,
        List<NewsletterSubscriber> recipients,
        string subject,
        string htmlBody,
        CancellationToken ct);
}

public record EmailSendResult(bool Success, int SentCount, int FailedCount, string? Error);
