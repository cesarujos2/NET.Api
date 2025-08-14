namespace NET.Api.Application.Common.Models.User;

public class ConfirmUserEmailChangeRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}