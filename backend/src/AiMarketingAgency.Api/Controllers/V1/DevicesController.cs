using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IPushNotificationService _pushService;
    private readonly ITenantContext _tenantContext;

    public DevicesController(IPushNotificationService pushService, ITenantContext tenantContext)
    {
        _pushService = pushService;
        _tenantContext = tenantContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register(
        [FromBody] RegisterDeviceRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FcmToken))
            return BadRequest(ApiResponse<object>.Fail("FcmToken is required."));

        await _pushService.RegisterTokenAsync(
            _tenantContext.UserId,
            request.FcmToken,
            string.IsNullOrWhiteSpace(request.Platform) ? "unknown" : request.Platform,
            request.DeviceName,
            ct);

        return Ok(ApiResponse<object>.Ok(new { registered = true }));
    }

    [HttpPost("unregister")]
    public async Task<ActionResult<ApiResponse<object>>> Unregister(
        [FromBody] UnregisterDeviceRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FcmToken))
            return BadRequest(ApiResponse<object>.Fail("FcmToken is required."));

        await _pushService.UnregisterTokenAsync(_tenantContext.UserId, request.FcmToken, ct);
        return Ok(ApiResponse<object>.Ok(new { unregistered = true }));
    }
}

public record RegisterDeviceRequest(string FcmToken, string? Platform, string? DeviceName);
public record UnregisterDeviceRequest(string FcmToken);
