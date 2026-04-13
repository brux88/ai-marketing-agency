using AiMarketingAgency.Api.Hubs;
using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AiMarketingAgency.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyJobStatusChanged(Guid tenantId, Guid agencyId, Guid jobId, string status, string? agentType = null)
    {
        await _hubContext.Clients.Group($"agency_{agencyId}").SendAsync("JobStatusChanged", new
        {
            jobId, agencyId, status, agentType, timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyContentGenerated(Guid tenantId, Guid agencyId, Guid contentId, string title, string status)
    {
        await _hubContext.Clients.Group($"agency_{agencyId}").SendAsync("ContentGenerated", new
        {
            contentId, agencyId, title, status, timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyContentApproved(Guid tenantId, Guid agencyId, Guid contentId, string title)
    {
        await _hubContext.Clients.Group($"agency_{agencyId}").SendAsync("ContentApproved", new
        {
            contentId, agencyId, title, timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyContentRejected(Guid tenantId, Guid agencyId, Guid contentId, string title)
    {
        await _hubContext.Clients.Group($"agency_{agencyId}").SendAsync("ContentRejected", new
        {
            contentId, agencyId, title, timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyPublishResult(Guid tenantId, Guid agencyId, Guid contentId, string platform, bool success, string? postUrl = null)
    {
        await _hubContext.Clients.Group($"agency_{agencyId}").SendAsync("PublishResult", new
        {
            contentId, agencyId, platform, success, postUrl, timestamp = DateTime.UtcNow
        });
    }
}
