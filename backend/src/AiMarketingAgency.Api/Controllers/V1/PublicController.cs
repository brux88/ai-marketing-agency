using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.Common.Models;
using AiMarketingAgency.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Api.Controllers.V1;

[ApiController]
[Route("api/v1/public")]
public class PublicController : ControllerBase
{
    private readonly IAppDbContext _context;

    public PublicController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpPost("newsletter/subscribe")]
    public async Task<ActionResult<ApiResponse<object>>> Subscribe(
        [FromBody] PublicSubscribeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.Fail("Email obbligatoria"));

        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _context.PlatformSubscribers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Email == email, ct);

        if (existing != null)
        {
            if (existing.IsActive)
                return Ok(ApiResponse<object>.Ok(null!));

            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            existing.Name = request.Name ?? existing.Name;
        }
        else
        {
            _context.PlatformSubscribers.Add(new PlatformSubscriber
            {
                Email = email,
                Name = request.Name,
                Source = request.Source ?? "landing-page",
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("newsletter/unsubscribe")]
    public async Task<ContentResult> Unsubscribe([FromQuery] string email, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var subscriber = await _context.PlatformSubscribers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Email == email.Trim().ToLowerInvariant(), ct);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IsActive = false;
                subscriber.UnsubscribedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        return new ContentResult
        {
            ContentType = "text/html",
            Content = """
                <html><body style="font-family:sans-serif;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;background:#f5f5f5">
                <div style="text-align:center;padding:2rem;background:white;border-radius:12px;box-shadow:0 2px 8px rgba(0,0,0,.1)">
                <h2>Disiscrizione completata</h2>
                <p>Non riceverai piu le nostre email. Ci dispiace vederti andare!</p>
                </div></body></html>
                """
        };
    }
}

public record PublicSubscribeRequest(string Email, string? Name = null, string? Source = null);
