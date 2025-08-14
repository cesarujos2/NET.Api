using Microsoft.AspNetCore.Identity;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Services;
using NET.Api.Shared.Models;
using System.Security.Claims;
using System.Text.Json;

namespace NET.Api.WebApi.Middleware;

public class ProfileCompletionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProfileCompletionMiddleware> _logger;
    private readonly HashSet<string> _excludedPaths;

    public ProfileCompletionMiddleware(RequestDelegate next, ILogger<ProfileCompletionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/api/Authentication",
            "/api/UserAccount",
            "/api/Health",
            "/swagger",
            "/favicon.ico"
        };
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, IProfileCompletionService profileCompletionService)
    {
        // Skip middleware for excluded paths
        if (ShouldSkipMiddleware(context))
        {
            await _next(context);
            return;
        }

        // Skip if user is not authenticated
        if (!context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        try
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _next(context);
                return;
            }

            // Check if profile is complete
            if (!profileCompletionService.IsProfileComplete(user))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a {Path} sin completar su perfil", userId, context.Request.Path);
                
                var missingFields = profileCompletionService.GetMissingRequiredFields(user);
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Debe completar su perfil antes de continuar.",
                    Data = new
                    {
                        RequiredAction = "COMPLETE_PROFILE",
                        MissingFields = missingFields,
                        RedirectUrl = "/api/useraccount/profile/status"
                    }
                };

                context.Response.StatusCode = 403; // Forbidden
                context.Response.ContentType = "application/json";
                
                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                await context.Response.WriteAsync(jsonResponse);
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ProfileCompletionMiddleware para el usuario {UserId}", 
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _next(context);
        }
    }

    private bool ShouldSkipMiddleware(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        
        // Skip for excluded paths
        if (_excludedPaths.Any(excludedPath => path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Skip for non-API requests
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip for OPTIONS requests (CORS preflight)
        if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}