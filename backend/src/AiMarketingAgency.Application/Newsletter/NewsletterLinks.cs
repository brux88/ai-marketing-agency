using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Application.Newsletter;

public static class NewsletterLinks
{
    public const string DefaultFrontendUrl = "https://wepostai.com";

    public static string UnsubscribeUrl(string frontendBaseUrl, NewsletterSubscriber subscriber)
    {
        var baseUrl = (frontendBaseUrl ?? DefaultFrontendUrl).TrimEnd('/');
        return $"{baseUrl}/unsubscribe?token={subscriber.UnsubscribeToken}";
    }
}
