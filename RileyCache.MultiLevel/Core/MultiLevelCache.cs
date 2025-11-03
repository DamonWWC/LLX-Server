using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RileyCache.MultiLevel.Logging;
using RileyCache.MultiLevel.Options;
using RileyCache.MultiLevel.Strategies;

namespace RileyCache.MultiLevel.Core;

/// <summary>
/// 双层缓存实现：内存缓存（L1）+ 分布式缓存（L2，可选）。
/// 提供命中优先级为 L1 → L2 的读路径，以及写入两层的一致化写路径。
/// </summary>
public sealed class MultiLevelCache : IMultiLevelCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly IBloomFilter? _bloomFilter;
    private readonly ICacheInvalidator? _cacheInvalidator;
    private readonly ILogger<MultiLevelCache> _logger;
    private readonly MultiLevelCacheOptions _options;

    /// <summary>
    /// 初始化多级缓存实例。
    /// </summary>
    /// <param name="memoryCache">L1 内存缓存。</param>
    /// <param name="logger">日志记录器。</param>
    /// <param name="options">多级缓存配置项。</param>
    /// <param name="distributedCache">L2 分布式缓存（可选）。</param>
    /// <param name="bloomFilter">布隆过滤器（可选）。</param>
    /// <param name="cacheInvalidator">缓存失效广播器（可选）。</param>
    public MultiLevelCache(
        IMemoryCache memoryCache,
        ILogger<MultiLevelCache> logger,
        IOptions<MultiLevelCacheOptions> options,
        IDistributedCache? distributedCache = null,
        IBloomFilter? bloomFilter = null,
        ICacheInvalidator? cacheInvalidator = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _bloomFilter = bloomFilter;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// 从 L1/L2 获取缓存值。
    /// 优先命中 L1；若未命中且启用布隆过滤，可能直接返回未命中以降低穿透；最终尝试 L2。
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);

        if (_memoryCache.TryGetValue(fullKey, out T? valueFromL1))
        {
            _logger.LogDebug(CacheEventIds.HitL1, "L1 cache hit for {Key}", fullKey);
            return valueFromL1;
        }

        if (_options.EnableBloomFilter && _bloomFilter is not null && !_bloomFilter.MightContain(fullKey))
        {
            _logger.LogDebug(CacheEventIds.BloomFilterMiss, "Bloom filter miss for {Key}", fullKey);
            return default;
        }

        if (_distributedCache is null)
        {
            _logger.LogDebug(CacheEventIds.Miss, "Cache miss (no L2) for {Key}", fullKey);
            return default;
        }

        var bytes = await _distributedCache.GetAsync(fullKey, cancellationToken).ConfigureAwait(false);
        if (bytes is null)
        {
            _logger.LogDebug(CacheEventIds.Miss, "L2 cache miss for {Key}", fullKey);
            return default;
        }

        var value = JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
        _logger.LogDebug(CacheEventIds.HitL2, "L2 cache hit for {Key}", fullKey);

        // 将 L2 命中的结果以较短 TTL 回填至 L1（避免 L1 长时间持有导致的陈旧风险）
        var l1Ttl = _options.L1TimeToLiveRatio <= 0
            ? TimeSpan.FromSeconds(5)
            : TimeSpan.FromSeconds(Math.Max(1, _options.DefaultTimeToLive.TotalSeconds * _options.L1TimeToLiveRatio));

        _memoryCache.Set(fullKey, value, l1Ttl);
        return value;
    }

    /// <summary>
    /// 同步写入 L1 与（可用时）L2。
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);

        var ttl = timeToLive <= TimeSpan.Zero ? _options.DefaultTimeToLive : timeToLive;

        // L1：使用主 TTL 的比例控制，以降低 L1 陈旧风险
        var l1Ttl = _options.L1TimeToLiveRatio <= 0
            ? TimeSpan.FromSeconds(5)
            : TimeSpan.FromSeconds(Math.Max(1, ttl.TotalSeconds * _options.L1TimeToLiveRatio));
        _memoryCache.Set(fullKey, value, l1Ttl);
        _logger.LogDebug(CacheEventIds.SetL1, "Set L1 for {Key} with TTL {TTL}", fullKey, l1Ttl);

        // L2：若存在分布式缓存，则按完整 TTL 写入
        if (_distributedCache is not null)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            await _distributedCache.SetAsync(fullKey, bytes, options, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug(CacheEventIds.SetL2, "Set L2 for {Key} with TTL {TTL}", fullKey, ttl);
        }

        if (_options.EnableBloomFilter && _bloomFilter is not null)
        {
            _bloomFilter.Add(fullKey);
        }
    }

    /// <summary>
    /// 从 L1/L2 移除缓存，并在存在失效广播器时对外广播。
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        _memoryCache.Remove(fullKey);
        _logger.LogDebug(CacheEventIds.Remove, "Removed L1 for {Key}", fullKey);

        if (_distributedCache is not null)
        {
            await _distributedCache.RemoveAsync(fullKey, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug(CacheEventIds.Remove, "Removed L2 for {Key}", fullKey);
        }

        if (_cacheInvalidator is not null)
        {
            await _cacheInvalidator.BroadcastInvalidationAsync(fullKey, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取或新增：未命中时执行工厂方法，写入缓存后返回。
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken).ConfigureAwait(false);
        await SetAsync(key, value!, timeToLive, cancellationToken).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// 基于配置前缀构造完整键。
    /// </summary>
    private string BuildKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }
        return string.IsNullOrWhiteSpace(_options.KeyPrefix) ? key : _options.KeyPrefix + key;
    }
}


