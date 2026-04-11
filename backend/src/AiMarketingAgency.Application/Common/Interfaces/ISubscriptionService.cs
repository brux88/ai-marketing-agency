namespace AiMarketingAgency.Application.Common.Interfaces;

public interface ISubscriptionService
{
    Task<string> CreateCheckoutSessionAsync(Guid tenantId, string priceId, string successUrl, string cancelUrl, CancellationToken ct = default);
    Task<string> CreatePortalSessionAsync(Guid tenantId, string returnUrl, CancellationToken ct = default);
    Task HandleWebhookAsync(string json, string signature, CancellationToken ct = default);
}
