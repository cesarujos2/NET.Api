using NET.Api.Application.Abstractions.Messaging;

namespace NET.Api.Application.Features.Authentication.Commands.ResendEmailConfirmation;

public class ResendEmailConfirmationCommand : ICommand<bool>
{
    public string Email { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}