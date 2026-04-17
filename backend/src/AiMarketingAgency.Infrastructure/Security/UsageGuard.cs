using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Infrastructure.Security;

public class UsageGuard : IUsageGuard
{
    private const int FreeTrialJobsPerMonth = 50;
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

        // No subscription, or subscription cancelled/expired → free trial
        var useFreeTrialLimits = subscription == null
            || subscription.Status == SubscriptionStatus.Cancelled
            || subscription.Status == SubscriptionStatus.Expired;

        if (useFreeTrialLimits)
        {
            var freeJobs = await _context.AgentJobs
                .IgnoreQueryFilters()
                .CountAsync(j => j.TenantId == tenantId && j.CreatedAt >= monthStart, ct);
            return freeJobs < FreeTrialJobsPerMonth;
        }

        // Active, Trialing, or PastDue → use subscription limits
        var currentPeriodStart = subscription!.CurrentPeriodEnd?.AddMonths(-1) ?? monthStart;
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

    public async Task<bool> CanCreateProjectAsync(Guid tenantId, CancellationToken ct = default)
    {
        var subscription = await _context.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        if (subscription == null) return true;

        var projectCount = await _context.Projects
            .IgnoreQueryFilters()
            .CountAsync(p => p.TenantId == tenantId && p.IsActive, ct);

        return projectCount < subscription.MaxProjects;
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
