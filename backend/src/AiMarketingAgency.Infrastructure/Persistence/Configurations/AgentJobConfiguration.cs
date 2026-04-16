using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class AgentJobConfiguration : IEntityTypeConfiguration<AgentJob>
{
    public void Configure(EntityTypeBuilder<AgentJob> builder)
    {
        builder.ToTable("agent_jobs");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.AgentType).HasConversion<int>();
        builder.Property(j => j.Status).HasConversion<int>();
        builder.Property(j => j.CostEstimate).HasPrecision(18, 6);

        builder.HasOne(j => j.Agency)
            .WithMany(a => a.AgentJobs)
            .HasForeignKey(j => j.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(j => j.Project)
            .WithMany(p => p.AgentJobs)
            .HasForeignKey(j => j.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(j => j.Schedule)
            .WithMany()
            .HasForeignKey(j => j.ScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(j => j.ScheduleId);
    }
}
