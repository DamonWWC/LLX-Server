using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using System.Collections.Concurrent;

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
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        
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
            _database = redis.GetDatabase(); // 确保 _database 被初始化
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

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiry = null)
    {
        if (!_isConnected)
        {
            return await factory();
        }

        try
        {
            // 先尝试从缓存获取
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // 使用信号量防止缓存击穿
            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                // 双重检查，防止并发时重复设置
                cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                {
                    return cachedValue;
                }

                // 从工厂方法获取数据
                var value = await factory();
                if (value != null)
                {
                    await SetAsync(key, value, expiry);
                }
                else
                {
                    // 防止缓存穿透：缓存空值，但设置较短的过期时间
                    await SetAsync(key, string.Empty, TimeSpan.FromMinutes(5));
                }

                return value;
            }
            finally
            {
                semaphore.Release();
                _semaphores.TryRemove(key, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get or set cache for key: {Key}. Falling back to factory method.", key);
            return await factory();
        }
    }

    public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T?>();
        
        if (!_isConnected)
        {
            return result;
        }

        try
        {
            var keyArray = keys.ToArray();
            var values = await _database.StringGetAsync(keyArray.Select(k => (RedisKey)k).ToArray());
            
            for (int i = 0; i < keyArray.Length; i++)
            {
                var key = keyArray[i];
                var value = values[i];
                
                if (!value.IsNullOrEmpty)
                {
                    try
                    {
                        result[key] = JsonSerializer.Deserialize<T>(value!);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize cached value for key: {Key}", key);
                        result[key] = default;
                    }
                }
                else
                {
                    result[key] = default;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get multiple values from Redis cache");
        }

        return result;
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null)
    {
        if (!_isConnected || !items.Any())
        {
            return;
        }

        try
        {
            var keyValuePairs = items.Select(kvp => 
                new KeyValuePair<RedisKey, RedisValue>(kvp.Key, JsonSerializer.Serialize(kvp.Value)))
                .ToArray();

            await _database.StringSetAsync(keyValuePairs);
            
            // 设置过期时间
            if (expiry.HasValue)
            {
                foreach (var key in items.Keys)
                {
                    await _database.KeyExpireAsync(key, expiry.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set multiple values in Redis cache");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (!_isConnected)
        {
            return false;
        }

        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if key exists in Redis cache: {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
    {
        if (!_isConnected)
        {
            return false;
        }

        try
        {
            return await _database.KeyExpireAsync(key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set expiry for key in Redis cache: {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        if (!_isConnected)
        {
            return null;
        }

        try
        {
            var ttl = await _database.KeyTimeToLiveAsync(key);
            return ttl;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get TTL for key in Redis cache: {Key}", key);
            return null;
        }
    }
}
