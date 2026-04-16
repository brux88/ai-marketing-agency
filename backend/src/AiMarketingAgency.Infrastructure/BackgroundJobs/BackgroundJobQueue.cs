using System.Threading.Channels;
using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Infrastructure.BackgroundJobs;

public class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<(Guid JobId, Guid TenantId)> _channel =
        Channel.CreateUnbounded<(Guid, Guid)>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(Guid jobId, Guid tenantId)
    {
        _channel.Writer.TryWrite((jobId, tenantId));
    }

    public async Task<(Guid JobId, Guid TenantId)> DequeueAsync(CancellationToken ct)
    {
        return await _channel.Reader.ReadAsync(ct);
    }
}
