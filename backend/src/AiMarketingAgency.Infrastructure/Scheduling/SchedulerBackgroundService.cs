using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Schedules.Commands.CreateSchedule;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Scheduling;

public class SchedulerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SchedulerBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public SchedulerBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SchedulerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Content Scheduler started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSchedules(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled jobs");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessDueSchedules(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        // Find all active schedules that are due (NextRunAt <= now)
        var dueSchedules = await context.Set<ContentSchedule>()
            .IgnoreQueryFilters() // Background service has no tenant context
            .Where(s => s.IsActive && s.NextRunAt != null && s.NextRunAt <= now)
            .Include(s => s.Agency)
            .ToListAsync(ct);

        if (dueSchedules.Count == 0) return;

        _logger.LogInformation("Found {Count} due schedules to process", dueSchedules.Count);

        foreach (var schedule in dueSchedules)
        {
            try
            {
                await ProcessSchedule(scope.ServiceProvider, schedule, ct);

                // Update last run and calculate next run
                schedule.LastRunAt = now;
                schedule.NextRunAt = CreateScheduleCommandHandler.CalculateNextRun(
                    schedule.Days, schedule.TimeOfDay, schedule.TimeZone);

                await context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Schedule '{Name}' (Agency: {AgencyId}, Agent: {AgentType}) executed. Next run: {NextRun}",
                    schedule.Name, schedule.AgencyId, schedule.AgentType, schedule.NextRunAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process schedule '{Name}' (Id: {Id}) for agency {AgencyId}",
                    schedule.Name, schedule.Id, schedule.AgencyId);

                // Still update NextRunAt so we don't retry the same slot forever
                schedule.NextRunAt = CreateScheduleCommandHandler.CalculateNextRun(
                    schedule.Days, schedule.TimeOfDay, schedule.TimeZone);
                await context.SaveChangesAsync(ct);
            }
        }
    }

    private async Task ProcessSchedule(IServiceProvider serviceProvider, ContentSchedule schedule, CancellationToken ct)
    {
        // Set up tenant context for the agency's tenant
        var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(schedule.TenantId, Guid.Empty);

        var context = serviceProvider.GetRequiredService<IAppDbContext>();

        // Check that agency has LLM configured
        if (schedule.Agency.DefaultLlmProviderKeyId == null)
        {
            _logger.LogWarning(
                "Skipping schedule '{Name}' — agency {AgencyId} has no LLM key configured",
                schedule.Name, schedule.AgencyId);
            return;
        }

        // Create agent job
        var job = new AgentJob
        {
            TenantId = schedule.TenantId,
            AgencyId = schedule.AgencyId,
            AgentType = schedule.AgentType,
            Status = JobStatus.Queued,
            Input = schedule.Input,
            ProjectId = schedule.ProjectId
        };

        context.AgentJobs.Add(job);
        await context.SaveChangesAsync(ct);

        // Process the job
        var jobProcessor = serviceProvider.GetRequiredService<IAgentJobProcessor>();
        await jobProcessor.ProcessJobAsync(job.Id, ct);
    }
}
