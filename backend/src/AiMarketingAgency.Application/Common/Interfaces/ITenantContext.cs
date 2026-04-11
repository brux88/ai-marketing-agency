namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
    void SetTenant(Guid tenantId, Guid userId);
}
