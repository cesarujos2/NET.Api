namespace NET.Api.Application.Common.Models.UserAccount;

public class ConfirmEmailChangeRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}