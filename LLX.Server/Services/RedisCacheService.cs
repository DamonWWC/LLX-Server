using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace LLX.Server.Services;

/// <summary>
/// Redis 缓存服务实现
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IServer? _server;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly bool _isConnected;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        try
        {
            _database = redis.GetDatabase();
            var endpoints = redis.GetEndPoints();
            if (endpoints.Any())
            {
                _server = redis.GetServer(endpoints.First());
            }
            _isConnected = redis.IsConnected;
            
            if (!_isConnected)
            {
                _logger.LogWarning("Redis is not connected. Cache operations will be skipped.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Redis connection. Cache will be disabled.");
            _isConnected = false;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (!_isConnected)
        {
            return default;
        }

        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get value from Redis cache for key: {Key}. Returning default.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (!_isConnected)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry ?? TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set value in Redis cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (!_isConnected)
        {
            return;
        }

        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove value from Redis cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        if (!_isConnected || _server == null)
        {
            return;
        }

        try
        {
            var keys = _server.Keys(pattern: pattern).ToArray();
            if (keys.Any())
                await _database.KeyDeleteAsync(keys);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove values by pattern from Redis cache: {Pattern}", pattern);
        }
    }
}
