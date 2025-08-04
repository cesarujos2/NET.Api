using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.UserAccount;

namespace NET.Api.Application.Features.UserAccount.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeCommand : ICommand<UserAccountResponseDto>
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}