using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class EmailConnectorConfiguration : IEntityTypeConfiguration<EmailConnector>
{
    public void Configure(EntityTypeBuilder<EmailConnector> builder)
    {
        builder.ToTable("email_connectors");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ProviderType).HasConversion<int>();
        builder.Property(c => c.SmtpHost).HasMaxLength(500);
        builder.Property(c => c.SmtpUsername).HasMaxLength(500);
        builder.Property(c => c.SmtpPassword).HasMaxLength(1000);
        builder.Property(c => c.ApiKey).HasMaxLength(1000);
        builder.Property(c => c.FromEmail).HasMaxLength(200).IsRequired();
        builder.Property(c => c.FromName).HasMaxLength(200).IsRequired();

        builder.HasOne(c => c.Agency)
            .WithMany()
            .HasForeignKey(c => c.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.AgencyId).IsUnique();
    }
}
