using System;

namespace RileyCache.MultiLevel.Core;

/// <summary>
/// 表示一个带有基础元数据的缓存项。
/// </summary>
/// <typeparam name="T">缓存值的类型。</typeparam>
public sealed class CacheItem<T>
{
    /// <summary>
    /// 初始化 <see cref="CacheItem{T}"/> 实例。
    /// </summary>
    /// <param name="value">缓存值。</param>
    /// <param name="createdAtUtc">创建时间（UTC）。</param>
    /// <param name="timeToLive">生存时间（TTL）。</param>
    public CacheItem(T value, DateTimeOffset createdAtUtc, TimeSpan timeToLive)
    {
        Value = value;
        CreatedAtUtc = createdAtUtc;
        TimeToLive = timeToLive;
    }

    /// <summary>
    /// 缓存值。
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// 创建时间（UTC）。
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// 生存时间（TTL）。
    /// </summary>
    public TimeSpan TimeToLive { get; }

    /// <summary>
    /// 过期时间（UTC）。
    /// </summary>
    public DateTimeOffset ExpiresAtUtc => CreatedAtUtc + TimeToLive;

    /// <summary>
    /// 判断缓存项是否过期。
    /// </summary>
    /// <param name="nowUtc">当前时间（UTC），可选；未传入时使用 <see cref="DateTimeOffset.UtcNow"/>。</param>
    /// <returns>若已过期返回 true，否则为 false。</returns>
    public bool IsExpired(DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        return now >= ExpiresAtUtc;
    }
}


