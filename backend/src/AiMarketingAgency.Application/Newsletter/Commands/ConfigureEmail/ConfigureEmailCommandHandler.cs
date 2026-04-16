using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Newsletter.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Commands.ConfigureEmail;

public class ConfigureEmailCommandHandler : IRequestHandler<ConfigureEmailCommand, EmailConnectorDto>
{
    private readonly IAppDbContext _context;

    public ConfigureEmailCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<EmailConnectorDto> Handle(ConfigureEmailCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.EmailConnectors
            .FirstOrDefaultAsync(
                c => c.AgencyId == request.AgencyId && c.ProjectId == request.ProjectId,
                cancellationToken);

        if (existing != null)
        {
            existing.ProviderType = request.ProviderType;
            existing.SmtpHost = request.SmtpHost;
            existing.SmtpPort = request.SmtpPort;
            existing.SmtpUsername = request.SmtpUsername;
            if (!string.IsNullOrEmpty(request.SmtpPassword))
                existing.SmtpPassword = request.SmtpPassword;
            if (!string.IsNullOrEmpty(request.ApiKey))
                existing.ApiKey = request.ApiKey;
            existing.FromEmail = request.FromEmail;
            existing.FromName = request.FromName;
            existing.IsActive = true;
        }
        else
        {
            existing = new EmailConnector
            {
                AgencyId = request.AgencyId,
                ProjectId = request.ProjectId,
                ProviderType = request.ProviderType,
                SmtpHost = request.SmtpHost,
                SmtpPort = request.SmtpPort,
                SmtpUsername = request.SmtpUsername,
                SmtpPassword = request.SmtpPassword,
                ApiKey = request.ApiKey,
                FromEmail = request.FromEmail,
                FromName = request.FromName,
                IsActive = true
            };
            _context.EmailConnectors.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new EmailConnectorDto
        {
            Id = existing.Id,
            ProjectId = existing.ProjectId,
            ProviderType = existing.ProviderType,
            SmtpHost = existing.SmtpHost,
            SmtpPort = existing.SmtpPort,
            SmtpUsername = existing.SmtpUsername,
            HasSmtpPassword = !string.IsNullOrEmpty(existing.SmtpPassword),
            HasApiKey = !string.IsNullOrEmpty(existing.ApiKey),
            FromEmail = existing.FromEmail,
            FromName = existing.FromName,
            IsActive = existing.IsActive
        };
    }
}
