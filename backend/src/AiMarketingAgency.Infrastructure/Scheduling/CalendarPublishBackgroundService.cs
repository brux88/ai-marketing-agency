using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Newsletter.Commands.SendNewsletter;
using AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using AiMarketingAgency.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Scheduling;

public class CalendarPublishBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CalendarPublishBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    public CalendarPublishBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CalendarPublishBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Calendar auto-publish worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueEntries(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing calendar entries");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessDueEntries(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        var due = await context.Set<EditorialCalendarEntry>()
            .IgnoreQueryFilters()
            .Include(e => e.Content)
            .Where(e => e.Status == CalendarEntryStatus.Scheduled && e.ScheduledAt <= now)
            .Take(20)
            .ToListAsync(ct);

        if (due.Count == 0) return;

        _logger.LogInformation("Auto-publishing {Count} scheduled calendar entries", due.Count);

        foreach (var entry in due)
        {
            try
            {
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                tenantContext.SetTenant(entry.TenantId, Guid.Empty);

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var contentTitle = entry.Content?.Title ?? "Contenuto";
                var projectId = entry.Content?.ProjectId;

                if (entry.Platform != null)
                {
                    var result = await mediator.Send(
                        new PublishContentCommand(entry.AgencyId, entry.ContentId, entry.Platform.Value),
                        ct);

                    entry.Status = result.Success ? CalendarEntryStatus.Published : CalendarEntryStatus.Failed;
                    entry.PublishedAt = result.Success ? DateTime.UtcNow : null;
                    entry.ErrorMessage = result.Success ? null : result.Error;
                    entry.PostUrl = result.Success ? result.PostUrl : entry.PostUrl;

                    context.Set<Notification>().Add(new Notification
                    {
                        TenantId = entry.TenantId,
                        AgencyId = entry.AgencyId,
                        ProjectId = projectId,
                        Type = result.Success ? "publish.success" : "publish.failed",
                        Title = result.Success
                            ? $"Pubblicato su {entry.Platform.Value}: {contentTitle}"
                            : $"Pubblicazione fallita su {entry.Platform.Value}",
                        Body = result.Success ? result.PostUrl : result.Error,
                        Read = false
                    });

                    try
                    {
                        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notifier.NotifyPublishResult(
                            entry.TenantId, entry.AgencyId, entry.ContentId,
                            entry.Platform.Value.ToString(), result.Success, result.PostUrl);

                        var telegram = scope.ServiceProvider.GetRequiredService<ITelegramBotService>();
                        var msg = result.Success
                            ? $"\ud83d\udce2 <b>Pubblicato su {entry.Platform.Value}</b>\n{contentTitle}\n{result.PostUrl}"
                            : $"\u274c <b>Pubblicazione fallita su {entry.Platform.Value}</b>\n{contentTitle}\n{result.Error}";
                        await telegram.NotifyAgencyAsync(entry.AgencyId, projectId, msg, ct);

                        // Email notification on publication
                        if (result.Success && projectId.HasValue)
                        {
                            var emailNotifier = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
                            var project = await context.Set<Project>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == projectId.Value, ct);

                            if (project?.NotifyEmailOnPublication == true && !string.IsNullOrWhiteSpace(project.NotificationEmail))
                            {
                                var subject = $"Contenuto pubblicato su {entry.Platform.Value} - {contentTitle}";
                                var htmlBody = $"""
                                    <h2>Contenuto pubblicato</h2>
                                    <p>Il contenuto <strong>{contentTitle}</strong> e stato pubblicato su <strong>{entry.Platform.Value}</strong>.</p>
                                    {(string.IsNullOrEmpty(result.PostUrl) ? "" : $"<p><a href=\"{result.PostUrl}\">Vedi il post</a></p>")}
                                    """;
                                await emailNotifier.SendEmailNotificationAsync(
                                    entry.AgencyId, projectId, subject, htmlBody, ct);
                            }
                        }
                    }
                    catch (Exception notifyEx)
                    {
                        _logger.LogWarning(notifyEx, "Failed to send publish notification for entry {EntryId}", entry.Id);
                    }
                }
                else if (entry.Content?.ContentType == ContentType.Newsletter)
                {
                    try
                    {
                        await mediator.Send(new SendNewsletterCommand(entry.AgencyId, entry.ContentId), ct);
                        entry.Status = CalendarEntryStatus.Published;
                        entry.PublishedAt = DateTime.UtcNow;

                        context.Set<Notification>().Add(new Notification
                        {
                            TenantId = entry.TenantId,
                            AgencyId = entry.AgencyId,
                            ProjectId = projectId,
                            Type = "publish.success",
                            Title = $"Newsletter inviata: {contentTitle}",
                            Read = false
                        });
                    }
                    catch (Exception nlEx)
                    {
                        entry.Status = CalendarEntryStatus.Failed;
                        entry.ErrorMessage = nlEx.Message;

                        context.Set<Notification>().Add(new Notification
                        {
                            TenantId = entry.TenantId,
                            AgencyId = entry.AgencyId,
                            ProjectId = projectId,
                            Type = "publish.failed",
                            Title = $"Invio newsletter fallito: {contentTitle}",
                            Body = nlEx.Message,
                            Read = false
                        });
                    }
                }

                await context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Calendar entry {EntryId} publish result: status={Status}",
                    entry.Id, entry.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-publish calendar entry {EntryId}", entry.Id);
                entry.Status = CalendarEntryStatus.Failed;
                entry.ErrorMessage = ex.Message;

                try
                {
                    var contentTitle = entry.Content?.Title ?? "Contenuto";
                    var projectId = entry.Content?.ProjectId;
                    context.Set<Notification>().Add(new Notification
                    {
                        TenantId = entry.TenantId,
                        AgencyId = entry.AgencyId,
                        ProjectId = projectId,
                        Type = "publish.failed",
                        Title = entry.Platform != null
                            ? $"Pubblicazione fallita su {entry.Platform.Value}: {contentTitle}"
                            : $"Invio fallito: {contentTitle}",
                        Body = ex.Message,
                        Read = false
                    });
                    await context.SaveChangesAsync(ct);
                }
                catch { }
            }
        }
    }
}
