using MediatR;

namespace NET.Api.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}