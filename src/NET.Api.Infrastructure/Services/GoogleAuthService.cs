using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Application.Configuration;
using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;
using System.Text.Json;

namespace NET.Api.Infrastructure.Services;

public class GoogleAuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    IOptions<GoogleAuthSettings> googleSettings,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly GoogleAuthSettings _googleSettings = googleSettings.Value;
    private readonly ILogger<GoogleAuthService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<AuthResponseDto> AuthenticateWithGoogleAsync(string googleIdToken)
    {
        try
        {
            // Validate Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_googleSettings.ClientId]
            }) ?? throw new InvalidOperationException("Token de Google inválido");

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(payload.Email);

            ApplicationUser user;


            if (existingUser != null)
            {
                user = existingUser;

                // Update user information if needed
                if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName))
                {
                    user.FirstName = payload.GivenName ?? "Usuario";
                    user.LastName = payload.FamilyName ?? "Google";
                    await _userManager.UpdateAsync(user);
                }
            }
            else
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "Usuario",
                    LastName = payload.FamilyName ?? "Google",
                    EmailConfirmed = true, // Google already verified the email
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Error al crear usuario de Google: {Errors}", errors);
                    throw new InvalidOperationException($"Error al crear el usuario: {errors}");
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, RoleConstants.Names.User);
            }

            // Generate JWT tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id);

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("Usuario autenticado con Google exitosamente: {Email}", user.Email);

            return new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30), // Default 30 minutes
                Roles = [.. roles],
                RequiresEmailConfirmation = false // Google users have confirmed email
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogError(ex, "Token de Google inválido");
            throw new InvalidOperationException("Token de Google inválido o expirado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la autenticación con Google");
            throw new InvalidOperationException("Error al autenticar con Google. Por favor, intente nuevamente.");
        }

    }


    /// <summary>
    /// Obtiene la URL de autenticación de Google OAuth 2.0
    /// </summary>
    /// <param name="redirectUri">URI de redirección después de la autenticación</param>
    /// <param name="state">Estado opcional para prevenir CSRF</param>
    /// <returns>URL completa de autenticación de Google</returns>
    public string GetGoogleAuthUrl(string redirectUri, string? state = null)
    {
        if (string.IsNullOrWhiteSpace(_googleSettings.ClientId))
        {
            throw new InvalidOperationException("Google Client ID no está configurado");
        }

        var baseUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        var scope = Uri.EscapeDataString("openid email profile");
        var clientId = Uri.EscapeDataString(_googleSettings.ClientId);
        var redirectUriEncoded = Uri.EscapeDataString(redirectUri);
        var responseType = "code";
        var stateParam = !string.IsNullOrWhiteSpace(state) ? $"&state={Uri.EscapeDataString(state)}" : string.Empty;

        return $"{baseUrl}?client_id={clientId}&redirect_uri={redirectUriEncoded}&response_type={responseType}&scope={scope}{stateParam}&access_type=offline&prompt=consent";
    }
    
    /// <summary>
    /// Exchanges an authorization code for an ID token using Google's token endpoint
    /// </summary>
    /// <param name="code">The authorization code received from Google</param>
    /// <param name="redirectUri">The redirect URI used in the authorization request</param>
    /// <returns>The ID token as a string</returns>
    public async Task<string> ExchangeCodeForIdTokenAsync(string code, string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(_googleSettings.ClientId) || string.IsNullOrWhiteSpace(_googleSettings.ClientSecret))
        {
            throw new InvalidOperationException("La configuración de Google OAuth no está completa. Verifique ClientId y ClientSecret.");
        }

        var httpClient = _httpClientFactory.CreateClient();
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _googleSettings.ClientId },
            { "client_secret", _googleSettings.ClientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        var content = new FormUrlEncodedContent(tokenRequest);
        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error al intercambiar código por token: {Error}", errorContent);
            throw new InvalidOperationException("Error al intercambiar el código de autorización por un token de Google");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        if (!tokenResponse.TryGetProperty("id_token", out var idTokenElement) || idTokenElement.ValueKind != JsonValueKind.String)
        {
            _logger.LogError("La respuesta de Google no contiene un ID token válido: {Response}", responseContent);
            throw new InvalidOperationException("La respuesta de Google no contiene un ID token válido");
        }

        return idTokenElement.GetString() ?? 
            throw new InvalidOperationException("No se pudo obtener el ID token de la respuesta de Google");
    }
}
