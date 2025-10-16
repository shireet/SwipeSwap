using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using SwipeSwap.Infrastructure.Redis.Implementations;
using SwipeSwap.Infrastructure.Redis.Interfaces;

namespace SwipeSwap.Infrastructure.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisConnection!)
        );
        services.AddScoped<IRedisClient, RedisClient>();
        return services;
    }
}