using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ISocialPublishingServiceFactory
{
    ISocialPublishingService Create(SocialPlatform platform);
}
