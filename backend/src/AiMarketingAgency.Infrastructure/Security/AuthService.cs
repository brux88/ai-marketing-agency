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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AiMarketingAgency.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ITransactionalEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAppDbContext context, IConfiguration configuration, ITransactionalEmailService emailService, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName ?? $"{request.FullName}'s Workspace",
            Slug = GenerateSlug(request.CompanyName ?? request.FullName),
            Plan = PlanTier.FreeTrial,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);

        var passwordHash = HashPassword(request.Password);
        var confirmationToken = GenerateSecureToken();
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.Email,
            FullName = request.FullName,
            ExternalId = passwordHash,
            Role = UserRole.Owner,
            IsEmailConfirmed = false,
            EmailConfirmationToken = confirmationToken,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

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
            MaxProjects = 3,
            MaxJobsPerMonth = 50,
            CreatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);

        await _context.SaveChangesAsync(ct);

        var frontendUrl = _configuration["Frontend:Url"] ?? "https://wepostai.com";
        var confirmationLink = $"{frontendUrl}/confirm-email?token={confirmationToken}";

        try
        {
            await _emailService.SendEmailConfirmationAsync(request.Email, request.FullName, confirmationLink, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", request.Email);
        }

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

    public async Task ConfirmEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token, ct);

        if (user == null)
            throw new InvalidOperationException("Token di conferma non valido.");

        if (user.IsEmailConfirmed)
            return;

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _context.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendWelcomeAsync(user.Email, user.FullName, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user == null)
            return;

        var resetToken = GenerateSecureToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync(ct);

        var frontendUrl = _configuration["Frontend:Url"] ?? "https://wepostai.com";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

        await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetLink, ct);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token, ct);

        if (user == null)
            throw new InvalidOperationException("Token di reset non valido.");

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Il token di reset è scaduto. Richiedi un nuovo reset.");

        user.ExternalId = HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _context.SaveChangesAsync(ct);
    }

    public async Task ResendConfirmationAsync(string email, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user == null || user.IsEmailConfirmed)
            return;

        if (string.IsNullOrEmpty(user.EmailConfirmationToken))
        {
            user.EmailConfirmationToken = GenerateSecureToken();
            await _context.SaveChangesAsync(ct);
        }

        var frontendUrl = _configuration["Frontend:Url"] ?? "https://wepostai.com";
        var confirmationLink = $"{frontendUrl}/confirm-email?token={user.EmailConfirmationToken}";

        await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, confirmationLink, ct);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("Utente non trovato.");

        if (!VerifyPassword(currentPassword, user.ExternalId))
            throw new UnauthorizedAccessException("La password attuale non è corretta.");

        user.ExternalId = HashPassword(newPassword);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateProfileAsync(Guid userId, string fullName, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("Utente non trovato.");

        user.FullName = fullName;
        await _context.SaveChangesAsync(ct);
    }

    public async Task RequestAccountDeletionAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("Utente non trovato.");

        var deletionToken = GenerateSecureToken();
        user.AccountDeletionToken = deletionToken;
        user.AccountDeletionTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync(ct);

        var frontendUrl = _configuration["Frontend:Url"] ?? "https://wepostai.com";
        var confirmationLink = $"{frontendUrl}/confirm-delete-account?token={deletionToken}";

        await _emailService.SendAccountDeletionConfirmationAsync(user.Email, user.FullName, confirmationLink, ct);
    }

    public async Task ConfirmAccountDeletionAsync(string token, CancellationToken ct = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.AccountDeletionToken == token, ct)
            ?? throw new InvalidOperationException("Token non valido.");

        if (user.AccountDeletionTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Il token è scaduto. Richiedi una nuova eliminazione.");

        var tenantId = user.TenantId;

        var agencies = await _context.Agencies
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantId)
            .ToListAsync(ct);
        _context.Agencies.RemoveRange(agencies);

        var subscriptions = await _context.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);
        _context.Subscriptions.RemoveRange(subscriptions);

        var users = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(ct);
        _context.Users.RemoveRange(users);

        if (user.Tenant != null)
        {
            user.Tenant.IsActive = false;
            user.Tenant.Name = $"[DELETED] {user.Tenant.Name}";
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
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
            new UserInfo(user.Id, user.Email, user.FullName, tenant.Id, user.Role.ToString(), user.IsEmailConfirmed)
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
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        if (!hash.StartsWith("$2"))
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes) == hash;
        }
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public async Task EnsureSuperAdminAsync(CancellationToken ct = default)
    {
        var email = _configuration["SuperAdmin:Email"] ?? "admin@aimarketing.local";
        var exists = await _context.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Role == UserRole.SuperAdmin, ct);
        if (exists) return;

        var password = _configuration["SuperAdmin:Password"] ?? "Admin123!";
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Platform Admin",
            Slug = "platform-admin",
            Plan = PlanTier.Enterprise,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = email,
            FullName = "Super Admin",
            ExternalId = HashPassword(password),
            Role = UserRole.SuperAdmin,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        _context.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            StripeCustomerId = $"admin_{tenant.Id}",
            PlanTier = PlanTier.Enterprise,
            Status = SubscriptionStatus.Active,
            MaxAgencies = 999,
            MaxJobsPerMonth = 99999,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            + "-" + Guid.NewGuid().ToString()[..8];
    }
}
