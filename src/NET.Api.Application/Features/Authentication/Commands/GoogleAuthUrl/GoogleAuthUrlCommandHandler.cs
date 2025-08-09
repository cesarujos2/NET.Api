using MediatR;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleAuthUrl;

/// <summary>
/// Handler for GoogleAuthUrlCommand
/// </summary>
public class GoogleAuthUrlCommandHandler(IGoogleAuthService googleAuthService, IMemoryCache memoryCache) : ICommandHandler<GoogleAuthUrlCommand, GoogleAuthUrlResponseDto>
{
    public Task<GoogleAuthUrlResponseDto> Handle(GoogleAuthUrlCommand request, CancellationToken cancellationToken)
    {
        var state = request.State;
        var authUrl = googleAuthService.GetGoogleAuthUrl(request.RedirectUri, state);
        
        // Store state in cache for CSRF validation with 10 minutes expiration
        var cacheKey = $"google_auth_state_{state}";
        memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(10));
        
        return Task.FromResult(new GoogleAuthUrlResponseDto
        {
            AuthUrl = authUrl,
            State = state
        });
    }
}
