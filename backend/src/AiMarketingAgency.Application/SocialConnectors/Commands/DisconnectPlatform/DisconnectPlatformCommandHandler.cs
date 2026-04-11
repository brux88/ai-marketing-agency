using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.DisconnectPlatform;

public class DisconnectPlatformCommandHandler : IRequestHandler<DisconnectPlatformCommand>
{
    private readonly IAppDbContext _context;

    public DisconnectPlatformCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DisconnectPlatformCommand request, CancellationToken cancellationToken)
    {
        var connector = await _context.SocialConnectors
            .FirstOrDefaultAsync(c => c.Id == request.ConnectorId && c.AgencyId == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Connector not found.");

        _context.SocialConnectors.Remove(connector);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
