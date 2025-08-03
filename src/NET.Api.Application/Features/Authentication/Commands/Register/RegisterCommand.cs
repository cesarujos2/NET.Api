using NET.Api.Application.Abstractions.Messaging;
using NET.Api.Application.Common.Models.Authentication;

namespace NET.Api.Application.Features.Authentication.Commands.Register;

public class RegisterCommand : ICommand<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityDocument { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}