namespace NET.Api.Application.Common.Models.User;

public class UserStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public bool IsProfileComplete { get; set; }
    public List<string> MissingRequiredFields { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}