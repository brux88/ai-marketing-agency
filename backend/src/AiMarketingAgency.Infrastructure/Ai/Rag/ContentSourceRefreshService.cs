using AiMarketingAgency.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.Ai.Rag;

public class ContentSourceRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ContentSourceRefreshService> _logger;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(2);

    public ContentSourceRefreshService(IServiceScopeFactory scopeFactory, ILogger<ContentSourceRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Content source refresh service started. Interval: {Interval}", RefreshInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAllSourcesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing content sources");
            }

            await Task.Delay(RefreshInterval, stoppingToken);
        }
    }

    private async Task RefreshAllSourcesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var fetcher = scope.ServiceProvider.GetRequiredService<IContentFetcherService>();

        // Get all active agencies with content sources
        var agencies = await context.Agencies
            .IgnoreQueryFilters()
            .Where(a => a.IsActive)
            .Select(a => a.Id)
            .ToListAsync(ct);

        foreach (var agencyId in agencies)
        {
            try
            {
                await fetcher.RefreshAllSourcesAsync(agencyId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh sources for agency {AgencyId}", agencyId);
            }
        }

        // Clean up old chunks (older than 7 days)
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var oldChunks = await context.ContentChunks
            .IgnoreQueryFilters()
            .Where(c => c.FetchedAt < cutoff)
            .ToListAsync(ct);

        foreach (var chunk in oldChunks)
            context.ContentChunks.Remove(chunk);

        if (oldChunks.Any())
        {
            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Cleaned up {Count} old content chunks", oldChunks.Count);
        }
    }
}
