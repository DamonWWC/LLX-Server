using Microsoft.Extensions.Logging;

namespace RileyCache.MultiLevel.Logging;

/// <summary>
/// 缓存相关的结构化日志事件编号定义。
/// </summary>
public static class CacheEventIds
{
    /// <summary>L1 内存缓存命中。</summary>
    public static readonly EventId HitL1 = new(1000, nameof(HitL1));
    /// <summary>L2 分布式缓存命中。</summary>
    public static readonly EventId HitL2 = new(1001, nameof(HitL2));
    /// <summary>缓存未命中。</summary>
    public static readonly EventId Miss = new(1002, nameof(Miss));
    /// <summary>写入 L1。</summary>
    public static readonly EventId SetL1 = new(1003, nameof(SetL1));
    /// <summary>写入 L2。</summary>
    public static readonly EventId SetL2 = new(1004, nameof(SetL2));
    /// <summary>移除缓存键。</summary>
    public static readonly EventId Remove = new(1005, nameof(Remove));
    /// <summary>布隆过滤器判定为“不存在”。</summary>
    public static readonly EventId BloomFilterMiss = new(1006, nameof(BloomFilterMiss));
    /// <summary>错误事件。</summary>
    public static readonly EventId Error = new(1099, nameof(Error));
}


