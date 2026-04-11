using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Infrastructure.Ai.ImageGeneration;

public class ImageGenerationServiceFactory : IImageGenerationServiceFactory
{
    public IImageGenerationService Create(LlmProviderType providerType, string apiKey, string? apiKeySecret = null, string? baseUrl = null)
    {
        return providerType switch
        {
            LlmProviderType.OpenAI => new DallEImageService(apiKey),
            LlmProviderType.NanoBanana => new NanoBananaImageService(apiKey, baseUrl),
            LlmProviderType.HiggField => new HiggFieldImageService(apiKey, apiKeySecret, baseUrl),
            LlmProviderType.Custom => new CustomImageService(apiKey, baseUrl ?? throw new ArgumentException("Base URL required for custom provider")),
            _ => throw new NotSupportedException($"Image generation not supported for provider {providerType}")
        };
    }
}
