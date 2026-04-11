using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class ContentScheduleConfiguration : IEntityTypeConfiguration<ContentSchedule>
{
    public void Configure(EntityTypeBuilder<ContentSchedule> builder)
    {
        builder.ToTable("content_schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Days).HasConversion<int>();
        builder.Property(s => s.TimeOfDay).IsRequired();
        builder.Property(s => s.TimeZone).HasMaxLength(100).IsRequired();
        builder.Property(s => s.AgentType).HasConversion<int>();
        builder.Property(s => s.Input).HasMaxLength(2000);

        builder.HasOne(s => s.Agency)
            .WithMany(a => a.Schedules)
            .HasForeignKey(s => s.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.AgencyId, s.IsActive });
        builder.HasIndex(s => s.NextRunAt);
    }
}
