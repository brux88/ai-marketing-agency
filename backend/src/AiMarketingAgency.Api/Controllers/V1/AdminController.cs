using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public AdminController(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    private async Task<bool> IsSuperAdmin(CancellationToken ct)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == _tenantContext.UserId, ct);
        return user?.Role == UserRole.SuperAdmin;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetStats(CancellationToken ct)
    {
        if (!await IsSuperAdmin(ct))
            return Forbid();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var superAdminTenantIds = await _context.Users.IgnoreQueryFilters()
            .Where(u => u.Role == UserRole.SuperAdmin)
            .Select(u => u.TenantId)
            .ToListAsync(ct);

        var totalTenants = await _context.Tenants.IgnoreQueryFilters()
            .CountAsync(t => !superAdminTenantIds.Contains(t.Id), ct);
        var activeTenants = await _context.Tenants.IgnoreQueryFilters()
            .CountAsync(t => t.IsActive && !superAdminTenantIds.Contains(t.Id), ct);
        var totalUsers = await _context.Users.IgnoreQueryFilters()
            .CountAsync(u => u.Role != UserRole.SuperAdmin, ct);
        var totalAgencies = await _context.Agencies.IgnoreQueryFilters()
            .CountAsync(a => a.IsActive && !superAdminTenantIds.Contains(a.TenantId), ct);
        var totalProjects = await _context.Projects.IgnoreQueryFilters()
            .CountAsync(p => p.IsActive && !superAdminTenantIds.Contains(p.TenantId), ct);

        var jobsThisMonth = await _context.AgentJobs.IgnoreQueryFilters()
            .CountAsync(j => j.CreatedAt >= monthStart && !superAdminTenantIds.Contains(j.TenantId), ct);
        var jobsTotal = await _context.AgentJobs.IgnoreQueryFilters()
            .CountAsync(j => !superAdminTenantIds.Contains(j.TenantId), ct);

        var contentsTotal = await _context.GeneratedContents.IgnoreQueryFilters()
            .CountAsync(c => !superAdminTenantIds.Contains(c.TenantId), ct);
        var contentsThisMonth = await _context.GeneratedContents.IgnoreQueryFilters()
            .CountAsync(c => c.CreatedAt >= monthStart && !superAdminTenantIds.Contains(c.TenantId), ct);

        var subscriptions = await _context.Subscriptions.IgnoreQueryFilters()
            .Where(s => !superAdminTenantIds.Contains(s.TenantId))
            .ToListAsync(ct);
        var activeSubs = subscriptions.Count(s => s.Status == Domain.Enums.SubscriptionStatus.Active);
        var trialSubs = subscriptions.Count(s => s.Status == Domain.Enums.SubscriptionStatus.Trialing);

        var planBreakdown = subscriptions
            .GroupBy(s => s.PlanTier.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var totalImageCost = await _context.GeneratedContents.IgnoreQueryFilters()
            .SumAsync(c => c.AiImageCostUsd ?? 0, ct);
        var totalTextCost = await _context.GeneratedContents.IgnoreQueryFilters()
            .SumAsync(c => c.AiGenerationCostUsd ?? 0, ct);

        return Ok(ApiResponse<AdminStatsDto>.Ok(new AdminStatsDto
        {
            TotalTenants = totalTenants,
            ActiveTenants = activeTenants,
            TotalUsers = totalUsers,
            TotalAgencies = totalAgencies,
            TotalProjects = totalProjects,
            JobsThisMonth = jobsThisMonth,
            JobsTotal = jobsTotal,
            ContentsTotal = contentsTotal,
            ContentsThisMonth = contentsThisMonth,
            ActiveSubscriptions = activeSubs,
            TrialSubscriptions = trialSubs,
            PlanBreakdown = planBreakdown,
            TotalImageCostUsd = totalImageCost,
            TotalTextCostUsd = totalTextCost,
            TotalCostUsd = totalImageCost + totalTextCost
        }));
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<ApiResponse<List<TenantDetailDto>>>> GetTenants(CancellationToken ct)
    {
        if (!await IsSuperAdmin(ct))
            return Forbid();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var superAdminTenantIds = await _context.Users.IgnoreQueryFilters()
            .Where(u => u.Role == UserRole.SuperAdmin)
            .Select(u => u.TenantId)
            .ToListAsync(ct);

        var tenants = await _context.Tenants.IgnoreQueryFilters()
            .Where(t => !superAdminTenantIds.Contains(t.Id))
            .Include(t => t.Users)
            .Include(t => t.Agencies)
            .Include(t => t.Subscription)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var result = new List<TenantDetailDto>();
        foreach (var t in tenants)
        {
            var jobCount = await _context.AgentJobs.IgnoreQueryFilters()
                .CountAsync(j => j.TenantId == t.Id && j.CreatedAt >= monthStart, ct);
            var contentCount = await _context.GeneratedContents.IgnoreQueryFilters()
                .CountAsync(c => c.TenantId == t.Id, ct);
            var totalCost = await _context.GeneratedContents.IgnoreQueryFilters()
                .Where(c => c.TenantId == t.Id)
                .SumAsync(c => (c.AiGenerationCostUsd ?? 0) + (c.AiImageCostUsd ?? 0), ct);

            result.Add(new TenantDetailDto
            {
                Id = t.Id,
                Name = t.Name,
                Plan = t.Subscription?.PlanTier.ToString() ?? "FreeTrial",
                Status = t.Subscription?.Status.ToString() ?? "Trial",
                IsActive = t.IsActive,
                UsersCount = t.Users.Count,
                AgenciesCount = t.Agencies.Count(a => a.IsActive),
                JobsThisMonth = jobCount,
                TotalContents = contentCount,
                TotalCostUsd = totalCost,
                CreatedAt = t.CreatedAt
            });
        }

        return Ok(ApiResponse<List<TenantDetailDto>>.Ok(result));
    }
}

public class AdminStatsDto
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAgencies { get; set; }
    public int TotalProjects { get; set; }
    public int JobsThisMonth { get; set; }
    public int JobsTotal { get; set; }
    public int ContentsTotal { get; set; }
    public int ContentsThisMonth { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public Dictionary<string, int> PlanBreakdown { get; set; } = new();
    public decimal TotalImageCostUsd { get; set; }
    public decimal TotalTextCostUsd { get; set; }
    public decimal TotalCostUsd { get; set; }
}

public class TenantDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Plan { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsActive { get; set; }
    public int UsersCount { get; set; }
    public int AgenciesCount { get; set; }
    public int JobsThisMonth { get; set; }
    public int TotalContents { get; set; }
    public decimal TotalCostUsd { get; set; }
    public DateTime CreatedAt { get; set; }
}
