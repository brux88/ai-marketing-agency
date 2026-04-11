using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class ContentSourceConfiguration : IEntityTypeConfiguration<ContentSource>
{
    public void Configure(EntityTypeBuilder<ContentSource> builder)
    {
        builder.ToTable("content_sources");
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Url).HasMaxLength(1000).IsRequired();
        builder.Property(cs => cs.Name).HasMaxLength(200);
        builder.Property(cs => cs.Type).HasConversion<int>();

        builder.HasOne(cs => cs.Agency)
            .WithMany(a => a.ContentSources)
            .HasForeignKey(cs => cs.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Project)
            .WithMany(p => p.ContentSources)
            .HasForeignKey(cs => cs.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
