# .NET Core 配置系统详解

## 🎯 概述

本文档详细说明在.NET Core中如何读取.env文件中的配置信息，包括配置加载、优先级、使用方式等。

## 📦 依赖包

### 1. DotNetEnv 包

```xml
<PackageReference Include="DotNetEnv" Version="3.1.1" />
```

**功能**：将.env文件中的环境变量加载到当前进程的环境变量中。

## 🔧 配置加载流程

### 1. 在 Program.cs 中加载 .env 文件

```csharp
using DotNetEnv;

public class Program
{
    public static async Task Main(string[] args)
    {
        // 第一步：加载 .env 文件到环境变量
        Env.Load();

        // 第二步：创建 WebApplicationBuilder
        var builder = WebApplication.CreateBuilder(args);

        // 第三步：获取配置
        var configuration = builder.Configuration;
        
        // 第四步：使用配置
        // ...
    }
}
```

### 2. 配置加载顺序

.NET Core 配置系统按以下顺序加载配置（后面的会覆盖前面的）：

1. **appsettings.json**
2. **appsettings.{Environment}.json**
3. **环境变量**
4. **命令行参数**

## 📋 .env 文件格式

### 1. 基本格式

```bash
# .env 文件示例
ASPNETCORE_ENVIRONMENT=Production
DB_PROVIDER=PostgreSQL
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30
REDIS_CONNECTION_STRING=localhost:6379,password=your_redis_password,ssl=false,abortConnect=false
API_PORT=8080
LOG_LEVEL=Information
```

### 2. 注释和空行

```bash
# 这是注释
ASPNETCORE_ENVIRONMENT=Production

# 空行会被忽略
DB_PROVIDER=PostgreSQL
```

### 3. 引号处理

```bash
# 带引号的值
DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password"

# 不带引号的值
DB_PROVIDER=PostgreSQL
```

## 🔍 配置读取方式

### 1. 通过 IConfiguration 读取

```csharp
public class MyService
{
    private readonly IConfiguration _configuration;

    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ReadConfig()
    {
        // 读取环境变量
        var dbProvider = _configuration["DB_PROVIDER"];
        var dbConnectionString = _configuration["DB_CONNECTION_STRING"];
        
        // 读取嵌套配置
        var logLevel = _configuration["Logging:LogLevel:Default"];
        
        // 读取连接字符串
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
    }
}
```

### 2. 通过环境变量直接读取

```csharp
public class MyService
{
    public void ReadEnvironmentVariables()
    {
        // 直接读取环境变量
        var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER");
        var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        
        // 指定环境变量作用域
        var userDbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER", EnvironmentVariableTarget.User);
        var machineDbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER", EnvironmentVariableTarget.Machine);
    }
}
```

### 3. 强类型配置

```csharp
// 定义配置类
public class DatabaseOptions
{
    public string Provider { get; set; } = "PostgreSQL";
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
}

// 在 Program.cs 中绑定配置
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(options =>
{
    options.Provider = builder.Configuration["DB_PROVIDER"] ?? "PostgreSQL";
    options.ConnectionString = builder.Configuration["DB_CONNECTION_STRING"] ?? string.Empty;
    options.CommandTimeout = int.Parse(builder.Configuration["DB_COMMAND_TIMEOUT"] ?? "30");
});

// 在服务中使用
public class MyService
{
    private readonly DatabaseOptions _dbOptions;

    public MyService(IOptions<DatabaseOptions> dbOptions)
    {
        _dbOptions = dbOptions.Value;
    }

    public void UseConfig()
    {
        var provider = _dbOptions.Provider;
        var connectionString = _dbOptions.ConnectionString;
    }
}
```

## 🔄 配置映射机制

### 1. 环境变量到配置的映射

在 `docker-compose.api-only.yml` 中：

```yaml
environment:
  # 环境变量映射到配置
  - Database__Provider=${DB_PROVIDER:-PostgreSQL}
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
  - ConnectionStrings__Redis=${REDIS_CONNECTION_STRING}
```

**映射规则**：
- `Database__Provider` → `Database:Provider`
- `ConnectionStrings__DefaultConnection` → `ConnectionStrings:DefaultConnection`

### 2. 配置键命名规则

```csharp
// 环境变量名 → 配置键
"DB_PROVIDER" → "DB_PROVIDER"
"Database__Provider" → "Database:Provider"
"ConnectionStrings__DefaultConnection" → "ConnectionStrings:DefaultConnection"
```

## 🎯 实际应用示例

### 1. 数据库配置读取

```csharp
// 在 ServiceCollectionExtensions.cs 中
public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
{
    // 读取数据库提供商
    var databaseProvider = configuration.GetValue<string>("Database:Provider") ?? "PostgreSQL";
    var provider = Enum.Parse<DatabaseProvider>(databaseProvider);

    // 读取连接字符串
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Database connection string not found");

    // 使用配置
    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        var encryptionService = serviceProvider.GetRequiredService<IConfigurationEncryptionService>();
        var decryptedConnectionString = encryptionService.DecryptConnectionStringIfNeeded(connectionString);

        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                options.UseNpgsql(decryptedConnectionString);
                break;
            case DatabaseProvider.SqlServer:
                options.UseSqlServer(decryptedConnectionString);
                break;
            // ... 其他数据库提供商
        }
    });

    return services;
}
```

### 2. Redis配置读取

```csharp
public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Redis connection string not found");

    services.AddSingleton<IConnectionMultiplexer>(provider =>
    {
        var encryptionService = provider.GetRequiredService<IConfigurationEncryptionService>();
        var decryptedConnectionString = encryptionService.DecryptConnectionStringIfNeeded(connectionString);

        var redisConfiguration = ConfigurationOptions.Parse(decryptedConnectionString);
        return ConnectionMultiplexer.Connect(redisConfiguration);
    });

    return services;
}
```

## 🔧 配置验证

### 1. 配置验证扩展

```csharp
public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Required configuration key '{key}' is missing or empty.");
        }
        return value;
    }

    public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
    {
        var value = configuration.GetValue<T>(key);
        if (value == null)
        {
            throw new InvalidOperationException($"Required configuration key '{key}' is missing or empty.");
        }
        return value;
    }
}

// 使用示例
var dbProvider = configuration.GetRequiredValue<string>("Database:Provider");
var connectionString = configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");
```

### 2. 配置验证服务

```csharp
public class ConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(IConfiguration configuration, ILogger<ConfigurationValidationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool ValidateConfiguration()
    {
        var isValid = true;

        // 验证数据库配置
        var dbProvider = _configuration["Database:Provider"];
        if (string.IsNullOrEmpty(dbProvider))
        {
            _logger.LogError("Database:Provider is not configured");
            isValid = false;
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("ConnectionStrings:DefaultConnection is not configured");
            isValid = false;
        }

        // 验证Redis配置
        var redisConnectionString = _configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            _logger.LogError("ConnectionStrings:Redis is not configured");
            isValid = false;
        }

        return isValid;
    }
}
```

## 🚀 最佳实践

### 1. 配置组织

```bash
# .env 文件组织
# ============================================
# 应用基础配置
# ============================================
ASPNETCORE_ENVIRONMENT=Production
API_PORT=8080

# ============================================
# 数据库配置
# ============================================
DB_PROVIDER=PostgreSQL
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# ============================================
# Redis配置
# ============================================
REDIS_CONNECTION_STRING=localhost:6379,password=your_redis_password,ssl=false,abortConnect=false

# ============================================
# 日志配置
# ============================================
LOG_LEVEL=Information
```

### 2. 配置安全

```csharp
// 敏感配置加密
public class SecureConfigurationService
{
    private readonly IConfigurationEncryptionService _encryptionService;

    public SecureConfigurationService(IConfigurationEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public string GetSecureConnectionString(string key)
    {
        var connectionString = _configuration.GetConnectionString(key);
        return _encryptionService.DecryptConnectionStringIfNeeded(connectionString);
    }
}
```

### 3. 配置缓存

```csharp
// 配置缓存
public class CachedConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public CachedConfigurationService(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration;
        _cache = cache;
    }

    public string GetCachedValue(string key)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return _configuration[key];
        });
    }
}
```

## 🔍 调试配置

### 1. 配置调试端点

```csharp
// 开发环境配置调试端点
if (app.Environment.IsDevelopment())
{
    app.MapGet("/config/debug", (IConfiguration config) =>
    {
        var configData = new Dictionary<string, string>();
        
        // 收集所有配置
        foreach (var item in config.AsEnumerable())
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                configData[item.Key] = item.Value;
            }
        }
        
        return Results.Json(configData);
    });
}
```

### 2. 配置日志

```csharp
// 配置加载日志
public class ConfigurationLogger
{
    private readonly ILogger<ConfigurationLogger> _logger;

    public ConfigurationLogger(ILogger<ConfigurationLogger> logger)
    {
        _logger = logger;
    }

    public void LogConfiguration(IConfiguration configuration)
    {
        _logger.LogInformation("Configuration loaded:");
        
        foreach (var item in configuration.AsEnumerable())
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                // 隐藏敏感信息
                var displayValue = IsSensitiveKey(item.Key) ? "***" : item.Value;
                _logger.LogInformation("  {Key}: {Value}", item.Key, displayValue);
            }
        }
    }

    private bool IsSensitiveKey(string key)
    {
        var sensitiveKeys = new[] { "password", "connectionstring", "key", "secret" };
        return sensitiveKeys.Any(sensitive => key.ToLower().Contains(sensitive));
    }
}
```

## 📋 总结

### 配置加载流程

1. **DotNetEnv.Load()** - 将.env文件加载到环境变量
2. **WebApplication.CreateBuilder()** - 创建配置构建器
3. **IConfiguration** - 通过配置接口读取配置
4. **服务注册** - 在服务注册时使用配置

### 配置优先级

1. **环境变量** (最高优先级)
2. **appsettings.json**
3. **默认值**

### 关键点

- ✅ 使用 `DotNetEnv` 包加载.env文件
- ✅ 在 `Program.cs` 的 `Main` 方法开始处调用 `Env.Load()`
- ✅ 通过 `IConfiguration` 接口读取配置
- ✅ 支持强类型配置绑定
- ✅ 支持配置验证和调试

---

**文档版本**：v1.0  
**最后更新**：2025-10-22  
**适用版本**：.NET 8.0
