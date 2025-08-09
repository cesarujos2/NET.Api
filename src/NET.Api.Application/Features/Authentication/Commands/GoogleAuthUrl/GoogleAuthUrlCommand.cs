using MediatR;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleAuthUrl;

public class GoogleAuthUrlCommand : ICommand<GoogleAuthUrlResponseDto>
{
    public string RedirectUri { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}