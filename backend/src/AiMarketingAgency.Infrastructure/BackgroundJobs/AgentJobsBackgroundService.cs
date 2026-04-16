using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.BackgroundJobs;

public class AgentJobsBackgroundService : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AgentJobsBackgroundService> _logger;

    public AgentJobsBackgroundService(
        IBackgroundJobQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AgentJobsBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AgentJobs background worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            (Guid jobId, Guid tenantId) item;
            try
            {
                item = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }

            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                    tenantContext.SetTenant(item.tenantId, Guid.Empty);
                    var processor = scope.ServiceProvider.GetRequiredService<IAgentJobProcessor>();
                    await processor.ProcessJobAsync(item.jobId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background agent job {JobId} failed", item.jobId);
                }
            }, stoppingToken);
        }
    }
}
