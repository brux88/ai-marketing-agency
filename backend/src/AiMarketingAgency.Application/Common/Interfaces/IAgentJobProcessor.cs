namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IAgentJobProcessor
{
    Task ProcessJobAsync(Guid jobId, CancellationToken ct = default);
}
