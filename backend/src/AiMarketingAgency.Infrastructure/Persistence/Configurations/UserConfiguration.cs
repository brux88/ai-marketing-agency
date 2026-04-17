using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.ExternalId).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Role).HasConversion<int>();
        builder.Property(u => u.IsEmailConfirmed).HasDefaultValue(false);
        builder.Property(u => u.EmailConfirmationToken).HasMaxLength(128);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(128);
        builder.Property(u => u.AccountDeletionToken).HasMaxLength(128);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.ExternalId);

        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
