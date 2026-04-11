using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agencies.Commands.UpdateDefaultLlm;

public class UpdateDefaultLlmCommandHandler : IRequestHandler<UpdateDefaultLlmCommand>
{
    private readonly IAppDbContext _context;

    public UpdateDefaultLlmCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDefaultLlmCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        agency.DefaultLlmProviderKeyId = request.DefaultLlmProviderKeyId;
        agency.ImageLlmProviderKeyId = request.ImageLlmProviderKeyId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
