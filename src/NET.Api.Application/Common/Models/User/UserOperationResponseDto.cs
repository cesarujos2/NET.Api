namespace NET.Api.Application.Common.Models.User;

public class UserOperationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool RequiresEmailConfirmation { get; set; } = false;
}