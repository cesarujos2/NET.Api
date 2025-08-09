using MediatR;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;
using Microsoft.Extensions.Caching.Memory;
using NET.Api.Domain.Exceptions;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleLogin;

public class GoogleLoginCommandHandler(IAuthService authService, IMemoryCache memoryCache) : ICommandHandler<GoogleLoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // Validate CSRF state parameter
        if (string.IsNullOrEmpty(request.State))
        {
            throw new UnauthorizedAccessException("Estado CSRF requerido para la autenticación con Google.");
        }

        var cacheKey = $"google_auth_state_{request.State}";
        if (!memoryCache.TryGetValue(cacheKey, out _))
        {
            throw new UnauthorizedAccessException("Estado CSRF inválido o expirado.");
        }

        // Remove state from cache to prevent reuse
        memoryCache.Remove(cacheKey);

        var googleAuthRequest = new GoogleAuthRequestDto
        {
            Code = request.Code,
            RedirectUri = request.RedirectUri,
        };

        return await authService.GoogleLoginAsync(googleAuthRequest);
    }
}
