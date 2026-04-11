using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(500);
        builder.Property(p => p.LogoUrl).HasMaxLength(500);

        builder.OwnsOne(p => p.BrandVoice, bv =>
        {
            bv.ToJson();
        });

        builder.OwnsOne(p => p.TargetAudience, ta =>
        {
            ta.ToJson();
            ta.OwnsMany(t => t.Personas);
        });

        builder.HasOne(p => p.Agency)
            .WithMany(a => a.Projects)
            .HasForeignKey(p => p.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
