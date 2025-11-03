namespace RileyCache.MultiLevel.Strategies;

/// <summary>
/// 布隆过滤器接口，用于降低缓存穿透（请求大量不存在的键）。
/// </summary>
public interface IBloomFilter
{
    /// <summary>
    /// 向过滤器中添加元素。
    /// </summary>
    /// <param name="item">元素字符串。</param>
    void Add(string item);

    /// <summary>
    /// 判断元素可能存在（可能为真，不存在时一定为假）。
    /// </summary>
    /// <param name="item">元素字符串。</param>
    /// <returns>可能存在返回 true；一定不存在返回 false。</returns>
    bool MightContain(string item);
}


