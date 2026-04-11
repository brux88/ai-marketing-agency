using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class EditorialCalendarEntryConfiguration : IEntityTypeConfiguration<EditorialCalendarEntry>
{
    public void Configure(EntityTypeBuilder<EditorialCalendarEntry> builder)
    {
        builder.ToTable("editorial_calendar");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Platform).HasConversion<int?>();
        builder.Property(e => e.Status).HasConversion<int>();

        builder.HasOne(e => e.Agency)
            .WithMany(a => a.CalendarEntries)
            .HasForeignKey(e => e.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Content)
            .WithMany(c => c.CalendarEntries)
            .HasForeignKey(e => e.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.AgencyId, e.ScheduledAt });
    }
}
