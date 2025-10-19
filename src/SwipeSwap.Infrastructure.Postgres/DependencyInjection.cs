using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwipeSwap.Infrastructure.Postgres.Context;
using SwipeSwap.Infrastructure.Postgres.Repositories;
using SwipeSwap.Infrastructure.Postgres.Repositories.Implementations;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;
using SwipeSwap.Infrastructure.Repositories.Implementations;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IExchangeRepository, ExchangeRepository>();
        return services;
    }
}