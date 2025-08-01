using Microsoft.AspNetCore.Mvc;
using NET.Api.Shared.Models;

namespace NET.Api.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : BaseApiController
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var healthInfo = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return HandleResult((object)healthInfo);
    }
}
