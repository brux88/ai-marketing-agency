using System.Net;
using System.Net.Mail;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Infrastructure.Email;

public class SmtpEmailService : IEmailSendingService
{
    public async Task<EmailSendResult> SendNewsletterAsync(
        EmailConnector config,
        List<NewsletterSubscriber> recipients,
        string subject,
        string htmlBody,
        CancellationToken ct)
    {
        var sentCount = 0;
        var failedCount = 0;

        try
        {
            using var client = new SmtpClient(config.SmtpHost, config.SmtpPort ?? 587)
            {
                Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword),
                EnableSsl = true
            };

            var from = new MailAddress(config.FromEmail, config.FromName);

            foreach (var subscriber in recipients.Where(s => s.IsActive))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    using var message = new MailMessage(from, new MailAddress(subscriber.Email, subscriber.Name))
                    {
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };
                    await client.SendMailAsync(message, ct);
                    sentCount++;
                }
                catch
                {
                    failedCount++;
                }
            }

            return new EmailSendResult(true, sentCount, failedCount, null);
        }
        catch (Exception ex)
        {
            return new EmailSendResult(false, sentCount, failedCount, ex.Message);
        }
    }
}
