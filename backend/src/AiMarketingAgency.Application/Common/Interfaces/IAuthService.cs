using AiMarketingAgency.Application.Auth.Dtos;

namespace AiMarketingAgency.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task EnsureSuperAdminAsync(CancellationToken ct = default);
    Task ConfirmEmailAsync(string token, CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);
    Task ResendConfirmationAsync(string email, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task UpdateProfileAsync(Guid userId, string fullName, CancellationToken ct = default);
    Task RequestAccountDeletionAsync(Guid userId, CancellationToken ct = default);
    Task ConfirmAccountDeletionAsync(string token, CancellationToken ct = default);
}
