using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Features.UserAccount.Commands.ChangePassword;

public class ChangePasswordCommand : ICommand<UserAccountResponseDto>
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}