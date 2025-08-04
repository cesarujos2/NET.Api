namespace NET.Api.Application.Common.Models.UserAccount;

public class ChangeEmailRequestDto
{
    public string NewEmail { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}