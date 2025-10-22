# Redis 连接字符串格式说明

## 📋 概述

本项目使用 StackExchange.Redis 库连接 Redis 服务器，支持多种连接字符串格式。

## 🔧 基本格式

### 1. 简单格式（无密码）
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### 2. 带密码格式
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=your_redis_password"
  }
}
```

### 3. 完整格式（推荐生产环境）
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=your_redis_password,ssl=false,abortConnect=false,connectRetry=3,connectTimeout=5000,syncTimeout=5000"
  }
}
```

## 📝 参数说明

| 参数 | 说明 | 默认值 | 示例 |
|------|------|--------|------|
| `host:port` | Redis 服务器地址和端口 | `localhost:6379` | `192.168.1.100:6379` |
| `password` | Redis 密码 | 无 | `password=mypassword` |
| `ssl` | 是否使用 SSL 连接 | `false` | `ssl=true` |
| `abortConnect` | 连接失败时是否中止 | `true` | `abortConnect=false` |
| `connectRetry` | 连接重试次数 | `3` | `connectRetry=5` |
| `connectTimeout` | 连接超时时间（毫秒） | `5000` | `connectTimeout=10000` |
| `syncTimeout` | 同步操作超时时间（毫秒） | `5000` | `syncTimeout=10000` |
| `database` | 数据库编号 | `0` | `database=1` |

## 🌍 不同环境配置

### 开发环境
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### 测试环境
```json
{
  "ConnectionStrings": {
    "Redis": "test-redis:6379,password=test_password"
  }
}
```

### 生产环境
```json
{
  "ConnectionStrings": {
    "Redis": "prod-redis-cluster:6379,password=strong_production_password,ssl=true,abortConnect=false,connectRetry=5,connectTimeout=10000"
  }
}
```

## 🔐 加密配置

### 加密前（明文）
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=myredispassword"
  }
}
```

### 加密后
```json
{
  "ConnectionStrings": {
    "Redis": "ENC:dP+2MUDL8gRCo1s9TJ+ZDzytaOXUEH+mTGBdu/2XvCQlahJgducaJY720CL/JLjVi8DoOkq/49gP7U7DjaKCQlsOZJ+572Nl6AZvgu7PfIDqx4n7J4XFdMW5UiaH+sU2"
  }
}
```

## 🏗️ 代码中的使用

### ServiceCollectionExtensions.cs 中的配置
```csharp
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
```

## 🔧 高级配置选项

### 1. 集群配置
```json
{
  "ConnectionStrings": {
    "Redis": "redis1:6379,redis2:6379,redis3:6379,password=cluster_password"
  }
}
```

### 2. 哨兵模式
```json
{
  "ConnectionStrings": {
    "Redis": "sentinel1:26379,sentinel2:26379,sentinel3:26379,serviceName=mymaster,password=master_password"
  }
}
```

### 3. 带数据库选择
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=mypassword,database=1"
  }
}
```

### 4. 性能优化配置
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=mypassword,abortConnect=false,connectRetry=5,connectTimeout=10000,syncTimeout=10000,asyncTimeout=10000,keepAlive=60"
  }
}
```

## 🐳 Docker 环境配置

### docker-compose.yml 中的 Redis 服务
```yaml
services:
  redis:
    image: redis:7.2-alpine
    container_name: llxrice_redis
    restart: unless-stopped
    command: redis-server --appendonly yes --requirepass your_redis_password
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - llxrice_network
```

### 应用程序连接配置
```json
{
  "ConnectionStrings": {
    "Redis": "redis:6379,password=your_redis_password"
  }
}
```

## 🔍 连接测试

### 使用 Redis CLI 测试连接
```bash
# 无密码连接
redis-cli -h localhost -p 6379

# 带密码连接
redis-cli -h localhost -p 6379 -a your_password

# 测试连接
ping
```

### 在应用程序中测试
```csharp
// 在 RedisCacheService 中会自动测试连接
public class RedisCacheService : ICacheService
{
    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        
        // 测试连接
        try
        {
            var db = _redis.GetDatabase();
            db.Ping();
            _logger.LogInformation("Redis connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis");
        }
    }
}
```

## ⚠️ 常见问题

### 1. 连接超时
**问题**: `TimeoutException` 或连接超时
**解决**: 增加超时时间
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,connectTimeout=10000,syncTimeout=10000"
  }
}
```

### 2. 认证失败
**问题**: `RedisAuthenticationException`
**解决**: 检查密码是否正确
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=correct_password"
  }
}
```

### 3. 连接被拒绝
**问题**: `RedisConnectionException`
**解决**: 检查 Redis 服务是否启动，端口是否正确

### 4. SSL 连接问题
**问题**: SSL 握手失败
**解决**: 根据 Redis 配置调整 SSL 设置
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,ssl=false"
  }
}
```

## 📚 相关文档

- [StackExchange.Redis 官方文档](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis 配置文档](https://redis.io/docs/manual/config/)
- [项目配置加密指南](./CONFIG_ENCRYPTION_GUIDE.md)

## 🎯 最佳实践

1. **生产环境使用加密配置**
2. **设置合适的超时时间**
3. **使用连接池和重试机制**
4. **监控 Redis 连接状态**
5. **定期备份 Redis 数据**
6. **使用强密码**
7. **限制网络访问**

---

**文档版本**: v1.0  
**创建日期**: 2025年10月21日  
**适用版本**: .NET 8 + StackExchange.Redis 2.7
