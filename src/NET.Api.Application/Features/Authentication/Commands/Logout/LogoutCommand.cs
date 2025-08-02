using NET.Api.Application.Abstractions.Messaging;

namespace NET.Api.Application.Features.Authentication.Commands.Logout;

public class LogoutCommand : ICommand<bool>
{
    public string UserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}