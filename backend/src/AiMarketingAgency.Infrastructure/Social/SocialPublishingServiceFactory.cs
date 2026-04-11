using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Infrastructure.Social;

public class SocialPublishingServiceFactory : ISocialPublishingServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SocialPublishingServiceFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ISocialPublishingService Create(SocialPlatform platform)
    {
        var httpClient = _httpClientFactory.CreateClient();

        return platform switch
        {
            SocialPlatform.Twitter => new TwitterPublisher(httpClient),
            SocialPlatform.Instagram => new MetaPublisher(httpClient, SocialPlatform.Instagram),
            SocialPlatform.Facebook => new MetaPublisher(httpClient, SocialPlatform.Facebook),
            SocialPlatform.LinkedIn => new LinkedInPublisher(httpClient),
            _ => throw new NotSupportedException($"Platform {platform} is not supported.")
        };
    }
}
