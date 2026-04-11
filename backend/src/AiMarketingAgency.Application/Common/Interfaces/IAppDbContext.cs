using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Agency> Agencies { get; }
    DbSet<ContentSource> ContentSources { get; }
    DbSet<LlmProviderKey> LlmProviderKeys { get; }
    DbSet<AgentJob> AgentJobs { get; }
    DbSet<GeneratedContent> GeneratedContents { get; }
    DbSet<Project> Projects { get; }
    DbSet<EditorialCalendarEntry> CalendarEntries { get; }
    DbSet<ContentSchedule> ContentSchedules { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<UsageRecord> UsageRecords { get; }
    DbSet<SocialConnector> SocialConnectors { get; }
    DbSet<EmailConnector> EmailConnectors { get; }
    DbSet<NewsletterSubscriber> NewsletterSubscribers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
