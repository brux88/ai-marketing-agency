namespace AiMarketingAgency.Application.Common.Interfaces;

public enum PushEventType
{
    ContentGenerated,
    ContentPublished,
    ApprovalNeeded,
    NewSubscriber,
}

public interface IPushNotificationService
{
    Task SendToProjectAsync(
        Guid agencyId,
        Guid? projectId,
        PushEventType eventType,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default);

    Task RegisterTokenAsync(Guid userId, string fcmToken, string platform, string? deviceName, CancellationToken ct = default);
    Task UnregisterTokenAsync(Guid userId, string fcmToken, CancellationToken ct = default);
}
