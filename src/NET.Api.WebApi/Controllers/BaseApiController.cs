using MediatR;
using Microsoft.AspNetCore.Mvc;
using NET.Api.Shared.Models;

namespace NET.Api.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult<ApiResponse<T>> HandleResult<T>(T result)
    {
        if (result == null)
            return NotFound(ApiResponse<T>.ErrorResponse("Resource not found"));

        return Ok(ApiResponse<T>.SuccessResponse(result));
    }

    protected ActionResult<ApiResponse<T>> HandleError<T>(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<T>.ErrorResponse(message));
    }
}
