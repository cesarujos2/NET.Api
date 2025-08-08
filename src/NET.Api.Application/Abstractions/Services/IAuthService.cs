using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Abstractions.Services;

public interface IAuthService: IApplicationService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string baseUrl);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<AuthResponseDto> ConfirmEmailAsync(string email, string token);
    Task<bool> ResendEmailConfirmationAsync(string email, string baseUrl);
    Task<bool> ForgotPasswordAsync(string email, string baseUrl);
    Task<AuthResponseDto> ResetPasswordAsync(string email, string token, string newPassword);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthRequestDto request);
    Task<bool> LogoutAsync(string userId);
}