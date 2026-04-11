using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AiMarketingAgency.Application.Auth.Dtos;
using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AiMarketingAgency.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(IAppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        // Create tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName ?? $"{request.FullName}'s Workspace",
            Slug = GenerateSlug(request.CompanyName ?? request.FullName),
            Plan = PlanTier.FreeTrial,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);

        // Create user
        var passwordHash = HashPassword(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.Email,
            FullName = request.FullName,
            ExternalId = passwordHash,
            Role = UserRole.Owner,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        // Create free trial subscription
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            StripeCustomerId = $"pending_{tenant.Id}",
            PlanTier = PlanTier.FreeTrial,
            Status = SubscriptionStatus.Trialing,
            TrialEndsAt = DateTime.UtcNow.AddDays(14),
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(14),
            MaxAgencies = 1,
            MaxJobsPerMonth = 20,
            CreatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);

        await _context.SaveChangesAsync(ct);

        return GenerateAuthResponse(user, tenant);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user == null || !VerifyPassword(request.Password, user.ExternalId))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return GenerateAuthResponse(user, user.Tenant);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // Simple refresh: decode the refresh token to get user ID
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            var principal = handler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true
            }, out _);

            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            return GenerateAuthResponse(user, user.Tenant);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }
    }

    private AuthResponse GenerateAuthResponse(User user, Tenant tenant)
    {
        var accessToken = GenerateJwtToken(user, tenant, TimeSpan.FromMinutes(
            int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "15")));
        var refreshToken = GenerateJwtToken(user, tenant, TimeSpan.FromDays(7));
        var expiresAt = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "15"));

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserInfo(user.Id, user.Email, user.FullName, tenant.Id, user.Role.ToString())
        );
    }

    private string GenerateJwtToken(User user, Tenant tenant, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("tenant_id", tenant.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            + "-" + Guid.NewGuid().ToString()[..8];
    }
}
