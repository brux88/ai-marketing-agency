using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agencies");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.WebsiteUrl).HasMaxLength(500);
        builder.Property(a => a.LogoUrl).HasMaxLength(500);
        builder.Property(a => a.ApprovalMode).HasConversion<int>();

        builder.OwnsOne(a => a.BrandVoice, bv =>
        {
            bv.ToJson();
        });

        builder.OwnsOne(a => a.TargetAudience, ta =>
        {
            ta.ToJson();
            ta.OwnsMany(t => t.Personas);
        });

        builder.HasOne(a => a.Tenant)
            .WithMany(t => t.Agencies)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.DefaultLlmProviderKey)
            .WithMany()
            .HasForeignKey(a => a.DefaultLlmProviderKeyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ImageLlmProviderKey)
            .WithMany()
            .HasForeignKey(a => a.ImageLlmProviderKeyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.VideoLlmProviderKey)
            .WithMany()
            .HasForeignKey(a => a.VideoLlmProviderKeyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
