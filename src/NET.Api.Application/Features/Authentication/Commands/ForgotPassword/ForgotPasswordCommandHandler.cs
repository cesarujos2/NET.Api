using MediatR;
using NET.Api.Application.Abstractions.Services;

namespace NET.Api.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ForgotPasswordAsync(request.Email, request.BaseUrl);
    }
}