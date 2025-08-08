using MediatR;
using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;
using System.ComponentModel.DataAnnotations;

namespace NET.Api.Application.Features.Authentication.Commands.GoogleLogin;

public class GoogleLoginCommand : ICommand<AuthResponseDto>
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
