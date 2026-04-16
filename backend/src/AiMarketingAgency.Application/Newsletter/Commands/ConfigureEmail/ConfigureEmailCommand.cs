using AiMarketingAgency.Application.Newsletter.Dtos;
using AiMarketingAgency.Domain.Enums;
using MediatR;

namespace AiMarketingAgency.Application.Newsletter.Commands.ConfigureEmail;

public record ConfigureEmailCommand : IRequest<EmailConnectorDto>
{
    public Guid AgencyId { get; init; }
    public Guid? ProjectId { get; init; }
    public EmailProviderType ProviderType { get; init; }
    public string? SmtpHost { get; init; }
    public int? SmtpPort { get; init; }
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }
    public string? ApiKey { get; init; }
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
