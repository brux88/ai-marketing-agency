using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.LlmKeys.Commands.DeleteLlmKey;

public class DeleteLlmKeyCommandHandler : IRequestHandler<DeleteLlmKeyCommand>
{
    private readonly IAppDbContext _context;

    public DeleteLlmKeyCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteLlmKeyCommand request, CancellationToken cancellationToken)
    {
        var key = await _context.LlmProviderKeys
            .FirstOrDefaultAsync(k => k.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("LLM key not found.");

        key.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
