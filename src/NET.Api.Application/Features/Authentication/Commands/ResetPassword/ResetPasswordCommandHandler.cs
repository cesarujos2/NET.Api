using MediatR;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
    }
}