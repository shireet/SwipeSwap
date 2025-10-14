using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SwipeSwap.Infrastructure;
using SwipeSwap.Infrastructure.Context;

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
        services.AddControllers()
            .AddFluentValidation(fv => 
            {
                fv.AutomaticValidationEnabled = true;
            });
        services.AddSwaggerGen();
        
        
        services.AddMvc(
            x =>
            {
                //x.Filters.Add(typeof(BusinessExceptionFilter));
            });

        services.AddSwaggerGen();
        services.AddInfrastructure(_configuration);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
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