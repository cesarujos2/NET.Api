namespace NET.Api.Domain.ValueObjects;

public class SmtpConfiguration
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public bool EnableSsl { get; init; }
    
    public SmtpConfiguration() { }
    
    public SmtpConfiguration(
        string host,
        int port,
        string username,
        string password,
        string from,
        bool enableSsl)
    {
        Host = host;
        Port = port;
        Username = username;
        Password = password;
        From = from;
        EnableSsl = enableSsl;
    }
}