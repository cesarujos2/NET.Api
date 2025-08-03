using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.ConfirmEmail;

public class ConfirmEmailCommand : ICommand<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}