using AiMarketingAgency.Application.Common.Interfaces;
using AiMarketingAgency.Application.SocialConnectors.Dtos;
using AiMarketingAgency.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiMarketingAgency.Application.SocialConnectors.Commands.ConnectPlatform;

public class ConnectPlatformCommandHandler : IRequestHandler<ConnectPlatformCommand, SocialConnectorDto>
{
    private readonly IAppDbContext _context;

    public ConnectPlatformCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SocialConnectorDto> Handle(ConnectPlatformCommand request, CancellationToken cancellationToken)
    {
        // Check if connector already exists for this agency+platform, update it
        var existing = await _context.SocialConnectors
            .FirstOrDefaultAsync(c => c.AgencyId == request.AgencyId && c.Platform == request.Platform, cancellationToken);

        if (existing != null)
        {
            existing.AccessToken = request.AccessToken;
            existing.RefreshToken = request.RefreshToken;
            existing.AccountId = request.AccountId;
            existing.AccountName = request.AccountName;
            existing.ProfileImageUrl = request.ProfileImageUrl;
            existing.TokenExpiresAt = request.TokenExpiresAt;
            existing.IsActive = true;
        }
        else
        {
            existing = new SocialConnector
            {
                AgencyId = request.AgencyId,
                Platform = request.Platform,
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken,
                AccountId = request.AccountId,
                AccountName = request.AccountName,
                ProfileImageUrl = request.ProfileImageUrl,
                TokenExpiresAt = request.TokenExpiresAt,
                IsActive = true
            };
            _context.SocialConnectors.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SocialConnectorDto
        {
            Id = existing.Id,
            Platform = existing.Platform,
            AccountId = existing.AccountId,
            AccountName = existing.AccountName,
            ProfileImageUrl = existing.ProfileImageUrl,
            IsActive = existing.IsActive,
            TokenExpiresAt = existing.TokenExpiresAt,
            CreatedAt = existing.CreatedAt
        };
    }
}
