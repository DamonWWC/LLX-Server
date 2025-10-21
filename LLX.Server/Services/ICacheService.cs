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
}
