using System;
using System.Threading;
using System.Threading.Tasks;

namespace RileyCache.MultiLevel.Core;

/// <summary>
/// 多级缓存接口，提供 L1（内存）与可选 L2（分布式）缓存的统一访问能力。
/// </summary>
public interface IMultiLevelCache
{
    /// <summary>
    /// 根据键获取缓存值。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="key">缓存键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>命中返回值，否则返回默认值。</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置缓存值。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="key">缓存键。</param>
    /// <param name="value">缓存值。</param>
    /// <param name="timeToLive">生存时间（TTL）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task SetAsync<T>(string key, T value, TimeSpan timeToLive, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除指定键的缓存。
    /// </summary>
    /// <param name="key">缓存键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取缓存，若未命中则执行工厂方法并写入缓存。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="key">缓存键。</param>
    /// <param name="factory">当未命中时用于生成值的异步工厂方法。</param>
    /// <param name="timeToLive">生存时间（TTL）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>最终的值（来自缓存或工厂）。</returns>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default);
}


