using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("newsletter_subscribers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Email).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(200);

        builder.HasOne(s => s.Agency)
            .WithMany()
            .HasForeignKey(s => s.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.AgencyId, s.Email }).IsUnique();
    }
}
