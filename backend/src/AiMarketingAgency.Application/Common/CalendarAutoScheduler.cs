using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Application.Common;

public static class CalendarAutoScheduler
{
    public static async Task<bool> TryScheduleAsync(
        IAppDbContext context,
        GeneratedContent content,
        ILogger logger,
        CancellationToken ct,
        ContentSchedule? triggeringSchedule = null)
    {
        var pubSchedules = await context.ContentSchedules
            .Where(s => s.AgencyId == content.AgencyId
                        && s.ScheduleType == ScheduleType.Publication
                        && s.IsActive
                        && (s.ProjectId == content.ProjectId || s.ProjectId == null))
            .OrderBy(s => s.ProjectId == null ? 1 : 0)
            .ToListAsync(ct);

        if (pubSchedules.Count == 0)
        {
            // Fallback: old AutoScheduleOnApproval behavior
            return await TryLegacyAutoScheduleAsync(context, content, triggeringSchedule, logger, ct);
        }

        var matchingSchedule = pubSchedules.FirstOrDefault(s =>
            s.PublishContentType == null || s.PublishContentType == (int)content.ContentType)
            ?? pubSchedules.FirstOrDefault(s => s.PublishContentType == null);

        if (matchingSchedule == null)
        {
            logger.LogInformation(
                "Auto-schedule skipped: no matching publication schedule for content {ContentId} type {Type}",
                content.Id, content.ContentType);
            return false;
        }

        var maxPerPlatform = matchingSchedule.MaxPostsPerPlatform ?? 1;
        bool created = false;

        if (content.ContentType is ContentType.SocialPost or ContentType.Carousel)
        {
            var platforms = ParsePlatforms(matchingSchedule.EnabledSocialPlatforms);
            foreach (var platform in platforms)
            {
                var alreadyScheduled = await context.CalendarEntries
                    .AnyAsync(e => e.ContentId == content.Id
                                   && e.Platform == platform
                                   && e.Status != CalendarEntryStatus.Failed, ct);
                if (alreadyScheduled) continue;

                var scheduledCountForPlatform = await context.CalendarEntries
                    .CountAsync(e => e.AgencyId == content.AgencyId
                                     && e.Platform == platform
                                     && e.Status == CalendarEntryStatus.Scheduled, ct);

                var scheduledAt = scheduledCountForPlatform < maxPerPlatform
                    ? ComputeNextSlot(matchingSchedule, 0)
                    : ComputeNextSlot(matchingSchedule, scheduledCountForPlatform / maxPerPlatform);

                context.CalendarEntries.Add(new EditorialCalendarEntry
                {
                    AgencyId = content.AgencyId,
                    TenantId = content.TenantId,
                    ContentId = content.Id,
                    Platform = platform,
                    ScheduledAt = scheduledAt,
                    Status = CalendarEntryStatus.Scheduled
                });
                created = true;
            }
        }
        else if (content.ContentType == ContentType.Newsletter)
        {
            var alreadyScheduled = await context.CalendarEntries
                .AnyAsync(e => e.ContentId == content.Id
                               && e.Platform == null
                               && e.Status != CalendarEntryStatus.Failed, ct);
            if (!alreadyScheduled)
            {
                var scheduledAt = ComputeNextSlot(matchingSchedule, 0);
                context.CalendarEntries.Add(new EditorialCalendarEntry
                {
                    AgencyId = content.AgencyId,
                    TenantId = content.TenantId,
                    ContentId = content.Id,
                    Platform = null,
                    ScheduledAt = scheduledAt,
                    Status = CalendarEntryStatus.Scheduled
                });
                created = true;
            }
        }

        if (created)
        {
            await context.SaveChangesAsync(ct);
            logger.LogInformation(
                "Auto-scheduled content {ContentId} via publication schedule '{ScheduleName}'",
                content.Id, matchingSchedule.Name);
        }

        return created;
    }

    private static async Task<bool> TryLegacyAutoScheduleAsync(
        IAppDbContext context,
        GeneratedContent content,
        ContentSchedule? schedule,
        ILogger logger,
        CancellationToken ct)
    {
        if (content.ContentType != ContentType.SocialPost) return false;

        var agency = await context.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == content.AgencyId, ct);
        if (agency is null) return false;

        bool autoScheduleEnabled = agency.AutoScheduleOnApproval;
        if (content.ProjectId.HasValue)
        {
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == content.ProjectId.Value, ct);
            if (project?.AutoScheduleOnApproval.HasValue == true)
                autoScheduleEnabled = project.AutoScheduleOnApproval.Value;
        }
        if (schedule?.AutoScheduleOnApproval.HasValue == true)
            autoScheduleEnabled = schedule.AutoScheduleOnApproval.Value;

        if (!autoScheduleEnabled) return false;

        var platform = ContentPlatformResolver.DetectFromTitle(content.Title);
        SocialConnector? connector;
        if (platform.HasValue)
        {
            connector = await context.SocialConnectors
                .Where(c => c.AgencyId == content.AgencyId && c.IsActive && c.Platform == platform.Value
                            && (c.ProjectId == content.ProjectId || c.ProjectId == null))
                .FirstOrDefaultAsync(ct);
        }
        else
        {
            connector = await context.SocialConnectors
                .Where(c => c.AgencyId == content.AgencyId && c.IsActive
                            && (c.ProjectId == content.ProjectId || c.ProjectId == null))
                .FirstOrDefaultAsync(ct);
            platform = connector?.Platform;
        }

        if (connector is null || platform is null) return false;

        var alreadyScheduled = await context.CalendarEntries
            .AnyAsync(e => e.ContentId == content.Id && e.Platform == platform
                           && e.Status != CalendarEntryStatus.Failed, ct);
        if (alreadyScheduled) return false;

        context.CalendarEntries.Add(new EditorialCalendarEntry
        {
            AgencyId = content.AgencyId,
            TenantId = content.TenantId,
            ContentId = content.Id,
            Platform = platform.Value,
            ScheduledAt = DateTime.UtcNow,
            Status = CalendarEntryStatus.Scheduled
        });
        await context.SaveChangesAsync(ct);
        return true;
    }

    private static DateTime ComputeNextSlot(ContentSchedule schedule, int slotOffset)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
            int found = 0;

            for (int i = 0; i <= 60; i++)
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
                if (i == 0 && candidateLocal <= nowLocal) continue;

                if (found >= slotOffset)
                {
                    return TimeZoneInfo.ConvertTimeToUtc(
                        DateTime.SpecifyKind(candidateLocal, DateTimeKind.Unspecified), tz);
                }
                found++;
            }
        }
        catch { }

        return DateTime.UtcNow.AddDays(slotOffset + 1);
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
