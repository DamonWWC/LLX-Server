namespace LLX.Server.Services;

/// <summary>
/// 缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>缓存数据</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>任务</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>任务</returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// 按模式删除缓存
    /// </summary>
    /// <param name="pattern">模式</param>
    /// <returns>任务</returns>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// 获取或设置缓存（防缓存穿透）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="factory">数据获取工厂方法</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>缓存数据</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiry = null);

    /// <summary>
    /// 批量获取缓存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="keys">缓存键列表</param>
    /// <returns>缓存数据字典</returns>
    Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys);

    /// <summary>
    /// 批量设置缓存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="items">缓存项字典</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>任务</returns>
    Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null);

    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 设置缓存过期时间
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="expiry">过期时间</param>
    /// <returns>是否设置成功</returns>
    Task<bool> ExpireAsync(string key, TimeSpan expiry);

    /// <summary>
    /// 获取缓存剩余过期时间
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>剩余过期时间</returns>
    Task<TimeSpan?> GetTimeToLiveAsync(string key);
}
