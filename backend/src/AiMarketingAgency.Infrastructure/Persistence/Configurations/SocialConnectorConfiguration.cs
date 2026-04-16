using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class SocialConnectorConfiguration : IEntityTypeConfiguration<SocialConnector>
{
    public void Configure(EntityTypeBuilder<SocialConnector> builder)
    {
        builder.ToTable("social_connectors");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Platform).HasConversion<int>();
        builder.Property(c => c.AccessToken).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.RefreshToken).HasMaxLength(2000);
        builder.Property(c => c.AccountId).HasMaxLength(200);
        builder.Property(c => c.AccountName).HasMaxLength(200);
        builder.Property(c => c.ProfileImageUrl).HasMaxLength(500);

        builder.HasOne(c => c.Agency)
            .WithMany()
            .HasForeignKey(c => c.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.AgencyId, c.ProjectId, c.Platform }).IsUnique();
    }
}
