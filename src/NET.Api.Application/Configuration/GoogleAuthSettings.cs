namespace NET.Api.Application.Configuration;

public class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";
    
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
