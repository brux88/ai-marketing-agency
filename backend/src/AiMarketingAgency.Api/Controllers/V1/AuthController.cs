using AiMarketingAgency.Application.Auth.Dtos;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITenantContext _tenantContext;

    public AuthController(IAuthService authService, ITenantContext tenantContext)
    {
        _authService = authService;
        _tenantContext = tenantContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    {
        await _authService.ConfirmEmailAsync(request.Token, ct);
        return Ok(ApiResponse<string>.Ok("Email confermata con successo."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request.Email, ct);
        return Ok(ApiResponse<string>.Ok("Se l'email esiste, riceverai un link per reimpostare la password."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        return Ok(ApiResponse<string>.Ok("Password reimpostata con successo."));
    }

    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<ApiResponse<string>>> ResendConfirmation([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _authService.ResendConfirmationAsync(request.Email, ct);
        return Ok(ApiResponse<string>.Ok("Se l'email esiste e non è confermata, riceverai un nuovo link di conferma."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(_tenantContext.UserId, request.CurrentPassword, request.NewPassword, ct);
        return Ok(ApiResponse<string>.Ok("Password aggiornata con successo."));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        await _authService.UpdateProfileAsync(_tenantContext.UserId, request.FullName, ct);
        return Ok(ApiResponse<string>.Ok("Profilo aggiornato con successo."));
    }

    [Authorize]
    [HttpPost("request-account-deletion")]
    public async Task<ActionResult<ApiResponse<string>>> RequestAccountDeletion(CancellationToken ct)
    {
        await _authService.RequestAccountDeletionAsync(_tenantContext.UserId, ct);
        return Ok(ApiResponse<string>.Ok("Ti abbiamo inviato un'email di conferma per l'eliminazione dell'account."));
    }

    [HttpPost("confirm-account-deletion")]
    public async Task<ActionResult<ApiResponse<string>>> ConfirmAccountDeletion([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    {
        await _authService.ConfirmAccountDeletionAsync(request.Token, ct);
        return Ok(ApiResponse<string>.Ok("Account eliminato con successo."));
    }
}
