using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AiMarketingAgency.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
        }
        await base.OnConnectedAsync();
    }

    public async Task JoinAgencyGroup(string agencyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"agency_{agencyId}");
    }

    public async Task LeaveAgencyGroup(string agencyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agency_{agencyId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
