using Microsoft.AspNetCore.Identity;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Domain.Entities;

namespace NET.Api.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El usuario ya existe con este correo electrónico.");
        }

        // Check if identity document already exists
        var existingDocument = _userManager.Users.FirstOrDefault(u => u.IdentityDocument == request.IdentityDocument);
        if (existingDocument != null)
        {
            throw new InvalidOperationException("Ya existe un usuario con este documento de identidad.");
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IdentityDocument = request.IdentityDocument,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true // For simplicity, auto-confirm email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al crear el usuario: {errors}");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        
        // Generate JWT tokens
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id);

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
            Roles = [.. roles]
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new UnauthorizedAccessException("La cuenta está bloqueada temporalmente.");
            }
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        
        // Generate JWT tokens
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id);

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
            Roles = [.. roles]
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        // Validate refresh token
        var isValidRefreshToken = await _jwtTokenService.ValidateRefreshTokenAsync(refreshToken);
        if (!isValidRefreshToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Get user ID from access token (even if expired)
        var userId = _jwtTokenService.GetUserIdFromToken(accessToken);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        // Get user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Revoke old refresh token
        await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken);

        // Generate new tokens
        var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user.Id, user.Email!, roles.ToList());
        var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user.Id);

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
}