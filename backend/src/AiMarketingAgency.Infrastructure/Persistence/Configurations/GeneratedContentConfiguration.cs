using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class GeneratedContentConfiguration : IEntityTypeConfiguration<GeneratedContent>
{
    public void Configure(EntityTypeBuilder<GeneratedContent> builder)
    {
        builder.ToTable("generated_contents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Body).IsRequired();
        builder.Property(c => c.ContentType).HasConversion<int>();
        builder.Property(c => c.Status).HasConversion<int>();
        builder.Property(c => c.QualityScore).HasPrecision(5, 2);
        builder.Property(c => c.RelevanceScore).HasPrecision(5, 2);
        builder.Property(c => c.SeoScore).HasPrecision(5, 2);
        builder.Property(c => c.BrandVoiceScore).HasPrecision(5, 2);
        builder.Property(c => c.OverallScore).HasPrecision(5, 2);
        builder.Property(c => c.ScoreExplanation).HasMaxLength(2000);

        builder.HasOne(c => c.Agency)
            .WithMany(a => a.GeneratedContents)
            .HasForeignKey(c => c.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Job)
            .WithMany(j => j.GeneratedContents)
            .HasForeignKey(c => c.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Project)
            .WithMany(p => p.GeneratedContents)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.ImageUrl).HasMaxLength(2000);
        builder.Property(c => c.ImagePrompt).HasMaxLength(2000);
        builder.Property(c => c.ImageUrls).HasMaxLength(8000);
        builder.Property(c => c.VideoUrl).HasMaxLength(2000);
        builder.Property(c => c.VideoPrompt).HasMaxLength(2000);
    }
}
