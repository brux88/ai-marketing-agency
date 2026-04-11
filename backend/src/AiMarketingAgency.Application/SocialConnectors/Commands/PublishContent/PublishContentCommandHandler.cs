using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.PublishContent;

public class PublishContentCommandHandler : IRequestHandler<PublishContentCommand, PublishResult>
{
    private readonly IAppDbContext _context;
    private readonly ISocialPublishingServiceFactory _factory;

    public PublishContentCommandHandler(IAppDbContext context, ISocialPublishingServiceFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    public async Task<PublishResult> Handle(PublishContentCommand request, CancellationToken cancellationToken)
    {
        var connector = await _context.SocialConnectors
            .FirstOrDefaultAsync(c => c.Id == request.ConnectorId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Connector not found.");

        var content = await _context.GeneratedContents
            .FirstOrDefaultAsync(c => c.Id == request.ContentId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Content not found.");

        var publisher = _factory.Create(connector.Platform);
        return await publisher.PublishAsync(connector, content, cancellationToken);
    }
}
