using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NET.Api.Application.Abstractions.Services;
using NET.Api.Application.Configuration;
using NET.Api.Domain.Entities;
using NET.Api.Domain.Interfaces;
using NET.Api.Domain.ValueObjects;
using NET.Api.Infrastructure.Persistence;
using NET.Api.Infrastructure.Repositories;
using NET.Api.Infrastructure.Services;
using System.Text;

namespace NET.Api.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework with Mysql
        var conectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(conectionString, ServerVersion.AutoDetect(conectionString)));
        
        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            
            // User settings
            options.User.RequireUniqueEmail = true;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        // Add JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "MySecretKeyForJWTTokenGeneration123456789";
        var issuer = jwtSettings["Issuer"] ?? "NET.Api";
        var audience = jwtSettings["Audience"] ?? "NET.Api.Users";
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
        
        // Configure JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        // Configure SMTP Settings
        services.Configure<SmtpConfiguration>(configuration.GetSection("Smtp"));
        
        // Register repositories
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        
        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
}
