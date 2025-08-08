using MediatR;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleAuthUrl;

/// <summary>
/// Handler for GoogleAuthUrlCommand
/// </summary>
public class GoogleAuthUrlCommandHandler : ICommandHandler<GoogleAuthUrlCommand, GoogleAuthUrlResponseDto>
{
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleAuthUrlCommandHandler(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    public Task<GoogleAuthUrlResponseDto> Handle(GoogleAuthUrlCommand request, CancellationToken cancellationToken)
    {
        var state = request.State ?? Guid.NewGuid().ToString();
        var authUrl = _googleAuthService.GetGoogleAuthUrl(request.RedirectUri, state);
        
        return Task.FromResult(new GoogleAuthUrlResponseDto
        {
            AuthUrl = authUrl,
            State = state
        });
    }
}
