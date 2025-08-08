using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Application.Configuration;
using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly GoogleAuthSettings _googleSettings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IOptions<GoogleAuthSettings> googleSettings,
        ILogger<GoogleAuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _googleSettings = googleSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> AuthenticateWithGoogleAsync(string googleIdToken)
    {
        try
        {
            // Validate Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleSettings.ClientId }
            });

            if (payload == null)
            {
                throw new InvalidOperationException("Token de Google inválido");
            }

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
                    IdentityDocument = string.Empty // Will be empty for Google users
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
                Roles = roles.ToList(),
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
}
