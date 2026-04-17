namespace AiMarketingAgency.Application.Auth.Dtos;

public record RegisterRequest(string Email, string Password, string FullName, string? CompanyName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserInfo User);
public record UserInfo(Guid Id, string Email, string FullName, Guid TenantId, string Role, bool IsEmailConfirmed);
public record RefreshRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ConfirmEmailRequest(string Token);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateProfileRequest(string FullName);
