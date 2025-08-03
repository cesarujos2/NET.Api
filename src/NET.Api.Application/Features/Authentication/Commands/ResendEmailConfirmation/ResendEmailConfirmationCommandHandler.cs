using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Abstractions.Services;

namespace NET.Api.Application.Features.Authentication.Commands.ResendEmailConfirmation;

public class ResendEmailConfirmationCommandHandler : ICommandHandler<ResendEmailConfirmationCommand, bool>
{
    private readonly IAuthService _authService;

    public ResendEmailConfirmationCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ResendEmailConfirmationAsync(request.Email, request.BaseUrl);
    }
}