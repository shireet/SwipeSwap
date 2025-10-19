using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application;
using SwipeSwap.Infrastructure.Postgres;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Jwt;
using SwipeSwap.Infrastructure.Redis;
using SwipeSwap.WebApi.Filters;
using SwipeSwap.WebApi.Middleware;

namespace SwipeSwap.WebApi;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        var currentAssembly = typeof(Startup).Assembly;
        services
            .AddMediatR(c => c.RegisterServicesFromAssembly(currentAssembly));
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
        services.AddValidatorsFromAssembly(currentAssembly);

        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableDataAnnotationsValidation = true;
        });
        services.ConfigureSwagger();
        
        services.AddInfrastructurePostgres(configuration);
        services.AddInfrastructureJwt(configuration);
        services.AddInfrastructureRedis(configuration);
        services.AddAuthorization();
        services.AddApplication();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
            });
        
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
    }
}