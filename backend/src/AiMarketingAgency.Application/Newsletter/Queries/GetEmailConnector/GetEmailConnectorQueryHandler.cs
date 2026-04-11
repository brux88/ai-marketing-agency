using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Newsletter.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Queries.GetEmailConnector;

public class GetEmailConnectorQueryHandler : IRequestHandler<GetEmailConnectorQuery, EmailConnectorDto?>
{
    private readonly IAppDbContext _context;

    public GetEmailConnectorQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<EmailConnectorDto?> Handle(GetEmailConnectorQuery request, CancellationToken cancellationToken)
    {
        var connector = await _context.EmailConnectors
            .FirstOrDefaultAsync(c => c.AgencyId == request.AgencyId, cancellationToken);

        if (connector == null) return null;

        return new EmailConnectorDto
        {
            Id = connector.Id,
            ProviderType = connector.ProviderType,
            SmtpHost = connector.SmtpHost,
            SmtpPort = connector.SmtpPort,
            SmtpUsername = connector.SmtpUsername,
            HasSmtpPassword = !string.IsNullOrEmpty(connector.SmtpPassword),
            HasApiKey = !string.IsNullOrEmpty(connector.ApiKey),
            FromEmail = connector.FromEmail,
            FromName = connector.FromName,
            IsActive = connector.IsActive
        };
    }
}
