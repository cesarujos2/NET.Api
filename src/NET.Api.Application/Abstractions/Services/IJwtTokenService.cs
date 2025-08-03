using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Domain.Entities;

namespace NET.Api.Application.Abstractions.Services;

public interface IJwtTokenService: IApplicationService
{
    Task<string> GenerateAccessTokenAsync(string userId, string email, List<string> roles);
    Task<string> GenerateRefreshTokenAsync(string userId);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    Task SaveRefreshTokenAsync(string userId, string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllRefreshTokensAsync(string userId);
    Task<string> GetUserIdFromTokenAsync(string token);
}