using MediatR;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Shared.Models;
using System.Security.Claims;

namespace NET.Api.WebApi.Controllers;

/// <summary>
/// Controlador base con funcionalidades comunes
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Instancia de MediatR para envío de comandos y queries
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// ID del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Email del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Nombre del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserName => User.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Verifica si el usuario está autenticado
    /// </summary>
    protected bool IsAuthenticated => User.Identity?.IsAuthenticated == true;
}
