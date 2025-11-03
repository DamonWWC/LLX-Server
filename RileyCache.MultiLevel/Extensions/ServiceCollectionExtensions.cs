using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RileyCache.MultiLevel.Core;
using RileyCache.MultiLevel.Options;
using RileyCache.MultiLevel.Strategies;

namespace RileyCache.MultiLevel.Extensions;

/// <summary>
/// DI 扩展：注册多级缓存及其相关依赖。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向依赖注入容器添加多级缓存。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configureOptions">配置项回调，可选。</param>
    /// <param name="tryAddMemoryCache">如为 true，则在容器中尝试注册默认的 <see cref="IMemoryCache"/>。</param>
    /// <returns>服务集合。</returns>
    public static IServiceCollection AddMultiLevelCache(
        this IServiceCollection services,
        Action<MultiLevelCacheOptions>? configureOptions = null,
        bool tryAddMemoryCache = true)
    {
        if (tryAddMemoryCache)
        {
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
        }

        services.AddOptions<MultiLevelCacheOptions>();
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        // 策略组件为可选；默认注册内存布隆过滤器实现（是否启用由 Options 控制）
        services.TryAddSingleton<IBloomFilter, InMemoryBloomFilter>();

        // 分布式缓存为可选——应用层可注册 Redis 等实现；此处提供 NoOp 以便依赖解析
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDistributedCache, NoOpDistributedCache>());

        services.AddSingleton<IMultiLevelCache, MultiLevelCache>();
        return services;
    }

    /// <summary>
    /// 无操作的分布式缓存占位实现；当应用层注册了真实的 <see cref="IDistributedCache"/> 时即可覆盖。
    /// </summary>
    private sealed class NoOpDistributedCache : IDistributedCache
    {
        public byte[]? Get(string key) => null;
        public System.Threading.Tasks.Task<byte[]?> GetAsync(string key, System.Threading.CancellationToken token = default) => System.Threading.Tasks.Task.FromResult<byte[]?>(null);
        public void Refresh(string key) { }
        public System.Threading.Tasks.Task RefreshAsync(string key, System.Threading.CancellationToken token = default) => System.Threading.Tasks.Task.CompletedTask;
        public void Remove(string key) { }
        public System.Threading.Tasks.Task RemoveAsync(string key, System.Threading.CancellationToken token = default) => System.Threading.Tasks.Task.CompletedTask;
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) { }
        public System.Threading.Tasks.Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, System.Threading.CancellationToken token = default) => System.Threading.Tasks.Task.CompletedTask;
    }
}


