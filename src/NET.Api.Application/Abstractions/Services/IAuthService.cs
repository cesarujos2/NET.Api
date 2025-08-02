using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Abstractions.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
}