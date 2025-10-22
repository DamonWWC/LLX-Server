namespace LLX.Server.Services;

/// <summary>
/// 缓存策略配置
/// </summary>
public static class CacheStrategy
{
    /// <summary>
    /// 缓存键前缀
    /// </summary>
    public static class Keys
    {
        public const string Product = "llxrice:product:";
        public const string ProductAll = "llxrice:product:all";
        public const string ProductSearch = "llxrice:product:search:";
        
        public const string Address = "llxrice:address:";
        public const string AddressAll = "llxrice:address:all";
        public const string AddressDefault = "llxrice:address:default";
        public const string AddressPhone = "llxrice:address:phone:";
        
        public const string Order = "llxrice:order:";
        public const string OrderAll = "llxrice:order:all";
        public const string OrderByStatus = "llxrice:order:status:";
        public const string OrderByAddress = "llxrice:order:address:";
        public const string OrderByOrderNo = "llxrice:order:orderno:";
        
        public const string Shipping = "llxrice:shipping:";
        public const string ShippingAll = "llxrice:shipping:all";
        public const string ShippingByProvince = "llxrice:shipping:province:";
    }

    /// <summary>
    /// 缓存过期时间配置
    /// </summary>
    public static class Expiry
    {
        // 商品相关缓存
        public static readonly TimeSpan ProductAll = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan ProductSingle = TimeSpan.FromHours(1);
        public static readonly TimeSpan ProductSearch = TimeSpan.FromMinutes(15);
        
        // 地址相关缓存
        public static readonly TimeSpan AddressAll = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan AddressSingle = TimeSpan.FromHours(2);
        public static readonly TimeSpan AddressDefault = TimeSpan.FromMinutes(10);
        
        // 订单相关缓存
        public static readonly TimeSpan OrderAll = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan OrderSingle = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan OrderByStatus = TimeSpan.FromMinutes(5);
        
        // 运费相关缓存
        public static readonly TimeSpan ShippingAll = TimeSpan.FromHours(6);
        public static readonly TimeSpan ShippingSingle = TimeSpan.FromHours(12);
        public static readonly TimeSpan ShippingByProvince = TimeSpan.FromHours(4);
        
        // 空值缓存（防缓存穿透）
        public static readonly TimeSpan EmptyValue = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// 缓存模式配置
    /// </summary>
    public static class Patterns
    {
        public const string ProductAll = "llxrice:product:*";
        public const string AddressAll = "llxrice:address:*";
        public const string OrderAll = "llxrice:order:*";
        public const string ShippingAll = "llxrice:shipping:*";
    }

    /// <summary>
    /// 获取带随机偏移的过期时间（防缓存雪崩）
    /// </summary>
    /// <param name="baseExpiry">基础过期时间</param>
    /// <param name="randomOffsetPercent">随机偏移百分比（0-0.2，即0-20%）</param>
    /// <returns>带随机偏移的过期时间</returns>
    public static TimeSpan GetRandomExpiry(TimeSpan baseExpiry, double randomOffsetPercent = 0.1)
    {
        var random = new Random();
        var offset = baseExpiry.TotalMilliseconds * randomOffsetPercent;
        var randomOffset = random.NextDouble() * offset * 2 - offset; // -offset 到 +offset
        
        return TimeSpan.FromMilliseconds(baseExpiry.TotalMilliseconds + randomOffset);
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <param name="suffix">后缀</param>
    /// <returns>完整的缓存键</returns>
    public static string GenerateKey(string prefix, string suffix)
    {
        return $"{prefix}{suffix}";
    }

    /// <summary>
    /// 生成搜索缓存键
    /// </summary>
    /// <param name="searchTerm">搜索词</param>
    /// <returns>搜索缓存键</returns>
    public static string GenerateSearchKey(string searchTerm)
    {
        // 对搜索词进行标准化处理
        var normalizedTerm = searchTerm?.Trim().ToLowerInvariant() ?? string.Empty;
        return $"{Keys.ProductSearch}{normalizedTerm}";
    }
}
