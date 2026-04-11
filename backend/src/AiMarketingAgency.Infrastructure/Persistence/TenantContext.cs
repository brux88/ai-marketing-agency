using AiMarketingAgency.Application.Common.Interfaces;

namespace AiMarketingAgency.Infrastructure.Persistence;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }

    public void SetTenant(Guid tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }
}
