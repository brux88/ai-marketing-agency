using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IEmailSendingService
{
    /// Sends the newsletter to each recipient. `htmlBodyTemplate` may contain
    /// the sentinel `{{UNSUBSCRIBE_URL}}` which is replaced per recipient with
    /// the output of `unsubscribeUrlFactory(subscriber)`.
    Task<EmailSendResult> SendNewsletterAsync(
        EmailConnector config,
        List<NewsletterSubscriber> recipients,
        string subject,
        string htmlBodyTemplate,
        Func<NewsletterSubscriber, string> unsubscribeUrlFactory,
        CancellationToken ct);
}

public record EmailSendResult(bool Success, int SentCount, int FailedCount, string? Error);
