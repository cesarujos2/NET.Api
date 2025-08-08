using MediatR;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleLogin;

public class GoogleLoginCommand : IRequest<AuthResponseDto>
{
    public string GoogleIdToken { get; set; } = string.Empty;
}
