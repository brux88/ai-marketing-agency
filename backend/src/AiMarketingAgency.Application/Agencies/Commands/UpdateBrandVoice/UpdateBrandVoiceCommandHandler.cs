using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateBrandVoice;

public class UpdateBrandVoiceCommandHandler : IRequestHandler<UpdateBrandVoiceCommand>
{
    private readonly IAppDbContext _context;

    public UpdateBrandVoiceCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateBrandVoiceCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        agency.BrandVoice = request.BrandVoice;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
