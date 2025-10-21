# 数据库切换完整示例

本文件提供了在不同数据库之间切换的完整示例和最佳实践。

## 🚀 快速开始

### 1. PostgreSQL 示例

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100"
  },
  "Database": {
    "Provider": "PostgreSQL",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

### 2. SQL Server 示例

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;Command Timeout=30"
  },
  "Database": {
    "Provider": "SqlServer",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

### 3. MySQL 示例

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Timeout=30;Command Timeout=30"
  },
  "Database": {
    "Provider": "MySql",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

### 4. SQLite 示例

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=llxrice.db;Cache=Shared"
  },
  "Database": {
    "Provider": "Sqlite",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

## 🔧 环境特定配置

### 开发环境 (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice_dev;Username=llxrice_user;Password=dev_password;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=10"
  },
  "Database": {
    "Provider": "PostgreSQL",
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true,
    "CommandTimeout": 30
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Information",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

### 生产环境 (appsettings.Production.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db.example.com;Port=5432;Database=llxrice;Username=llxrice_user;Password=prod_password;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;SSL Mode=Require"
  },
  "Database": {
    "Provider": "PostgreSQL",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

## 🧪 测试和验证

### 1. 连接测试

```csharp
// 测试数据库连接
var isConnected = await DatabaseTest.TestConnectionAsync(
    "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass",
    DatabaseProvider.PostgreSQL
);

if (isConnected)
{
    Console.WriteLine("✅ 数据库连接成功!");
}
else
{
    Console.WriteLine("❌ 数据库连接失败!");
}
```

### 2. 迁移测试

```csharp
// 测试数据库迁移
var migrationSuccess = await DatabaseTest.TestMigrationAsync(
    "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass",
    DatabaseProvider.PostgreSQL
);

if (migrationSuccess)
{
    Console.WriteLine("✅ 数据库迁移成功!");
}
else
{
    Console.WriteLine("❌ 数据库迁移失败!");
}
```

### 3. CRUD 操作测试

```csharp
// 测试 CRUD 操作
var crudSuccess = await DatabaseTest.TestCrudOperationsAsync(
    "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass",
    DatabaseProvider.PostgreSQL
);

if (crudSuccess)
{
    Console.WriteLine("✅ CRUD 操作测试成功!");
}
else
{
    Console.WriteLine("❌ CRUD 操作测试失败!");
}
```

### 4. 性能测试

```csharp
// 运行性能测试
using var context = new AppDbContext(options);
var performanceTest = new DatabasePerformanceTest(context, DatabaseProvider.PostgreSQL);
var result = await performanceTest.RunFullTestAsync();

Console.WriteLine($"数据库提供程序: {result.DatabaseProvider}");
Console.WriteLine($"连接时间: {result.ConnectionTime.TotalMilliseconds}ms");
Console.WriteLine($"总体评分: {result.OverallScore}/100");
```

## 📊 性能优化建议

### PostgreSQL 优化

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Idle Lifetime=300;Connection Pruning Interval=10;Command Timeout=30;Timeout=15"
  }
}
```

### SQL Server 优化

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;Command Timeout=30;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Lifetime=300"
  }
}
```

### MySQL 优化

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Idle Timeout=300;Connection Lifetime=300;Connection Timeout=30;Command Timeout=30"
  }
}
```

### SQLite 优化

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=llxrice.db;Cache=Shared;Journal Mode=WAL;Synchronous=Normal;Temp Store=Memory;Page Size=4096"
  }
}
```

## 🔄 数据库切换工作流

### 1. 准备阶段

```bash
# 1. 备份当前数据库
pg_dump -h localhost -U llxrice_user llxrice > backup.sql

# 2. 安装目标数据库
# 例如：安装 MySQL
sudo apt-get install mysql-server

# 3. 创建目标数据库
mysql -u root -p -e "CREATE DATABASE llxrice;"
```

### 2. 配置阶段

```bash
# 1. 更新配置文件
./scripts/switch-database.sh MySQL "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;"

# 2. 测试连接
./scripts/test-connection.ps1 MySQL "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;"
```

### 3. 迁移阶段

```bash
# 1. 创建迁移
./scripts/migrate-database.ps1 MySQL "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;" InitialCreate

# 2. 验证迁移
dotnet ef database update --context AppDbContext
```

### 4. 验证阶段

```bash
# 1. 运行应用
dotnet run

# 2. 测试 API
curl http://localhost:5000/health

# 3. 运行性能测试
dotnet run --project . --performance-test
```

## 🚨 故障排除

### 常见问题及解决方案

#### 1. 连接超时

**问题**: 数据库连接超时

**解决方案**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass;Connection Timeout=60;Command Timeout=60"
  }
}
```

#### 2. 认证失败

**问题**: 数据库认证失败

**解决方案**:
- 检查用户名和密码
- 确认数据库用户权限
- 检查 SSL 配置

#### 3. 迁移失败

**问题**: 数据库迁移失败

**解决方案**:
```bash
# 删除现有迁移
dotnet ef migrations remove

# 重新创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

#### 4. 性能问题

**问题**: 数据库性能不佳

**解决方案**:
- 优化连接字符串
- 添加适当的索引
- 调整连接池设置
- 使用查询优化

## 📚 最佳实践

### 1. 配置管理

- 使用环境变量存储敏感信息
- 为不同环境创建不同的配置文件
- 定期轮换数据库密码

### 2. 连接管理

- 使用连接池
- 设置适当的超时时间
- 监控连接使用情况

### 3. 迁移管理

- 为每个数据库创建独立的迁移
- 测试迁移脚本
- 备份数据

### 4. 监控和日志

- 启用详细日志
- 监控数据库性能
- 设置告警

## 🎯 总结

通过本示例，您可以：

1. 快速切换不同数据库
2. 测试数据库连接和性能
3. 管理数据库迁移
4. 优化数据库配置
5. 解决常见问题

记住始终在生产环境切换前进行充分测试！
