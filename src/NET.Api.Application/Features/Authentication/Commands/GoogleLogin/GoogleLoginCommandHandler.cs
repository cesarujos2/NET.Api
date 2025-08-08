using MediatR;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public GoogleLoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var googleAuthRequest = new GoogleAuthRequestDto
        {
            GoogleIdToken = request.GoogleIdToken
        };

        return await _authService.GoogleLoginAsync(googleAuthRequest);
    }
}
