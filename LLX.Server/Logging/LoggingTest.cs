using Microsoft.Extensions.Logging;

namespace LLX.Server.Logging;

/// <summary>
/// 日志系统测试类
/// </summary>
public class LoggingTest
{
    private readonly ILogger<LoggingTest> _logger;

    public LoggingTest(ILogger<LoggingTest> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 测试所有日志级别
    /// </summary>
    public void TestAllLogLevels()
    {
        _logger.LogTrace("这是一条跟踪日志 - 用于最详细的调试信息");
        _logger.LogDebug("这是一条调试日志 - 用于开发时的调试信息");
        _logger.LogInformation("这是一条信息日志 - 用于记录常规操作");
        _logger.LogWarning("这是一条警告日志 - 用于记录需要注意的问题");
        _logger.LogError("这是一条错误日志 - 用于记录可恢复的错误");
        _logger.LogCritical("这是一条严重错误日志 - 用于记录系统级严重错误");
    }

    /// <summary>
    /// 测试结构化日志
    /// </summary>
    public void TestStructuredLogging()
    {
        var orderId = 12345;
        var userId = "user001";
        var amount = 99.99m;

        _logger.LogInformation("订单创建成功: OrderId={OrderId}, UserId={UserId}, Amount={Amount}", 
            orderId, userId, amount);

        _logger.LogWarning("库存不足: ProductId={ProductId}, Required={Required}, Available={Available}", 
            1, 10, 5);

        _logger.LogError("支付失败: OrderId={OrderId}, ErrorCode={ErrorCode}, Message={Message}", 
            orderId, "PAY_001", "余额不足");
    }

    /// <summary>
    /// 测试异常日志
    /// </summary>
    public void TestExceptionLogging()
    {
        try
        {
            throw new InvalidOperationException("这是一个测试异常");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理订单时发生错误: OrderId={OrderId}", 12345);
        }

        try
        {
            throw new ArgumentNullException("参数不能为空");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "系统发生严重错误，需要立即处理");
        }
    }

    /// <summary>
    /// 测试不同类别的日志
    /// </summary>
    public void TestDifferentCategories()
    {
        // 模拟不同服务的日志
        _logger.LogInformation("ProductService: 获取商品列表成功，共 {Count} 个商品", 10);
        _logger.LogInformation("OrderService: 创建订单成功，订单号: {OrderNo}", "ORD20250117001");
        _logger.LogInformation("CacheService: 缓存命中，Key: {CacheKey}", "products:all");
        _logger.LogInformation("DatabaseService: 执行查询成功，耗时: {ElapsedMs}ms", 45);
    }
}
