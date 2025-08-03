using MediatR;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}