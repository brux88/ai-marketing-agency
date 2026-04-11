using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class UsageRecordConfiguration : IEntityTypeConfiguration<UsageRecord>
{
    public void Configure(EntityTypeBuilder<UsageRecord> builder)
    {
        builder.ToTable("usage_records");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.EstimatedCost).HasPrecision(18, 6);

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Agency)
            .WithMany()
            .HasForeignKey(u => u.AgencyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => new { u.TenantId, u.Period });
    }
}
