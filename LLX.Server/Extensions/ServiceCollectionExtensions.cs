using LLX.Server.Data;
using LLX.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace LLX.Server.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册数据库服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionEncryptString = configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("Database connection string not found");
        // 获取数据库配置
        var databaseProvider = configuration.GetValue<string>("Database:Provider") ?? "PostgreSQL";

        var provider = Enum.Parse<DatabaseProvider>(databaseProvider);
        // 设置当前数据库提供程序
        AppDbContext.SetCurrentProvider(provider);

        services.AddDbContext<AppDbContext>((serviceProvider,options) =>
        {
            var encryptionService = serviceProvider.GetRequiredService<IConfigurationEncryptionService>();
            var connectionString = encryptionService.DecryptConnectionStringIfNeeded(connectionEncryptString);

            switch (provider)
            {
                case DatabaseProvider.PostgreSQL:
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    });
                    break;

                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                    break;

                case DatabaseProvider.MySql:
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                    break;

                case DatabaseProvider.Sqlite:
                    options.UseSqlite(connectionString);
                    break;

                default:
                    throw new NotSupportedException($"Database provider {provider} is not supported.");
            }
        });

        return services;
    }

   
    /// <summary>
    /// 检测数据库提供程序
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库提供程序</returns>
    private static DatabaseProvider DetectDatabaseProvider(string connectionString)
    {
        var cs = connectionString.ToLowerInvariant();
        
        if (cs.Contains("server=") && cs.Contains("port=") && !cs.Contains("sql server"))
            return DatabaseProvider.MySql;
        else if (cs.Contains("server=") || cs.Contains("data source="))
            return DatabaseProvider.SqlServer;
        else if (cs.Contains("data source=") && cs.Contains(".db"))
            return DatabaseProvider.Sqlite;
        else
            return DatabaseProvider.PostgreSQL;
    }

    /// <summary>
    /// 注册 Redis 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionEncryptString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string not found");
            
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var encryptionService = provider.GetRequiredService<IConfigurationEncryptionService>();
            var connectionString = encryptionService.DecryptConnectionStringIfNeeded(connectionEncryptString);

            var redisConfiguration = ConfigurationOptions.Parse(connectionString);
            redisConfiguration.AbortOnConnectFail = false;
            redisConfiguration.ConnectRetry = 3;
            redisConfiguration.ConnectTimeout = 5000;
            redisConfiguration.SyncTimeout = 5000;
            
            return ConnectionMultiplexer.Connect(redisConfiguration);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// 注册仓储层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<Repositories.IProductRepository, Repositories.ProductRepository>();
        services.AddScoped<Repositories.IAddressRepository, Repositories.AddressRepository>();
        services.AddScoped<Repositories.IOrderRepository, Repositories.OrderRepository>();
        services.AddScoped<Repositories.IShippingRepository, Repositories.ShippingRepository>();

        return services;
    }

    /// <summary>
    /// 注册业务服务层
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<Services.IProductService, Services.ProductService>();
        services.AddScoped<Services.IAddressService, Services.AddressService>();
        services.AddScoped<Services.IOrderService, Services.OrderService>();
        services.AddScoped<Services.IShippingService, Services.ShippingService>();

        return services;
    }

    /// <summary>
    /// 注册所有核心服务（数据库、Redis、仓储、业务服务）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDatabase(configuration)
            .AddRedis(configuration)
            .AddRepositories()
            .AddBusinessServices();
    }

    /// <summary>
    /// 注册健康检查服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Database connection string not found");
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string not found");

        // 获取数据库配置
        var databaseProvider = configuration.GetValue<string>("Database:Provider") ?? "PostgreSQL";
        var provider = Enum.Parse<DatabaseProvider>(databaseProvider);
        
        var healthChecks = services.AddHealthChecks();

        // 根据数据库类型添加相应的健康检查
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                healthChecks.AddNpgSql(dbConnectionString, name: "PostgreSQL DB", failureStatus: HealthStatus.Unhealthy);
                break;
            case DatabaseProvider.SqlServer:
                healthChecks.AddSqlServer(dbConnectionString, name: "SQL Server DB", failureStatus: HealthStatus.Unhealthy);
                break;
            case DatabaseProvider.MySql:
                healthChecks.AddMySql(dbConnectionString, name: "MySQL DB", failureStatus: HealthStatus.Unhealthy);
                break;
            case DatabaseProvider.Sqlite:
                healthChecks.AddSqlite(dbConnectionString, name: "SQLite DB", failureStatus: HealthStatus.Unhealthy);
                break;
        }

        // 添加 Redis 健康检查
        healthChecks.AddRedis(redisConnectionString, name: "Redis Cache", failureStatus: HealthStatus.Unhealthy);

        return services;
    }

    /// <summary>
    /// 注册 Swagger 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "林龍香大米商城 API",
                Version = "v1",
                Description = "基于 .NET 8 Minimal API 的后端服务",
                Contact = new()
                {
                    Name = "LLXRice Team",
                    Email = "support@llxrice.com"
                }
            });

            // 包含 XML 注释
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
