namespace SwipeSwap.Infrastructure.Redis.Implementations;

public interface IRedisClient
{
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}