using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Infrastructure.Ai.VideoGeneration;

public class VideoGenerationServiceFactory : IVideoGenerationServiceFactory
{
    public IVideoGenerationService Create(LlmProviderType providerType, string apiKey, string? apiKeySecret = null, string? baseUrl = null)
    {
        return providerType switch
        {
            LlmProviderType.HiggField => new HiggFieldVideoService(apiKey, apiKeySecret, baseUrl),
            _ => throw new NotSupportedException($"Video generation not supported for provider {providerType}")
        };
    }
}
