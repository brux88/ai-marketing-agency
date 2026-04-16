using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).HasMaxLength(64);
        builder.Property(n => n.Title).HasMaxLength(256);
        builder.HasIndex(n => new { n.TenantId, n.AgencyId, n.Read });
        builder.HasIndex(n => n.CreatedAt);
    }
}
