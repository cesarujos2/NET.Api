using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NET.Api.Application.Features.Authentication.Commands.Login;
using NET.Api.Application.Features.Authentication.Commands.Register;
using NET.Api.Application.Features.Authentication.Commands.RefreshToken;
using NET.Api.Application.Features.Authentication.Commands.Logout;
using NET.Api.Application.Features.Authentication.Commands.ConfirmEmail;
using NET.Api.Application.Features.Authentication.Commands.ResendEmailConfirmation;
using NET.Api.Application.Features.Authentication.Commands.ForgotPassword;
using NET.Api.Application.Features.Authentication.Commands.ResetPassword;
using NET.Api.Application.Features.Authentication.Commands.GoogleLogin;
using System.Security.Claims;
using NET.Api.Application.Common.Models.Authentication;
using NET.Api.Application.Features.Authentication.Commands.GoogleAuthUrl;

namespace NET.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <returns>Authentication response with user data and token</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestWithBaseUrlDto request)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IdentityDocument = request.IdentityDocument,
                PhoneNumber = request.PhoneNumber,
                BaseUrl = request.BaseUrl
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Usuario registrado exitosamente.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Confirm user email with token
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="token">Email confirmation token</param>
    /// <returns>Authentication response with user data and tokens</returns>
    [HttpGet("confirm-email")]
    public async Task<ActionResult<AuthResponseDto>> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            var command = new ConfirmEmailCommand
            {
                Email = email,
                Token = token
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Correo electrónico confirmado exitosamente.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    /// <param name="request">Email and base URL to resend confirmation</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("resend-email-confirmation")]
    public async Task<ActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationCommand request)
    {
        try
        {
            var result = await mediator.Send(request);
            return Ok(new { success = true, message = "Si el correo electrónico existe y no está confirmado, se ha reenviado el enlace de confirmación." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    /// <param name="request">User email and base URL</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand request)
    {
        try
        {
            var result = await mediator.Send(request);
            return Ok(new { success = true, message = "Si el correo electrónico existe, se ha enviado un enlace de restablecimiento de contraseña." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Reset password data</param>
    /// <returns>Authentication response with user data and tokens</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<AuthResponseDto>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var command = new ResetPasswordCommand
            {
                Email = request.Email,
                Token = request.Token,
                NewPassword = request.NewPassword,
                ConfirmPassword = request.ConfirmPassword
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Contraseña restablecida exitosamente.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with user data and token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Inicio de sesión exitoso.", data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication response with refreshed tokens</returns>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var command = new RefreshTokenCommand
            {
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Token renovado exitosamente.", data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Logout (requires authentication)
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Usuario no autenticado." });
            }

            var command = new LogoutCommand
            {
                UserId = userId,
                AccessToken = accessToken
            };

            var result = await mediator.Send(command);
            
            if (result)
            {
                return Ok(new { success = true, message = "Sesión cerrada exitosamente." });
            }
            
            return BadRequest(new { success = false, message = "Error al cerrar sesión." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Login with Google authentication
    /// </summary>
    /// <param name="request">Google ID token</param>
    /// <returns>Authentication response with user data and token</returns>
    [HttpPost("google/login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleAuthRequestDto request)
    {
        try
        {
            var command = new GoogleLoginCommand
            {
                Code = request.Code,
                RedirectUri = request.RedirectUri
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "Autenticación con Google exitosa.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Google authentication URL
    /// </summary>
    /// <param name="redirectUri">Redirect URI</param>
    /// <param name="state">State parameter</param>
    /// <returns>Google authentication URL</returns>
    [HttpGet("google/auth-url")]
    public async Task<ActionResult<GoogleAuthUrlResponseDto>> GetGoogleAuthUrl([FromQuery] string redirectUri, [FromQuery] string? state = null)
    {
        try{
            var command = new GoogleAuthUrlCommand
            {
                RedirectUri = redirectUri,
                State = state
            };

            var result = await mediator.Send(command);
            return Ok(new { success = true, message = "URL de autenticación con Google obtenida exitosamente.", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
        }
    }
}