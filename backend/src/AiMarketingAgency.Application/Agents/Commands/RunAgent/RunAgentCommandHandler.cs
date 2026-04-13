using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.Agents.Commands.RunAgent;

public class RunAgentCommandHandler : IRequestHandler<RunAgentCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUsageGuard _usageGuard;
    private readonly IAgentJobProcessor _jobProcessor;

    public RunAgentCommandHandler(
        IAppDbContext context,
        ITenantContext tenantContext,
        IUsageGuard usageGuard,
        IAgentJobProcessor jobProcessor)
    {
        _context = context;
        _tenantContext = tenantContext;
        _usageGuard = usageGuard;
        _jobProcessor = jobProcessor;
    }

    public async Task<Guid> Handle(RunAgentCommand request, CancellationToken cancellationToken)
    {
        if (!await _usageGuard.CanRunJobAsync(_tenantContext.TenantId, cancellationToken))
            throw new InvalidOperationException("You have reached the maximum number of jobs for your plan this month.");

        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException("Agency not found.");

        if (agency.DefaultLlmProviderKeyId == null)
            throw new InvalidOperationException("Nessuna chiave LLM configurata per questa agenzia. Vai nelle impostazioni per configurare un provider LLM.");

        var job = new AgentJob
        {
            TenantId = _tenantContext.TenantId,
            AgencyId = request.AgencyId,
            AgentType = request.AgentType,
            Status = JobStatus.Queued,
            Input = request.Input,
            ProjectId = request.ProjectId,
            ImageMode = request.ImageMode,
            ImageCount = Math.Clamp(request.ImageCount, 1, 10)
        };

        _context.AgentJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        await _usageGuard.IncrementJobCountAsync(_tenantContext.TenantId, request.AgencyId, cancellationToken);

        // Process synchronously for now (Service Bus dispatch will replace this later)
        await _jobProcessor.ProcessJobAsync(job.Id, cancellationToken);

        return job.Id;
    }
}
