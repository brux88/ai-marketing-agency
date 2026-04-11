using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.ContentSources.Commands.DeleteContentSource;

public class DeleteContentSourceCommandHandler : IRequestHandler<DeleteContentSourceCommand>
{
    private readonly IAppDbContext _context;

    public DeleteContentSourceCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteContentSourceCommand request, CancellationToken cancellationToken)
    {
        var contentSource = await _context.ContentSources
            .FirstOrDefaultAsync(cs => cs.Id == request.Id && cs.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Content source not found.");

        contentSource.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
