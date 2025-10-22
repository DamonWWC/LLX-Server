using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LLX.Server.Utils;

/// <summary>
/// 数据库连接池优化配置
/// </summary>
public static class DatabaseConnectionOptimizer
{
    /// <summary>
    /// 数据库连接池配置
    /// </summary>
    public class ConnectionPoolConfig
    {
        public int MinPoolSize { get; set; } = 5;
        public int MaxPoolSize { get; set; } = 100;
        public int ConnectionLifetime { get; set; } = 300; // 5分钟
        public int ConnectionIdleLifetime { get; set; } = 300; // 5分钟
        public int CommandTimeout { get; set; } = 30; // 30秒
        public bool EnableRetryOnFailure { get; set; } = true;
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelay { get; set; } = 30; // 30秒
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public bool EnableDetailedErrors { get; set; } = false;
    }

    /// <summary>
    /// 配置数据库连接池
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection ConfigureDatabaseConnectionPool(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
        }

        var poolConfig = configuration.GetSection("Database:ConnectionPool").Get<ConnectionPoolConfig>() 
                        ?? new ConnectionPoolConfig();

        // 构建优化的连接字符串
        var optimizedConnectionString = BuildOptimizedConnectionString(connectionString, poolConfig);

        // 配置EF Core
        services.AddDbContext<Data.AppDbContext>(options =>
        {
            options.UseNpgsql(optimizedConnectionString, npgsqlOptions =>
            {
                // 启用重试策略
                if (poolConfig.EnableRetryOnFailure)
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: poolConfig.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(poolConfig.MaxRetryDelay),
                        errorCodesToAdd: null);
                }

                // 设置命令超时
                npgsqlOptions.CommandTimeout(poolConfig.CommandTimeout);

                // 启用性能优化
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // 开发环境配置
            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging(poolConfig.EnableSensitiveDataLogging);
                options.EnableDetailedErrors(poolConfig.EnableDetailedErrors);
            }

            // 查询优化
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // 配置连接池监控
        services.AddSingleton<IConnectionPoolMonitor, ConnectionPoolMonitor>();

        return services;
    }

    /// <summary>
    /// 构建优化的连接字符串
    /// </summary>
    /// <param name="baseConnectionString">基础连接字符串</param>
    /// <param name="config">连接池配置</param>
    /// <returns>优化的连接字符串</returns>
    private static string BuildOptimizedConnectionString(string baseConnectionString, ConnectionPoolConfig config)
    {
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            // 连接池配置
            MinPoolSize = config.MinPoolSize,
            MaxPoolSize = config.MaxPoolSize,
            ConnectionLifetime = config.ConnectionLifetime,
            ConnectionIdleLifetime = config.ConnectionIdleLifetime,
            
            // 性能优化配置
            CommandTimeout = config.CommandTimeout,
            Timeout = 15, // 连接超时15秒
            TcpKeepAlive = true,
            TcpKeepAliveTime = 30,
            TcpKeepAliveInterval = 5,
            
            // 其他优化
            ApplicationName = "LLX.Server",
            IncludeErrorDetail = config.EnableDetailedErrors,
            LogParameters = config.EnableSensitiveDataLogging
        };

        return builder.ToString();
    }
}

/// <summary>
/// 连接池监控服务
/// </summary>
public interface IConnectionPoolMonitor
{
    Task<ConnectionPoolStats> GetStatsAsync();
    Task<bool> IsHealthyAsync();
}

/// <summary>
/// 连接池统计信息
/// </summary>
public class ConnectionPoolStats
{
    public int ActiveConnections { get; set; }
    public int IdleConnections { get; set; }
    public int TotalConnections { get; set; }
    public int MaxConnections { get; set; }
    public double ConnectionUtilization { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 连接池监控实现
/// </summary>
public class ConnectionPoolMonitor : IConnectionPoolMonitor
{
    private readonly ILogger<ConnectionPoolMonitor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ConnectionPoolMonitor(ILogger<ConnectionPoolMonitor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<ConnectionPoolStats> GetStatsAsync()
    {
        try
        {
            // 这里可以添加更详细的连接池统计逻辑
            // 目前返回基本统计信息
            var stats = new ConnectionPoolStats
            {
                ActiveConnections = 0, // 需要从数据库获取
                IdleConnections = 0,
                TotalConnections = 0,
                MaxConnections = 100, // 从配置获取
                ConnectionUtilization = 0.0,
                LastUpdated = DateTime.UtcNow
            };

            return Task.FromResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connection pool stats");
            return Task.FromResult(new ConnectionPoolStats { LastUpdated = DateTime.UtcNow });
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
            
            // 执行简单查询检查连接健康状态
            await context.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection health check failed");
            return false;
        }
    }
}
