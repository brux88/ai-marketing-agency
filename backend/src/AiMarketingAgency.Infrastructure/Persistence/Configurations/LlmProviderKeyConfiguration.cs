using AiMarketingAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiMarketingAgency.Infrastructure.Persistence.Configurations;

public class LlmProviderKeyConfiguration : IEntityTypeConfiguration<LlmProviderKey>
{
    public void Configure(EntityTypeBuilder<LlmProviderKey> builder)
    {
        builder.ToTable("llm_provider_keys");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(k => k.EncryptedApiKey).HasMaxLength(1000).IsRequired();
        builder.Property(k => k.EncryptedApiKeySecret).HasMaxLength(1000);
        builder.Property(k => k.KeyVaultSecretName).HasMaxLength(500);
        builder.Property(k => k.ModelName).HasMaxLength(100);
        builder.Property(k => k.BaseUrl).HasMaxLength(500);
        builder.Property(k => k.ProviderType).HasConversion<int>();
        builder.Property(k => k.Category).HasConversion<int>();

        builder.HasOne(k => k.Tenant)
            .WithMany(t => t.LlmProviderKeys)
            .HasForeignKey(k => k.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
