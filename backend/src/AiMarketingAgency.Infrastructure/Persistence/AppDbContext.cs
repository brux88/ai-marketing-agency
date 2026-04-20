using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<ContentSource> ContentSources => Set<ContentSource>();
    public DbSet<LlmProviderKey> LlmProviderKeys => Set<LlmProviderKey>();
    public DbSet<AgentJob> AgentJobs => Set<AgentJob>();
    public DbSet<GeneratedContent> GeneratedContents => Set<GeneratedContent>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<EditorialCalendarEntry> CalendarEntries => Set<EditorialCalendarEntry>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<ContentSchedule> ContentSchedules => Set<ContentSchedule>();
    public DbSet<SocialConnector> SocialConnectors => Set<SocialConnector>();
    public DbSet<EmailConnector> EmailConnectors => Set<EmailConnector>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();
    public DbSet<TelegramConnection> TelegramConnections => Set<TelegramConnection>();
    public DbSet<ContentChunk> ContentChunks => Set<ContentChunk>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PlatformSubscriber> PlatformSubscribers => Set<PlatformSubscriber>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();
    public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();

    // EF Core evaluates this property per-instance for cached query filters
    private Guid CurrentTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply global query filters for tenant isolation
        // Using a DbContext instance member so EF Core re-evaluates per request
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // GeneratedContent gets a combined tenant + soft-delete filter.
            if (entityType.ClrType == typeof(GeneratedContent))
            {
                modelBuilder.Entity<GeneratedContent>()
                    .HasQueryFilter(e => e.TenantId == CurrentTenantId && !e.IsDeleted);
                continue;
            }

            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Auto-set TenantId on new tenant-scoped entities
        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Property(nameof(ITenantScoped.TenantId)).CurrentValue = _tenantContext.TenantId;
            }
        }

        // Auto-set Id for new BaseEntity entries
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.Id == Guid.Empty)
            {
                entry.Entity.Id = Guid.NewGuid();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
