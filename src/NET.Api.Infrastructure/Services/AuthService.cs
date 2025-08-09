using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Application.Common.Models.UserAccount;
using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;
using AutoMapper;
using System.Security.Cryptography;

namespace NET.Api.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    IEmailService emailService,
    IGoogleAuthService googleAuthService,
    IUserAccountService userAccountService,
    IMapper mapper,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string baseUrl)
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El usuario ya existe con este correo electrónico.");
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false // Require email confirmation
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al crear el usuario: {errors}");
        }

        // Assign default "User" role
        var roleResult = await userManager.AddToRoleAsync(user, RoleConstants.Names.User);
        if (!roleResult.Succeeded)
        {
            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al asignar el rol por defecto: {roleErrors}");
        }

        // Create default user account
        try
        {
            await userAccountService.CreateDefaultAccountAsync(user.Id, "Principal");
            logger.LogInformation("Default account created for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create default account for user {UserId}", user.Id);
            // Don't throw here, user registration was successful
        }

        // Generate email confirmation token
        var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Send email confirmation
        await emailService.SendEmailConfirmationAsync(
            user.Email!,
            emailConfirmationToken,
            user.FullName,
            baseUrl
        );

        // Return response indicating email confirmation is required
        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            AccessToken = string.Empty, // No token until email is confirmed
            RefreshToken = string.Empty, // No token until email is confirmed
            ExpiresAt = DateTime.UtcNow,
            Roles = [RoleConstants.Names.User],
            RequiresEmailConfirmation = true
        };
    }

    public async Task<LoginWithAccountSelectionResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        // Check if email is confirmed
        if (!user.EmailConfirmed)
        {
            throw new UnauthorizedAccessException("Debes confirmar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada o solicita un nuevo enlace de confirmación.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new UnauthorizedAccessException("La cuenta está bloqueada temporalmente.");
            }
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        // Get user accounts
        var userAccounts = await userAccountService.GetAccountsForSelectionAsync(user.Id);
        
        // If user has no accounts, create a default one
        if (!userAccounts.Any())
        {
            try
            {
                await userAccountService.CreateDefaultAccountAsync(user.Id, "Principal");
                userAccounts = await userAccountService.GetAccountsForSelectionAsync(user.Id);
                logger.LogInformation("Default account created for existing user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create default account for existing user {UserId}", user.Id);
                throw new InvalidOperationException("Error al configurar la cuenta del usuario.");
            }
        }

        // If user has only one account, log them in directly
        if (userAccounts.Count == 1)
        {
            var singleAccount = userAccounts.First();
            await userAccountService.UpdateLastAccessedAsync(singleAccount.Id);
            
            var authResponse = await GenerateAuthResponseAsync(user, singleAccount.Id);
            
            return new LoginWithAccountSelectionResponseDto
            {
                RequiresAccountSelection = false,
                AuthResponse = authResponse,
                SelectedAccount = mapper.Map<UserAccountDto>(singleAccount)
            };
        }

        // User has multiple accounts, require selection
        var selectionToken = GenerateSelectionToken(user.Id);
        
        return new LoginWithAccountSelectionResponseDto
        {
            RequiresAccountSelection = true,
            AvailableAccounts = userAccounts,
            SelectionToken = selectionToken
        };
    }

    public async Task<AuthResponseDto> SelectAccountAsync(SelectAccountRequestDto request)
    {
        try
        {
            // Validate selection token
            var userId = ValidateSelectionToken(request.SelectionToken);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Token de selección inválido o expirado.");
            }

            // Verify user exists
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuario no encontrado.");
            }

            // Verify account belongs to user
            var userAccount = await userAccountService.GetUserAccountByIdAsync(request.AccountId);
            if (userAccount == null || userAccount.ApplicationUserId != userId)
            {
                throw new UnauthorizedAccessException("Cuenta no válida.");
            }

            // Update last accessed
            await userAccountService.UpdateLastAccessedAsync(request.AccountId);

            // Generate auth response
            return await GenerateAuthResponseAsync(user, request.AccountId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error selecting account {AccountId}", request.AccountId);
            throw;
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        // Validate refresh token
        var isValidRefreshToken = await jwtTokenService.ValidateRefreshTokenAsync(refreshToken);
        if (!isValidRefreshToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Get user ID from access token (even if expired)
        var userId = await jwtTokenService.GetUserIdFromTokenAsync(accessToken);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        // Get user
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Revoke old refresh token
        await jwtTokenService.RevokeRefreshTokenAsync(refreshToken);

        // Generate new tokens
        var newAccessToken = await jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
        var newRefreshToken = await jwtTokenService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // From configuration
            Roles = [.. roles]
        };
    }

    public async Task<AuthResponseDto> ConfirmEmailAsync(string email, string token)
    {
        logger.LogInformation("Attempting to confirm email for user: {Email}", email);

        // Find user by email
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogWarning("User not found with email: {Email}", email);
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        // Check if email is already confirmed
        if (user.EmailConfirmed)
        {
            logger.LogInformation("Email already confirmed for user: {Email}", email);
            throw new InvalidOperationException("El correo electrónico ya ha sido confirmado.");
        }

        // Confirm email with token
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to confirm email for user {Email}: {Errors}", email, errors);
            throw new InvalidOperationException($"Error al confirmar el correo electrónico: {errors}");
        }

        logger.LogInformation("Email confirmed successfully for user: {Email}", email);

        // Send welcome email
        try
        {
            await emailService.SendWelcomeEmailAsync(user.Email!, user.FullName);
            logger.LogInformation("Welcome email sent to user: {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome email to user: {Email}", email);
            // Don't throw here, email confirmation was successful
        }

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Generate JWT tokens
        var accessToken = await jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
        var refreshToken = await jwtTokenService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // From configuration
            Roles = [.. roles],
            RequiresEmailConfirmation = false
        };
    }

    public async Task<bool> ResendEmailConfirmationAsync(string email, string baseUrl)
    {
        logger.LogInformation("Attempting to resend email confirmation for user: {Email}", email);

        // Find user by email
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogWarning("User not found with email: {Email}", email);
            // Por seguridad, no revelamos si el usuario existe o no
            return true;
        }

        // Check if email is already confirmed
        if (user.EmailConfirmed)
        {
            logger.LogInformation("Email already confirmed for user: {Email}", email);
            throw new InvalidOperationException("El correo electrónico ya ha sido confirmado.");
        }

        try
        {
            // Generate new email confirmation token
            var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send email confirmation
            await emailService.SendEmailConfirmationAsync(
                user.Email!,
                emailConfirmationToken,
                user.FullName,
                baseUrl
            );

            logger.LogInformation("Email confirmation resent successfully for user: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resend email confirmation for user: {Email}", email);
            throw new InvalidOperationException("Error al reenviar el correo de confirmación. Inténtalo de nuevo más tarde.");
        }
    }

    public async Task<bool> ForgotPasswordAsync(string email, string baseUrl)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist for security reasons
                logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return true; // Return true to not reveal user existence
            }

            // Generate password reset token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            // Send password reset email
            await emailService.SendPasswordResetAsync(user.Email!, resetToken, user.FirstName, baseUrl);

            logger.LogInformation("Password reset email sent to user: {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing forgot password request for email: {Email}", email);
            throw new InvalidOperationException("An error occurred while processing the password reset request.");
        }
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                logger.LogWarning("Password reset attempted for non-existent email: {Email}", email);
                throw new InvalidOperationException("Invalid reset token or email.");
            }

            // Reset the password
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Password reset failed for user {Email}: {Errors}", user.Email, errors);
                throw new InvalidOperationException($"Password reset failed: {errors}");
            }

            // Get user roles
            var roles = await userManager.GetRolesAsync(user);

            // Generate new tokens
            var accessToken = await jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
            var refreshToken = await jwtTokenService.GenerateRefreshTokenAsync(user.Id);

            logger.LogInformation("Password reset successful for user: {Email}", user.Email);

            return new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // From configuration
                Roles = [.. roles],
                RequiresEmailConfirmation = false
            };
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            logger.LogError(ex, "Error occurred while resetting password for email: {Email}", email);
            throw new InvalidOperationException("An error occurred while resetting the password.");
        }
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        try
        {
            logger.LogInformation("Attempting to logout user: {UserId}", userId);

            // Revoke all refresh tokens for the user
            await jwtTokenService.RevokeAllRefreshTokensAsync(userId);

            logger.LogInformation("User logged out successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while logging out user: {UserId}", userId);
            throw new InvalidOperationException("An error occurred while logging out.");
        }
    }

    public async Task<LoginWithAccountSelectionResponseDto> GoogleLoginAsync(GoogleAuthRequestDto request)
    {
        var idToken = await googleAuthService.ExchangeCodeForIdTokenAsync(request.Code, request.RedirectUri);
        var authResponse = await googleAuthService.AuthenticateWithGoogleAsync(idToken);
        
        // Get user accounts after Google authentication
        var userAccounts = await userAccountService.GetAccountsForSelectionAsync(authResponse.Id);
        
        // If user has no accounts, create a default one
        if (!userAccounts.Any())
        {
            try
            {
                await userAccountService.CreateDefaultAccountAsync(authResponse.Id, "Principal");
                userAccounts = await userAccountService.GetAccountsForSelectionAsync(authResponse.Id);
                logger.LogInformation("Default account created for Google user {UserId}", authResponse.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create default account for Google user {UserId}", authResponse.Id);
                throw new InvalidOperationException("Error al configurar la cuenta del usuario.");
            }
        }

        // If user has only one account, return direct login
        if (userAccounts.Count == 1)
        {
            var singleAccount = userAccounts.First();
            await userAccountService.UpdateLastAccessedAsync(singleAccount.Id);
            
            // Update auth response with account info
            var userAccountDto = await userAccountService.GetUserAccountByIdAsync(singleAccount.Id);
            authResponse.SelectedAccount = userAccountDto;
            authResponse.HasMultipleAccounts = false;
            
            return new LoginWithAccountSelectionResponseDto
            {
                RequiresAccountSelection = false,
                AuthResponse = authResponse,
                SelectedAccount = userAccountDto
            };
        }

        // User has multiple accounts, require selection
        var selectionToken = GenerateSelectionToken(authResponse.Id);
        
        return new LoginWithAccountSelectionResponseDto
        {
            RequiresAccountSelection = true,
            AvailableAccounts = userAccounts,
            SelectionToken = selectionToken
        };
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user, string accountId)
    {
        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Generate JWT tokens
        var accessToken = await jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
        var refreshToken = await jwtTokenService.GenerateRefreshTokenAsync(user.Id);

        // Get selected account info
        var selectedAccount = await userAccountService.GetUserAccountByIdAsync(accountId);
        var userAccounts = await userAccountService.GetAccountsForSelectionAsync(user.Id);

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // From configuration
            Roles = [.. roles],
            SelectedAccount = selectedAccount,
            HasMultipleAccounts = userAccounts.Count > 1
        };
    }

    private string GenerateSelectionToken(string userId)
    {
        // Create a simple token with user ID and expiration (5 minutes)
        var tokenData = $"{userId}:{DateTime.UtcNow.AddMinutes(5):O}";
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }

    private string? ValidateSelectionToken(string token)
    {
        try
        {
            var tokenBytes = Convert.FromBase64String(token);
            var tokenData = System.Text.Encoding.UTF8.GetString(tokenBytes);
            var parts = tokenData.Split(':');
            
            if (parts.Length != 2)
                return null;

            var userId = parts[0];
            if (!DateTime.TryParse(parts[1], out var expiration))
                return null;

            if (DateTime.UtcNow > expiration)
                return null;

            return userId;
        }
        catch
        {
            return null;
        }
    }
}