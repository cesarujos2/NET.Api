namespace NET.Api.Application.Common.Models.User;

public class ChangeUserEmailRequestDto
{
    public string NewEmail { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}