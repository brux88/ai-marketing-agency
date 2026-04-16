using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AiMarketingAgency.Infrastructure.Social;

public class SocialPublishingServiceFactory : ISocialPublishingServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SocialPublishingServiceFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public ISocialPublishingService Create(SocialPlatform platform)
    {
        var httpClient = _httpClientFactory.CreateClient();

        return platform switch
        {
            SocialPlatform.Twitter => new TwitterPublisher(httpClient),
            SocialPlatform.Instagram => new MetaPublisher(httpClient, SocialPlatform.Instagram, _configuration),
            SocialPlatform.Facebook => new MetaPublisher(httpClient, SocialPlatform.Facebook, _configuration),
            SocialPlatform.LinkedIn => new LinkedInPublisher(httpClient),
            _ => throw new NotSupportedException($"Platform {platform} is not supported.")
        };
    }
}
