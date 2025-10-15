using Microsoft.EntityFrameworkCore;
using SwipeSwap.Application;
using SwipeSwap.Infrastructure;
using SwipeSwap.Infrastructure.Context;
using SwipeSwap.WebApi.Middleware;

namespace SwipeSwap.WebApi;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    
    [Obsolete("Obsolete")]
    public void ConfigureServices(IServiceCollection services)
    {
        var currentAssembly = typeof(Startup).Assembly;
        services
            .AddMediatR(c => c.RegisterServicesFromAssembly(currentAssembly));
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddSwaggerGen();
        services.AddInfrastructure(_configuration);

        services.ConfigJwt(_configuration);
        services.AddAuthorization();
        services.AddApplication();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<LoggingMiddleware>();
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