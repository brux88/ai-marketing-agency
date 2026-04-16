namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IBackgroundJobQueue
{
    void Enqueue(Guid jobId, Guid tenantId);
    Task<(Guid JobId, Guid TenantId)> DequeueAsync(CancellationToken ct);
}
