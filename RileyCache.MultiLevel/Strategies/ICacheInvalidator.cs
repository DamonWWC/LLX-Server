using System.Threading;
using System.Threading.Tasks;

namespace RileyCache.MultiLevel.Strategies;

/// <summary>
/// 缓存失效策略接口，用于在本地与多节点间传播键失效事件。
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// 使本地缓存对指定键失效。
    /// </summary>
    Task InvalidateLocalAsync(string fullKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将失效事件广播给其他节点（例如通过发布/订阅机制）。
    /// </summary>
    Task BroadcastInvalidationAsync(string fullKey, CancellationToken cancellationToken = default);
}


