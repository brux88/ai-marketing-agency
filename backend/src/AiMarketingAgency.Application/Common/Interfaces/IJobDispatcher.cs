using AiMarketingAgency.Domain.Enums;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IJobDispatcher
{
    Task DispatchAsync(Guid jobId, Guid agencyId, Guid tenantId, AgentType agentType, CancellationToken ct = default);
}
