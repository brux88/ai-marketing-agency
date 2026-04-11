using AiMarketingAgency.Domain.Entities;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ISocialPublishingService
{
    Task<PublishResult> PublishAsync(SocialConnector connector, GeneratedContent content, CancellationToken ct);
}

public record PublishResult(bool Success, string? PostId, string? PostUrl, string? Error);
