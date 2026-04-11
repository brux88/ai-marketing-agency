using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Infrastructure.Email;

public class SendGridEmailService : IEmailSendingService
{
    private readonly HttpClient _httpClient;

    public SendGridEmailService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.ApiKey);

            var activeRecipients = recipients.Where(s => s.IsActive).ToList();

            // SendGrid supports up to 1000 personalizations per request
            foreach (var batch in activeRecipients.Chunk(1000))
            {
                ct.ThrowIfCancellationRequested();

                var payload = new
                {
                    personalizations = batch.Select(r => new
                    {
                        to = new[] { new { email = r.Email, name = r.Name ?? r.Email } }
                    }).ToArray(),
                    from = new { email = config.FromEmail, name = config.FromName },
                    subject,
                    content = new[]
                    {
                        new { type = "text/html", value = htmlBody }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content, ct);

                if (response.IsSuccessStatusCode)
                    sentCount += batch.Length;
                else
                    failedCount += batch.Length;
            }

            return new EmailSendResult(true, sentCount, failedCount, null);
        }
        catch (Exception ex)
        {
            return new EmailSendResult(false, sentCount, failedCount, ex.Message);
        }
    }
}
