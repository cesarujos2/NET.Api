using MediatR;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.SelectAccount;

/// <summary>
/// Handler for SelectAccountCommand
/// </summary>
public class SelectAccountCommandHandler(
    IAuthService authService) : IRequestHandler<SelectAccountCommand, AuthResponseDto>
{
    /// <summary>
    /// Handle the select account command
    /// </summary>
    /// <param name="request">The select account command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with user data and tokens</returns>
    public async Task<AuthResponseDto> Handle(SelectAccountCommand request, CancellationToken cancellationToken)
    {
        var selectAccountRequest = new SelectAccountRequestDto
        {
            SelectionToken = request.SelectionToken,
            AccountId = request.AccountId
        };

        return await authService.SelectAccountAsync(selectAccountRequest);
    }
}