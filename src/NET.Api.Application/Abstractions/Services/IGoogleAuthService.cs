using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Abstractions.Services;

public interface IGoogleAuthService : IApplicationService
{
    Task<AuthResponseDto> AuthenticateWithGoogleAsync(string googleIdToken);
}
