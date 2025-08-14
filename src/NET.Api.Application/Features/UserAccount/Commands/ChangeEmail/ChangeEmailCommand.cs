using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.User;

namespace NET.Api.Application.Features.UserAccount.Commands.ChangeEmail;

public class ChangeEmailCommand : ICommand<UserOperationResponseDto>
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}