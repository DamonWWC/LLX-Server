using Microsoft.EntityFrameworkCore;
using LLX.Server.Data;
using LLX.Server.Models.Entities;

namespace LLX.Server;

/// <summary>
/// 数据库测试类 - 用于测试不同数据库的兼容性
/// </summary>
public static class DatabaseTest
{
    /// <summary>
    /// 测试数据库连接
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="provider">数据库提供程序</param>
    /// <returns>测试结果</returns>
    public static async Task<bool> TestConnectionAsync(string connectionString, DatabaseProvider provider)
    {
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseDatabase(connectionString, provider)
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数据库连接测试失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 测试数据库迁移
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="provider">数据库提供程序</param>
    /// <returns>测试结果</returns>
    public static async Task<bool> TestMigrationAsync(string connectionString, DatabaseProvider provider)
    {
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseDatabase(connectionString, provider)
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.MigrateAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数据库迁移测试失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 测试基本 CRUD 操作
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="provider">数据库提供程序</param>
    /// <returns>测试结果</returns>
    public static async Task<bool> TestCrudOperationsAsync(string connectionString, DatabaseProvider provider)
    {
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseDatabase(connectionString, provider)
                .Options;

            using var context = new AppDbContext(options);

            // 创建测试商品
            var product = new Product
            {
                Name = "测试商品",
                Price = 10.50m,
                Unit = "袋",
                Weight = 1.0m,
                Quantity = 100
            };

            // 添加
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // 查询
            var foundProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "测试商品");
            if (foundProduct == null) return false;

            // 更新
            foundProduct.Price = 12.00m;
            await context.SaveChangesAsync();

            // 删除
            context.Products.Remove(foundProduct);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRUD 操作测试失败: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// 数据库选项构建器扩展
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// 配置数据库
    /// </summary>
    /// <param name="optionsBuilder">选项构建器</param>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="provider">数据库提供程序</param>
    /// <returns>选项构建器</returns>
    public static DbContextOptionsBuilder<AppDbContext> UseDatabase(
        this DbContextOptionsBuilder<AppDbContext> optionsBuilder,
        string connectionString,
        DatabaseProvider provider)
    {
        return provider switch
        {
            DatabaseProvider.PostgreSQL => optionsBuilder.UseNpgsql(connectionString),
            DatabaseProvider.SqlServer => optionsBuilder.UseSqlServer(connectionString),
            DatabaseProvider.MySql => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
            DatabaseProvider.Sqlite => optionsBuilder.UseSqlite(connectionString),
            _ => throw new NotSupportedException($"Database provider {provider} is not supported.")
        };
    }
}
