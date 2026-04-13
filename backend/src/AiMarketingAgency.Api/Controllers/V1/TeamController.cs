using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Entities;
using AiMarketingAgency.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/team")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public TeamController(IAppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    [HttpGet("members")]
    public async Task<ActionResult<ApiResponse<List<TeamMemberDto>>>> GetMembers(CancellationToken ct)
    {
        var members = await _context.Users
            .Select(u => new TeamMemberDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<TeamMemberDto>>.Ok(members));
    }

    [HttpGet("invitations")]
    public async Task<ActionResult<ApiResponse<List<InvitationDto>>>> GetInvitations(CancellationToken ct)
    {
        var invitations = await _context.TeamInvitations
            .Where(i => i.Status == InvitationStatus.Pending)
            .Select(i => new InvitationDto
            {
                Id = i.Id,
                Email = i.Email,
                Role = i.Role.ToString(),
                Status = i.Status.ToString(),
                ExpiresAt = i.ExpiresAt,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<InvitationDto>>.Ok(invitations));
    }

    [HttpPost("invite")]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> InviteMember(
        [FromBody] InviteRequest request, CancellationToken ct)
    {
        // Check if user already exists in this tenant
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (existingUser != null)
            return BadRequest(ApiResponse<InvitationDto>.Fail("User already a team member."));

        // Check if there's a pending invitation
        var existingInvite = await _context.TeamInvitations
            .FirstOrDefaultAsync(i => i.Email == request.Email && i.Status == InvitationStatus.Pending, ct);
        if (existingInvite != null)
            return BadRequest(ApiResponse<InvitationDto>.Fail("Invitation already sent."));

        var invitation = new TeamInvitation
        {
            TenantId = _tenantContext.TenantId,
            Email = request.Email,
            Role = Enum.TryParse<UserRole>(request.Role, out var role) ? role : UserRole.Member,
            InvitedBy = _tenantContext.UserId,
            Token = Guid.NewGuid().ToString("N"),
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.TeamInvitations.Add(invitation);
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<InvitationDto>.Ok(new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role.ToString(),
            Status = invitation.Status.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt
        }));
    }

    [HttpPost("invitations/{invitationId:guid}/revoke")]
    public async Task<ActionResult> RevokeInvitation(Guid invitationId, CancellationToken ct)
    {
        var invitation = await _context.TeamInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId, ct);

        if (invitation == null) return NotFound();

        invitation.Status = InvitationStatus.Revoked;
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true });
    }

    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    public async Task<ActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken ct)
    {
        var invitation = await _context.TeamInvitations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Token == request.Token && i.Status == InvitationStatus.Pending, ct);

        if (invitation == null)
            return BadRequest(new { error = "Invalid or expired invitation." });

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _context.SaveChangesAsync(ct);
            return BadRequest(new { error = "Invitation has expired." });
        }

        // Check if user already registered
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == invitation.Email, ct);

        if (existingUser != null)
        {
            // User exists but in different tenant - add to this tenant
            if (existingUser.TenantId != invitation.TenantId)
                return BadRequest(new { error = "User already belongs to another workspace." });
        }
        else
        {
            // Create new user
            var user = new User
            {
                TenantId = invitation.TenantId,
                Email = invitation.Email,
                FullName = request.FullName ?? invitation.Email.Split('@')[0],
                ExternalId = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = invitation.Role,
            };
            _context.Users.Add(user);
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true, message = "Invitation accepted. You can now log in." });
    }

    [HttpPut("members/{userId:guid}/role")]
    public async Task<ActionResult> UpdateRole(Guid userId, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return NotFound();

        if (Enum.TryParse<UserRole>(request.Role, out var role))
        {
            user.Role = role;
            await _context.SaveChangesAsync(ct);
        }

        return Ok(new { success = true });
    }

    [HttpDelete("members/{userId:guid}")]
    public async Task<ActionResult> RemoveMember(Guid userId, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return NotFound();

        if (user.Role == UserRole.Owner)
            return BadRequest(new { error = "Cannot remove the workspace owner." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true });
    }
}

public record InviteRequest(string Email, string? Role);
public record AcceptInvitationRequest(string Token, string? FullName, string Password);
public record UpdateRoleRequest(string Role);

public class TeamMemberDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class InvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
