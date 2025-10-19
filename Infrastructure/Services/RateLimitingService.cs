using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(string key, int maxAttempts = 5, TimeSpan? window = null);
    Task RecordAttemptAsync(string key, TimeSpan? window = null);
    Task<int> GetAttemptCountAsync(string key);
}

public class RateLimitingService(IMemoryCache _cache) : IRateLimitingService
{
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(15);

    public async Task<bool> IsAllowedAsync(string key, int maxAttempts = 5, TimeSpan? window = null)
    {
        var windowPeriod = window ?? DefaultWindow;
        var cacheKey = $"ratelimit_{key}";
        
        var attempts = await GetAttemptCountAsync(key);
        var isAllowed = attempts < maxAttempts;

        return isAllowed;
    }

    public async Task RecordAttemptAsync(string key, TimeSpan? window = null)
    {
        var windowPeriod = window ?? DefaultWindow;
        var cacheKey = $"ratelimit_{key}";
        
        var currentAttempts = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = windowPeriod;
            return 0;
        });
        
        _cache.Set(cacheKey, currentAttempts + 1, windowPeriod);
        
        await Task.CompletedTask;
    }

    public async Task<int> GetAttemptCountAsync(string key)
    {
        var cacheKey = $"ratelimit_{key}";
        var attempts = _cache.Get<int>(cacheKey);
        return await Task.FromResult(attempts);
    }
}