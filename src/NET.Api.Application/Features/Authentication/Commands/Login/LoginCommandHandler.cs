using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginWithAccountSelectionResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginWithAccountSelectionResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password
        };

        return await _authService.LoginAsync(loginRequest);
    }
}