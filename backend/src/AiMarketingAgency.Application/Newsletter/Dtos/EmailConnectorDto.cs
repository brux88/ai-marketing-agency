using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Newsletter.Dtos;

public class EmailConnectorDto
{
    public Guid Id { get; set; }
    public EmailProviderType ProviderType { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public bool HasSmtpPassword { get; set; }
    public bool HasApiKey { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
