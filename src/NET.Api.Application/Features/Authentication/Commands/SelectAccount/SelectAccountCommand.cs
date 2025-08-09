using MediatR;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.SelectAccount;

/// <summary>
/// Command for selecting a user account during login
/// </summary>
public class SelectAccountCommand : IRequest<AuthResponseDto>
{
    /// <summary>
    /// Selection token from login response
    /// </summary>
    public required string SelectionToken { get; set; }

    /// <summary>
    /// ID of the account to select
    /// </summary>
    public required string AccountId { get; set; }
}