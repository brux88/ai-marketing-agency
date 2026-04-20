using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class UserDeviceTokenConfiguration : IEntityTypeConfiguration<UserDeviceToken>
{
    public void Configure(EntityTypeBuilder<UserDeviceToken> builder)
    {
        builder.ToTable("user_device_tokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FcmToken).HasMaxLength(512).IsRequired();
        builder.Property(t => t.Platform).HasMaxLength(16).IsRequired();
        builder.Property(t => t.DeviceName).HasMaxLength(128);
        builder.HasIndex(t => t.FcmToken).IsUnique();
        builder.HasIndex(t => new { t.TenantId, t.UserId });
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
