using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("project_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(500).IsRequired();
        builder.Property(d => d.FileName).HasMaxLength(500).IsRequired();
        builder.Property(d => d.FileUrl).HasMaxLength(2000).IsRequired();

        builder.HasOne(d => d.Agency)
            .WithMany()
            .HasForeignKey(d => d.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Project)
            .WithMany()
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
