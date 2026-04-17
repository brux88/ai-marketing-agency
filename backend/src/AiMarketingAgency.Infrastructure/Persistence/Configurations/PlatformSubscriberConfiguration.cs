using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class PlatformSubscriberConfiguration : IEntityTypeConfiguration<PlatformSubscriber>
{
    public void Configure(EntityTypeBuilder<PlatformSubscriber> builder)
    {
        builder.ToTable("platform_subscribers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Email).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(200);
        builder.Property(s => s.Source).HasMaxLength(100);

        builder.HasIndex(s => s.Email).IsUnique();
    }
}
