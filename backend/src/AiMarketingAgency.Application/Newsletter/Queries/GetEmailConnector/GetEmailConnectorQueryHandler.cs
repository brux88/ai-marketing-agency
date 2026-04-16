using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Newsletter.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Newsletter.Queries.GetEmailConnector;

public class GetEmailConnectorQueryHandler : IRequestHandler<GetEmailConnectorQuery, List<EmailConnectorDto>>
{
    private readonly IAppDbContext _context;

    public GetEmailConnectorQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmailConnectorDto>> Handle(GetEmailConnectorQuery request, CancellationToken cancellationToken)
    {
        var connectors = await _context.EmailConnectors
            .Where(c => c.AgencyId == request.AgencyId)
            .Select(c => new EmailConnectorDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                ProjectName = c.Project != null ? c.Project.Name : null,
                ProviderType = c.ProviderType,
                SmtpHost = c.SmtpHost,
                SmtpPort = c.SmtpPort,
                SmtpUsername = c.SmtpUsername,
                HasSmtpPassword = !string.IsNullOrEmpty(c.SmtpPassword),
                HasApiKey = !string.IsNullOrEmpty(c.ApiKey),
                FromEmail = c.FromEmail,
                FromName = c.FromName,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        // Default (agency-level, ProjectId == null) first, then per-project alphabetical
        return connectors
            .OrderBy(c => c.ProjectId == null ? 0 : 1)
            .ThenBy(c => c.ProjectName)
            .ToList();
    }
}
