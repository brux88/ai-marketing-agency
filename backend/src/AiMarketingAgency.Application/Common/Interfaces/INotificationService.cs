namespace AiMarketingAgency.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyJobStatusChanged(Guid tenantId, Guid agencyId, Guid jobId, string status, string? agentType = null);
    Task NotifyContentGenerated(Guid tenantId, Guid agencyId, Guid contentId, string title, string status);
    Task NotifyContentApproved(Guid tenantId, Guid agencyId, Guid contentId, string title);
    Task NotifyContentRejected(Guid tenantId, Guid agencyId, Guid contentId, string title);
    Task NotifyPublishResult(Guid tenantId, Guid agencyId, Guid contentId, string platform, bool success, string? postUrl = null);
}
