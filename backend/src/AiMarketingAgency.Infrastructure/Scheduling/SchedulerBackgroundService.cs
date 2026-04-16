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
        var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(schedule.TenantId, Guid.Empty);

        if (schedule.ScheduleType == ScheduleType.Publication)
        {
            await ProcessPublicationSchedule(serviceProvider, schedule, ct);
            return;
        }

        await ProcessGenerationSchedule(serviceProvider, schedule, ct);
    }

    private async Task ProcessGenerationSchedule(IServiceProvider serviceProvider, ContentSchedule schedule, CancellationToken ct)
    {
        var context = serviceProvider.GetRequiredService<IAppDbContext>();

        if (schedule.Agency.DefaultLlmProviderKeyId == null)
        {
            _logger.LogWarning(
                "Skipping schedule '{Name}' — agency {AgencyId} has no LLM key configured",
                schedule.Name, schedule.AgencyId);
            return;
        }

        var job = new AgentJob
        {
            TenantId = schedule.TenantId,
            AgencyId = schedule.AgencyId,
            AgentType = schedule.AgentType,
            Status = JobStatus.Queued,
            Input = schedule.Input,
            ProjectId = schedule.ProjectId,
            ScheduleId = schedule.Id
        };

        context.AgentJobs.Add(job);
        await context.SaveChangesAsync(ct);

        var jobProcessor = serviceProvider.GetRequiredService<IAgentJobProcessor>();
        await jobProcessor.ProcessJobAsync(job.Id, ct);
    }

    private async Task ProcessPublicationSchedule(IServiceProvider serviceProvider, ContentSchedule schedule, CancellationToken ct)
    {
        var context = serviceProvider.GetRequiredService<IAppDbContext>();
        var maxPerPlatform = schedule.MaxPostsPerPlatform ?? 1;

        var query = context.GeneratedContents
            .IgnoreQueryFilters()
            .Where(c => c.AgencyId == schedule.AgencyId
                        && c.Status == ContentStatus.Approved);

        if (schedule.ProjectId.HasValue)
            query = query.Where(c => c.ProjectId == schedule.ProjectId);

        if (schedule.PublishContentType.HasValue)
            query = query.Where(c => (int)c.ContentType == schedule.PublishContentType.Value);

        var contents = await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        var platforms = ParsePlatforms(
            !string.IsNullOrWhiteSpace(schedule.EnabledSocialPlatforms) ? schedule.EnabledSocialPlatforms : null);

        var socialContents = contents
            .Where(c => c.ContentType is ContentType.SocialPost or ContentType.Carousel)
            .ToList();
        var newsletterContents = contents
            .Where(c => c.ContentType == ContentType.Newsletter)
            .ToList();

        var futureSlots = ComputeFutureSlots(schedule, 20);
        int entriesCreated = 0;

        foreach (var platform in platforms)
        {
            var unscheduled = new List<GeneratedContent>();
            foreach (var c in socialContents)
            {
                var alreadyScheduled = await context.CalendarEntries
                    .IgnoreQueryFilters()
                    .AnyAsync(e => e.ContentId == c.Id
                                   && e.Platform == platform
                                   && e.Status != CalendarEntryStatus.Failed, ct);
                if (!alreadyScheduled)
                    unscheduled.Add(c);
            }

            if (unscheduled.Count == 0) continue;

            var nowBatch = unscheduled.Take(maxPerPlatform).ToList();
            var remainder = unscheduled.Skip(maxPerPlatform).ToList();

            foreach (var content in nowBatch)
            {
                context.CalendarEntries.Add(new EditorialCalendarEntry
                {
                    TenantId = schedule.TenantId,
                    AgencyId = schedule.AgencyId,
                    ContentId = content.Id,
                    Platform = platform,
                    ScheduledAt = DateTime.UtcNow,
                    Status = CalendarEntryStatus.Scheduled
                });
                entriesCreated++;
            }

            var slotIndex = 0;
            foreach (var content in remainder)
            {
                if (slotIndex >= futureSlots.Count) break;
                context.CalendarEntries.Add(new EditorialCalendarEntry
                {
                    TenantId = schedule.TenantId,
                    AgencyId = schedule.AgencyId,
                    ContentId = content.Id,
                    Platform = platform,
                    ScheduledAt = futureSlots[slotIndex],
                    Status = CalendarEntryStatus.Scheduled
                });
                entriesCreated++;
                slotIndex++;
            }
        }

        var unscheduledNewsletters = new List<GeneratedContent>();
        foreach (var c in newsletterContents)
        {
            var alreadyScheduled = await context.CalendarEntries
                .IgnoreQueryFilters()
                .AnyAsync(e => e.ContentId == c.Id
                               && e.Platform == null
                               && e.Status != CalendarEntryStatus.Failed, ct);
            if (!alreadyScheduled)
                unscheduledNewsletters.Add(c);
        }

        var nlNow = unscheduledNewsletters.Take(maxPerPlatform).ToList();
        var nlRemainder = unscheduledNewsletters.Skip(maxPerPlatform).ToList();

        foreach (var nl in nlNow)
        {
            context.CalendarEntries.Add(new EditorialCalendarEntry
            {
                TenantId = schedule.TenantId,
                AgencyId = schedule.AgencyId,
                ContentId = nl.Id,
                Platform = null,
                ScheduledAt = DateTime.UtcNow,
                Status = CalendarEntryStatus.Scheduled
            });
            entriesCreated++;
        }

        var nlSlot = 0;
        foreach (var nl in nlRemainder)
        {
            if (nlSlot >= futureSlots.Count) break;
            context.CalendarEntries.Add(new EditorialCalendarEntry
            {
                TenantId = schedule.TenantId,
                AgencyId = schedule.AgencyId,
                ContentId = nl.Id,
                Platform = null,
                ScheduledAt = futureSlots[nlSlot],
                Status = CalendarEntryStatus.Scheduled
            });
            entriesCreated++;
            nlSlot++;
        }

        if (entriesCreated > 0)
            await context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Publication schedule '{Name}': created {Count} calendar entries",
            schedule.Name, entriesCreated);
    }

    private static List<DateTime> ComputeFutureSlots(ContentSchedule schedule, int maxSlots)
    {
        var slots = new List<DateTime>();
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            for (int i = 1; i <= 60 && slots.Count < maxSlots; i++)
            {
                var candidate = nowLocal.Date.AddDays(i);
                var dayFlag = candidate.DayOfWeek switch
                {
                    System.DayOfWeek.Monday => DayOfWeekFlag.Monday,
                    System.DayOfWeek.Tuesday => DayOfWeekFlag.Tuesday,
                    System.DayOfWeek.Wednesday => DayOfWeekFlag.Wednesday,
                    System.DayOfWeek.Thursday => DayOfWeekFlag.Thursday,
                    System.DayOfWeek.Friday => DayOfWeekFlag.Friday,
                    System.DayOfWeek.Saturday => DayOfWeekFlag.Saturday,
                    System.DayOfWeek.Sunday => DayOfWeekFlag.Sunday,
                    _ => DayOfWeekFlag.None
                };
                if (!schedule.Days.HasFlag(dayFlag)) continue;

                var candidateLocal = candidate.Add(schedule.TimeOfDay.ToTimeSpan());
                var candidateUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(candidateLocal, DateTimeKind.Unspecified), tz);
                slots.Add(candidateUtc);
            }
        }
        catch (Exception)
        {
            // Fallback: daily slots
            for (int i = 1; i <= maxSlots; i++)
                slots.Add(DateTime.UtcNow.AddDays(i));
        }
        return slots;
    }

    private static List<SocialPlatform> ParsePlatforms(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return [SocialPlatform.Twitter, SocialPlatform.LinkedIn, SocialPlatform.Instagram, SocialPlatform.Facebook];

        var result = new List<SocialPlatform>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<SocialPlatform>(part, true, out var p))
                result.Add(p);
        }
        return result.Count > 0 ? result : [SocialPlatform.Twitter, SocialPlatform.LinkedIn, SocialPlatform.Instagram, SocialPlatform.Facebook];
    }
}
