using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.User;

namespace NET.Api.Application.Features.UserAccount.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeCommand : ICommand<UserOperationResponseDto>
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}