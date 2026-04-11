using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StripeCustomerId).HasMaxLength(256).IsRequired();
        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(256);
        builder.Property(s => s.PlanTier).HasConversion<int>();
        builder.Property(s => s.Status).HasConversion<int>();

        builder.HasIndex(s => s.StripeCustomerId).IsUnique();
        builder.HasIndex(s => s.TenantId).IsUnique();
    }
}
