using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Abstractions.Services;

public interface IGoogleAuthService : IApplicationService
{
    Task<AuthResponseDto> AuthenticateWithGoogleAsync(string googleIdToken);
    string GetGoogleAuthUrl(string redirectUri, string? state = null);
    
    /// <summary>
    /// Exchanges an authorization code for an ID token using Google's token endpoint
    /// </summary>
    /// <param name="code">The authorization code received from Google</param>
    /// <param name="redirectUri">The redirect URI used in the authorization request</param>
    /// <returns>The ID token as a string</returns>
    Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri);
}
