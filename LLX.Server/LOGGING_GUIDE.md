# 日志系统使用指南

## 概述

本项目使用 .NET Core 内置的 `Microsoft.Extensions.Logging` 日志系统，实现了按日志级别分文件的日志记录功能。

## 功能特点

- ✅ **分级别记录**: 不同日志级别生成独立的日志文件
- ✅ **按日期归档**: 日志文件按日期自动分割
- ✅ **自动清理**: 支持保留指定数量的历史日志文件
- ✅ **多输出目标**: 同时支持控制台、调试器和文件输出
- ✅ **高性能**: 异步写入，不阻塞主线程
- ✅ **线程安全**: 支持并发写入

## 日志文件结构

日志文件按日志级别和日期组织：

```
logs/
├── trace-2025-10-21.log      # 跟踪日志（包含所有详细信息）
├── debug-2025-10-21.log      # 调试日志（开发时使用）
├── info-2025-10-21.log       # 信息日志（常规操作记录）
├── warning-2025-10-21.log    # 警告日志（需要注意的问题）
├── error-2025-10-21.log      # 错误日志（可恢复的错误）
└── critical-2025-10-21.log   # 严重错误日志（系统崩溃等）
```

## 配置说明

### appsettings.json 配置

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "File": {
      "LogDirectory": "logs",
      "RetainedFileCountLimit": 30,
      "MinimumLevel": "Information"
    }
  }
}
```

### 配置参数说明

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `LogDirectory` | string | "logs" | 日志文件存储目录 |
| `RetainedFileCountLimit` | int | 30 | 保留的日志文件数量（0表示不限制） |
| `MinimumLevel` | string | "Information" | 最小日志级别 |

### 不同环境的配置

#### 开发环境 (appsettings.Development.json)
```json
{
  "Logging": {
    "File": {
      "LogDirectory": "logs",
      "RetainedFileCountLimit": 7,
      "MinimumLevel": "Debug"
    }
  }
}
```

#### 生产环境 (appsettings.Production.json)
```json
{
  "Logging": {
    "File": {
      "LogDirectory": "/var/log/llxserver",
      "RetainedFileCountLimit": 30,
      "MinimumLevel": "Information"
    }
  }
}
```

## 使用示例

### 1. 在控制器或服务中使用日志

```csharp
public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    private readonly IProductRepository _repository;

    public ProductService(
        ILogger<ProductService> logger,
        IProductRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        _logger.LogInformation("开始获取商品信息: {ProductId}", id);
        
        try
        {
            var product = await _repository.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("商品不存在: {ProductId}", id);
                return null;
            }
            
            _logger.LogInformation("成功获取商品信息: {ProductId}, 名称: {ProductName}", 
                id, product.Name);
            
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取商品信息失败: {ProductId}", id);
            throw;
        }
    }
}
```

### 2. 日志级别使用建议

#### Trace (跟踪)
用于最详细的调试信息，仅在开发时启用。

```csharp
_logger.LogTrace("开始执行方法: {MethodName}, 参数: {Parameters}", 
    nameof(CalculatePrice), new { quantity, price });
```

#### Debug (调试)
用于开发时的调试信息。

```csharp
_logger.LogDebug("缓存命中: Key={CacheKey}", cacheKey);
_logger.LogDebug("数据库查询: {Query}", query);
```

#### Information (信息)
用于记录常规操作流程。

```csharp
_logger.LogInformation("用户登录成功: {UserId}", userId);
_logger.LogInformation("订单创建成功: {OrderId}", orderId);
```

#### Warning (警告)
用于记录可能导致问题但不影响功能的情况。

```csharp
_logger.LogWarning("缓存未命中: {CacheKey}", cacheKey);
_logger.LogWarning("库存不足: ProductId={ProductId}, Required={Required}, Available={Available}", 
    productId, required, available);
```

#### Error (错误)
用于记录可恢复的错误。

```csharp
_logger.LogError(ex, "数据库查询失败: {Query}", query);
_logger.LogError("支付失败: OrderId={OrderId}, Reason={Reason}", 
    orderId, reason);
```

#### Critical (严重错误)
用于记录系统级的严重错误。

```csharp
_logger.LogCritical(ex, "数据库连接失败，系统无法正常运行");
_logger.LogCritical("磁盘空间不足: Available={Available}MB", availableSpace);
```

### 3. 结构化日志

使用结构化日志可以方便后续分析：

```csharp
// ✅ 推荐：使用结构化参数
_logger.LogInformation("订单创建: OrderId={OrderId}, UserId={UserId}, Amount={Amount}", 
    order.Id, order.UserId, order.Amount);

// ❌ 不推荐：字符串拼接
_logger.LogInformation($"订单创建: OrderId={order.Id}, UserId={order.UserId}");
```

### 4. 异常日志记录

```csharp
try
{
    // 业务逻辑
    await ProcessOrderAsync(orderId);
}
catch (OrderNotFoundException ex)
{
    _logger.LogWarning(ex, "订单不存在: {OrderId}", orderId);
    throw;
}
catch (PaymentException ex)
{
    _logger.LogError(ex, "支付失败: {OrderId}, 原因: {Reason}", 
        orderId, ex.Message);
    throw;
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "处理订单时发生未预期的错误: {OrderId}", orderId);
    throw;
}
```

## 日志格式

### 文件日志格式
```
[2025-10-21 14:30:45.123] [INFO    ] LLX.Server.Services.ProductService: 成功获取商品信息: ProductId=1, Name=稻花香
[2025-10-21 14:30:46.456] [WARNING ] LLX.Server.Services.CacheService: 缓存未命中: Key=product:1
[2025-10-21 14:30:47.789] [ERROR   ] LLX.Server.Services.OrderService: 创建订单失败: OrderId=12345
Exception:
System.InvalidOperationException: 库存不足
   at LLX.Server.Services.OrderService.CreateAsync(CreateOrderDto dto)
```

### 格式说明
- `[时间戳]`: 精确到毫秒的时间戳
- `[日志级别]`: 8个字符宽度，左对齐
- `类别名称`: 完整的类名
- `消息`: 日志消息内容
- `异常`: 如果有异常，会完整输出异常堆栈

## 日志管理

### 查看日志

```bash
# 查看最新的信息日志
tail -f logs/info-2025-10-21.log

# 查看错误日志
tail -f logs/error-2025-10-21.log

# 搜索特定内容
grep "OrderId=12345" logs/info-2025-10-21.log

# 查看所有级别的日志
tail -f logs/*.log
```

### 日志清理

日志系统会自动清理超过保留数量的历史文件。也可以手动清理：

```bash
# 删除7天前的日志
find logs/ -name "*.log" -mtime +7 -delete

# 删除所有日志
rm -rf logs/*.log
```

### Docker 环境日志

在 Docker 中，建议将日志目录映射到主机：

```yaml
services:
  api:
    volumes:
      - ./logs:/app/logs
```

这样可以在主机上直接查看和管理日志文件。

## 性能考虑

1. **异步写入**: 日志写入采用异步方式，不会阻塞主线程
2. **缓冲写入**: 使用 `StreamWriter` 的自动刷新功能
3. **线程安全**: 使用锁机制确保并发安全
4. **最小开销**: 按日志级别过滤，避免不必要的格式化

## 最佳实践

1. **合理设置日志级别**: 生产环境使用 Information，开发环境使用 Debug
2. **避免敏感信息**: 不要记录密码、密钥等敏感信息
3. **使用结构化日志**: 便于后续分析和查询
4. **合理的日志保留策略**: 根据磁盘空间设置保留数量
5. **监控日志文件大小**: 防止单个文件过大影响性能
6. **定期备份重要日志**: 归档到长期存储

## 故障排查

### 日志没有生成

1. 检查日志目录是否有写入权限
2. 检查日志级别配置是否正确
3. 检查 `Program.cs` 中是否正确配置了 `AddFileLogger`

### 日志文件过大

1. 检查是否有大量重复日志
2. 降低日志级别（如从 Debug 改为 Information）
3. 减少 `RetainedFileCountLimit` 的值

### 找不到某些日志

1. 检查日志级别是否被过滤
2. 检查日期是否正确
3. 检查是否在正确的文件中查找（按级别分文件）

## 与 Serilog 的对比

| 特性 | 本系统 (Microsoft.Extensions.Logging) | Serilog |
|------|---------------------------------------|---------|
| **依赖** | .NET 内置，无需额外包 | 需要额外 NuGet 包 |
| **性能** | 轻量级，性能优异 | 功能丰富，稍重 |
| **配置** | 简单直观 | 灵活强大 |
| **扩展性** | 自定义 Provider | 丰富的 Sink |
| **学习成本** | 低 | 中等 |
| **适用场景** | 中小型项目 | 大型复杂项目 |

本项目选择 Microsoft.Extensions.Logging 是因为：
- ✅ 无需额外依赖，减少包体积
- ✅ 性能开销更小
- ✅ 满足项目需求
- ✅ 更好的与 .NET 生态集成

## 相关文档

- [Microsoft.Extensions.Logging 官方文档](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [日志最佳实践](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
- [.NET 性能优化指南](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/)

