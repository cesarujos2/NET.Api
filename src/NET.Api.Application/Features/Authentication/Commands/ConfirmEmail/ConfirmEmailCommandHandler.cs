using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public ConfirmEmailCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ConfirmEmailAsync(request.Email, request.Token);
    }
}