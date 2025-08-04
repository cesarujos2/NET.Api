using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Features.UserAccount.Commands.ChangeEmail;

public class ChangeEmailCommand : ICommand<UserAccountResponseDto>
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}