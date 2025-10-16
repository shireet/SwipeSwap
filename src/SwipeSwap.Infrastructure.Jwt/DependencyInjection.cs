using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SwipeSwap.Infrastructure.Jwt.Auth.Implementations;
using SwipeSwap.Infrastructure.Jwt.Auth.Interfaces;

namespace SwipeSwap.Infrastructure.Jwt;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.ConfigJwt(configuration);
        return services;
    }

    private static IServiceCollection ConfigJwt(this IServiceCollection services, IConfiguration configuration)
    {
        
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        return services;
    }
}