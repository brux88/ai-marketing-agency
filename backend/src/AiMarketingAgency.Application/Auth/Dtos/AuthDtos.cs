namespace AiMarketingAgency.Application.Auth.Dtos;

public record RegisterRequest(string Email, string Password, string FullName, string? CompanyName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserInfo User);
public record UserInfo(Guid Id, string Email, string FullName, Guid TenantId, string Role);
public record RefreshRequest(string RefreshToken);
