namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IUsageGuard
{
    Task<bool> CanRunJobAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> CanCreateAgencyAsync(Guid tenantId, CancellationToken ct = default);
    Task IncrementJobCountAsync(Guid tenantId, Guid agencyId, CancellationToken ct = default);
}
