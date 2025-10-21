namespace LLX.Server.Utils;

/// <summary>
/// 订单号生成器
/// </summary>
public static class OrderNumberGenerator
{
    private static readonly object _lock = new();
    private static int _counter = 0;

    /// <summary>
    /// 生成订单号
    /// 格式: ORD + 时间戳毫秒 + 3位计数器
    /// </summary>
    /// <returns>订单号</returns>
    public static string Generate()
    {
        lock (_lock)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var counter = Interlocked.Increment(ref _counter) % 1000;
            
            return $"ORD{timestamp}{counter:D3}";
        }
    }
}
