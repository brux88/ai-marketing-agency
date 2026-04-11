using AiMarketingAgency.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IAppDbContext _context;

    public DeleteProjectCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.AgencyId == request.AgencyId && p.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        project.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
