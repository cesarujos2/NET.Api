namespace NET.Api.Application.Common.Models.UserAccount;

public class UserAccountResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool RequiresEmailConfirmation { get; set; } = false;
}