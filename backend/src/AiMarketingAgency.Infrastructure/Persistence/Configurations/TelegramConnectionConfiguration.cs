using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class TelegramConnectionConfiguration : IEntityTypeConfiguration<TelegramConnection>
{
    public void Configure(EntityTypeBuilder<TelegramConnection> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.Agency)
            .WithMany()
            .HasForeignKey(c => c.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.AgencyId, c.ProjectId });
    }
}
