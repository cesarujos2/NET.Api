using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        return await _authService.RegisterAsync(registerRequest, request.BaseUrl);
    }
}