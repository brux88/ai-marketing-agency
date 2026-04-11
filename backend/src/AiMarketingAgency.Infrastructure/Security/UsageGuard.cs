using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Infrastructure.Security;

public class UsageGuard : IUsageGuard
{
    private readonly IAppDbContext _context;

    public UsageGuard(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanRunJobAsync(Guid tenantId, CancellationToken ct = default)
    {
        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        // Calendar month start as universal fallback
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        if (subscription == null)
        {
            // No subscription — treat as FreeTrial: 20 jobs per calendar month
            var freeJobs = await _context.AgentJobs
                .IgnoreQueryFilters()
                .CountAsync(j => j.TenantId == tenantId && j.CreatedAt >= monthStart, ct);
            return freeJobs < 20;
        }

        var currentPeriodStart = subscription.CurrentPeriodEnd?.AddMonths(-1) ?? monthStart;
        var jobsThisPeriod = await _context.AgentJobs
            .IgnoreQueryFilters()
            .CountAsync(j => j.TenantId == tenantId && j.CreatedAt >= currentPeriodStart, ct);

        return jobsThisPeriod < subscription.MaxJobsPerMonth;
    }

    public async Task<bool> CanCreateAgencyAsync(Guid tenantId, CancellationToken ct = default)
    {
        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        if (subscription == null) return true; // Allow during initial setup

        var agencyCount = await _context.Agencies
            .IgnoreQueryFilters()
            .CountAsync(a => a.TenantId == tenantId && a.IsActive, ct);

        return agencyCount < subscription.MaxAgencies;
    }

    public async Task IncrementJobCountAsync(Guid tenantId, Guid agencyId, CancellationToken ct = default)
    {
        var period = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var record = await _context.UsageRecords
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.AgencyId == agencyId && u.Period == period, ct);

        if (record != null)
        {
            record.JobsCount++;
        }
        else
        {
            _context.UsageRecords.Add(new UsageRecord
            {
                TenantId = tenantId,
                AgencyId = agencyId,
                Period = period,
                JobsCount = 1
            });
        }

        await _context.SaveChangesAsync(ct);
    }
}
