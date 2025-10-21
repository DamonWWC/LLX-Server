using Microsoft.EntityFrameworkCore;
using LLX.Server.Data;
using LLX.Server.Models.Entities;

namespace LLX.Server.Utils;

/// <summary>
/// 数据库性能测试工具
/// </summary>
public class DatabasePerformanceTest
{
    private readonly AppDbContext _context;
    private readonly DatabaseProvider _provider;

    public DatabasePerformanceTest(AppDbContext context, DatabaseProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    /// <summary>
    /// 运行完整的性能测试
    /// </summary>
    /// <returns>测试结果</returns>
    public async Task<PerformanceTestResult> RunFullTestAsync()
    {
        var result = new PerformanceTestResult
        {
            DatabaseProvider = _provider.ToString(),
            TestTime = DateTime.UtcNow
        };

        // 测试连接性能
        result.ConnectionTime = await TestConnectionPerformanceAsync();

        // 测试查询性能
        result.QueryPerformance = await TestQueryPerformanceAsync();

        // 测试插入性能
        result.InsertPerformance = await TestInsertPerformanceAsync();

        // 测试更新性能
        result.UpdatePerformance = await TestUpdatePerformanceAsync();

        // 测试删除性能
        result.DeletePerformance = await TestDeletePerformanceAsync();

        // 计算总体评分
        result.OverallScore = CalculateOverallScore(result);

        return result;
    }

    /// <summary>
    /// 测试连接性能
    /// </summary>
    private async Task<TimeSpan> TestConnectionPerformanceAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 10; i++)
        {
            await _context.Database.CanConnectAsync();
        }
        
        stopwatch.Stop();
        return TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / 10);
    }

    /// <summary>
    /// 测试查询性能
    /// </summary>
    private async Task<QueryPerformanceResult> TestQueryPerformanceAsync()
    {
        var result = new QueryPerformanceResult();

        // 简单查询
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var productCount = await _context.Products.CountAsync();
        stopwatch.Stop();
        result.SimpleQueryTime = stopwatch.Elapsed;

        // 复杂查询（带关联）
        stopwatch.Restart();
        var ordersWithItems = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Address)
            .Take(100)
            .ToListAsync();
        stopwatch.Stop();
        result.ComplexQueryTime = stopwatch.Elapsed;

        // 聚合查询
        stopwatch.Restart();
        var totalValue = await _context.Orders.SumAsync(o => o.GrandTotal);
        stopwatch.Stop();
        result.AggregateQueryTime = stopwatch.Elapsed;

        return result;
    }

    /// <summary>
    /// 测试插入性能
    /// </summary>
    private async Task<InsertPerformanceResult> TestInsertPerformanceAsync()
    {
        var result = new InsertPerformanceResult();
        var testProducts = new List<Product>();

        // 准备测试数据
        for (int i = 0; i < 100; i++)
        {
            testProducts.Add(new Product
            {
                Name = $"测试商品 {i}",
                Price = 10.50m + i,
                Unit = "袋",
                Weight = 1.0m + i,
                Quantity = 100 + i
            });
        }

        // 批量插入
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _context.Products.AddRange(testProducts);
        await _context.SaveChangesAsync();
        stopwatch.Stop();
        result.BatchInsertTime = stopwatch.Elapsed;

        // 单个插入
        var singleProduct = new Product
        {
            Name = "单个测试商品",
            Price = 15.00m,
            Unit = "袋",
            Weight = 1.5m,
            Quantity = 50
        };

        stopwatch.Restart();
        _context.Products.Add(singleProduct);
        await _context.SaveChangesAsync();
        stopwatch.Stop();
        result.SingleInsertTime = stopwatch.Elapsed;

        return result;
    }

    /// <summary>
    /// 测试更新性能
    /// </summary>
    private async Task<UpdatePerformanceResult> TestUpdatePerformanceAsync()
    {
        var result = new UpdatePerformanceResult();

        // 批量更新
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var products = await _context.Products.Take(50).ToListAsync();
        foreach (var product in products)
        {
            product.Price += 1.00m;
        }
        await _context.SaveChangesAsync();
        stopwatch.Stop();
        result.BatchUpdateTime = stopwatch.Elapsed;

        // 单个更新
        var singleProduct = await _context.Products.FirstOrDefaultAsync();
        if (singleProduct != null)
        {
            stopwatch.Restart();
            singleProduct.Price += 0.50m;
            await _context.SaveChangesAsync();
            stopwatch.Stop();
            result.SingleUpdateTime = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// 测试删除性能
    /// </summary>
    private async Task<DeletePerformanceResult> TestDeletePerformanceAsync()
    {
        var result = new DeletePerformanceResult();

        // 批量删除
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var productsToDelete = await _context.Products
            .Where(p => p.Name.Contains("测试商品"))
            .Take(50)
            .ToListAsync();
        _context.Products.RemoveRange(productsToDelete);
        await _context.SaveChangesAsync();
        stopwatch.Stop();
        result.BatchDeleteTime = stopwatch.Elapsed;

        // 单个删除
        var singleProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == "单个测试商品");
        if (singleProduct != null)
        {
            stopwatch.Restart();
            _context.Products.Remove(singleProduct);
            await _context.SaveChangesAsync();
            stopwatch.Stop();
            result.SingleDeleteTime = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// 计算总体评分
    /// </summary>
    private int CalculateOverallScore(PerformanceTestResult result)
    {
        var score = 100;

        // 连接时间评分
        if (result.ConnectionTime.TotalMilliseconds > 1000) score -= 20;
        else if (result.ConnectionTime.TotalMilliseconds > 500) score -= 10;

        // 查询性能评分
        if (result.QueryPerformance.SimpleQueryTime.TotalMilliseconds > 100) score -= 15;
        if (result.QueryPerformance.ComplexQueryTime.TotalMilliseconds > 1000) score -= 15;
        if (result.QueryPerformance.AggregateQueryTime.TotalMilliseconds > 500) score -= 10;

        // 插入性能评分
        if (result.InsertPerformance.BatchInsertTime.TotalMilliseconds > 2000) score -= 10;
        if (result.InsertPerformance.SingleInsertTime.TotalMilliseconds > 100) score -= 10;

        // 更新性能评分
        if (result.UpdatePerformance.BatchUpdateTime.TotalMilliseconds > 1000) score -= 10;
        if (result.UpdatePerformance.SingleUpdateTime.TotalMilliseconds > 50) score -= 10;

        // 删除性能评分
        if (result.DeletePerformance.BatchDeleteTime.TotalMilliseconds > 1000) score -= 10;
        if (result.DeletePerformance.SingleDeleteTime.TotalMilliseconds > 50) score -= 10;

        return Math.Max(0, score);
    }
}

/// <summary>
/// 性能测试结果
/// </summary>
public class PerformanceTestResult
{
    public string DatabaseProvider { get; set; } = string.Empty;
    public DateTime TestTime { get; set; }
    public TimeSpan ConnectionTime { get; set; }
    public QueryPerformanceResult QueryPerformance { get; set; } = new();
    public InsertPerformanceResult InsertPerformance { get; set; } = new();
    public UpdatePerformanceResult UpdatePerformance { get; set; } = new();
    public DeletePerformanceResult DeletePerformance { get; set; } = new();
    public int OverallScore { get; set; }
}

/// <summary>
/// 查询性能结果
/// </summary>
public class QueryPerformanceResult
{
    public TimeSpan SimpleQueryTime { get; set; }
    public TimeSpan ComplexQueryTime { get; set; }
    public TimeSpan AggregateQueryTime { get; set; }
}

/// <summary>
/// 插入性能结果
/// </summary>
public class InsertPerformanceResult
{
    public TimeSpan BatchInsertTime { get; set; }
    public TimeSpan SingleInsertTime { get; set; }
}

/// <summary>
/// 更新性能结果
/// </summary>
public class UpdatePerformanceResult
{
    public TimeSpan BatchUpdateTime { get; set; }
    public TimeSpan SingleUpdateTime { get; set; }
}

/// <summary>
/// 删除性能结果
/// </summary>
public class DeletePerformanceResult
{
    public TimeSpan BatchDeleteTime { get; set; }
    public TimeSpan SingleDeleteTime { get; set; }
}
