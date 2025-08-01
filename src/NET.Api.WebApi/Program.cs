using NET.Api.WebApi.Configuration;
using NET.Api.WebApi.Middleware;
using NET.Api.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services using dependency injection configuration
builder.Services.AddWebApiServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Add CORS
app.UseCors(ApiConstants.Policies.DefaultCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
