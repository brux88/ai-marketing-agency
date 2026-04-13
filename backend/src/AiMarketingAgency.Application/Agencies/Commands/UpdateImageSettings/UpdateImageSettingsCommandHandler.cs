using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateImageSettings;

public class UpdateImageSettingsCommandHandler : IRequestHandler<UpdateImageSettingsCommand>
{
    private readonly IAppDbContext _context;

    public UpdateImageSettingsCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateImageSettingsCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        agency.EnableLogoOverlay = request.EnableLogoOverlay;
        agency.LogoOverlayPosition = Math.Clamp(request.LogoOverlayPosition, 0, 3);
        agency.LogoUrl = request.LogoUrl;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
