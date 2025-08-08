namespace NET.Api.Application.Common.Models.Authentication;

public class GoogleAuthUrlResponseDto
{
    public string AuthUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
