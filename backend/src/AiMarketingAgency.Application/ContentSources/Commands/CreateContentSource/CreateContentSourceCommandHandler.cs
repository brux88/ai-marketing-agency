using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.ContentSources.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.ContentSources.Commands.CreateContentSource;

public class CreateContentSourceCommandHandler : IRequestHandler<CreateContentSourceCommand, ContentSourceDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateContentSourceCommandHandler(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ContentSourceDto> Handle(CreateContentSourceCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        var contentSource = new ContentSource
        {
            AgencyId = request.AgencyId,
            TenantId = _tenantContext.TenantId,
            ProjectId = request.ProjectId,
            Type = request.Type,
            Url = request.Url,
            Name = request.Name,
            Config = request.Config,
        };

        _context.ContentSources.Add(contentSource);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContentSourceDto
        {
            Id = contentSource.Id,
            AgencyId = contentSource.AgencyId,
            ProjectId = contentSource.ProjectId,
            Type = contentSource.Type,
            Url = contentSource.Url,
            Name = contentSource.Name,
            Config = contentSource.Config,
            IsActive = contentSource.IsActive,
            LastFetchedAt = contentSource.LastFetchedAt,
            CreatedAt = contentSource.CreatedAt,
        };
    }
}
