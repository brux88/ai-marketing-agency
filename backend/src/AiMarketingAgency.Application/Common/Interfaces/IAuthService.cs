using AiMarketingAgency.Application.Auth.Dtos;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task EnsureSuperAdminAsync(CancellationToken ct = default);
}
