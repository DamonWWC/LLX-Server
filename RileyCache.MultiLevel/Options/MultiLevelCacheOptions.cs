using System;

namespace RileyCache.MultiLevel.Options;

/// <summary>
/// 多级缓存配置选项。
/// </summary>
public sealed class MultiLevelCacheOptions
{
    /// <summary>
    /// 应用于所有键的前缀（可选）。
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// 默认生存时间（当调用方未显式传入 TTL 时使用）。
    /// </summary>
    public TimeSpan DefaultTimeToLive { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// L1 TTL 相对于主 TTL 的比例，例如 0.1 表示 10%。
    /// </summary>
    public double L1TimeToLiveRatio { get; set; } = 0.1;

    /// <summary>
    /// 是否启用布隆过滤以降低缓存穿透。
    /// </summary>
    public bool EnableBloomFilter { get; set; } = false;

    /// <summary>
    /// 布隆过滤器容量建议值（仅内存实现使用）。
    /// </summary>
    public int BloomFilterCapacity { get; set; } = 100_000;

    /// <summary>
    /// 布隆过滤器的误判率（0..1）。
    /// </summary>
    public double BloomFilterErrorRate { get; set; } = 0.01;
}


