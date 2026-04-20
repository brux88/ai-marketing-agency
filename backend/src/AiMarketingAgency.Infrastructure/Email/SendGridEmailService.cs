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

    private const string UnsubscribePlaceholder = "{{UNSUBSCRIBE_URL}}";

    public async Task<EmailSendResult> SendNewsletterAsync(
        EmailConnector config,
        List<NewsletterSubscriber> recipients,
        string subject,
        string htmlBodyTemplate,
        Func<NewsletterSubscriber, string> unsubscribeUrlFactory,
        CancellationToken ct)
    {
        var sentCount = 0;
        var failedCount = 0;

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.ApiKey);

            var activeRecipients = recipients.Where(s => s.IsActive).ToList();

            // One request per recipient so we can bake a unique unsubscribe
            // URL into each HTML body. Fine for our scale; swap for SendGrid
            // dynamic templates if the list ever gets large.
            foreach (var r in activeRecipients)
            {
                ct.ThrowIfCancellationRequested();

                var personalHtml = htmlBodyTemplate.Replace(UnsubscribePlaceholder, unsubscribeUrlFactory(r));

                var payload = new
                {
                    personalizations = new[]
                    {
                        new { to = new[] { new { email = r.Email, name = r.Name ?? r.Email } } }
                    },
                    from = new { email = config.FromEmail, name = config.FromName },
                    subject,
                    content = new[]
                    {
                        new { type = "text/html", value = personalHtml }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content, ct);

                if (response.IsSuccessStatusCode)
                    sentCount++;
                else
                    failedCount++;
            }

            return new EmailSendResult(true, sentCount, failedCount, null);
        }
        catch (Exception ex)
        {
            return new EmailSendResult(false, sentCount, failedCount, ex.Message);
        }
    }
}
