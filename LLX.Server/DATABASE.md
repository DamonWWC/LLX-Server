# 数据库支持说明

本项目支持多种数据库，包括 PostgreSQL、SQL Server、MySQL 和 SQLite。

## 支持的数据库

| 数据库 | 提供程序 | 包名 | 版本 |
|--------|----------|------|------|
| PostgreSQL | Npgsql | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.0 |
| SQL Server | Microsoft | Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 |
| MySQL | Pomelo | Pomelo.EntityFrameworkCore.MySql | 8.0.0 |
| SQLite | Microsoft | Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 |

## 配置数据库

### 1. 修改配置文件

编辑 `appsettings.json` 或 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "你的连接字符串"
  },
  "Database": {
    "Provider": "PostgreSQL",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

### 2. 连接字符串示例

#### PostgreSQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100"
  },
  "Database": {
    "Provider": "PostgreSQL"
  }
}
```

#### SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "Database": {
    "Provider": "SqlServer"
  }
}
```

#### MySQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;"
  },
  "Database": {
    "Provider": "MySql"
  }
}
```

#### SQLite
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=llxrice.db"
  },
  "Database": {
    "Provider": "Sqlite"
  }
}
```

## 自动检测数据库类型

如果不指定 `Database:Provider`，系统会根据连接字符串自动检测数据库类型：

- 包含 `server=` 和 `port=` 且不包含 `sql server` → MySQL
- 包含 `server=` 或 `data source=` → SQL Server
- 包含 `data source=` 和 `.db` → SQLite
- 其他情况 → PostgreSQL

## 数据库迁移

### 创建迁移
```bash
# 确保已安装 EF Core 工具
dotnet tool install --global dotnet-ef

# 创建迁移
dotnet ef migrations add InitialCreate

# 更新数据库
dotnet ef database update
```

### 指定数据库提供程序
```bash
# 为特定数据库创建迁移
dotnet ef migrations add InitialCreate --context AppDbContext
```

## 数据库特定功能

### PostgreSQL
- 支持 JSON 数据类型
- 支持数组类型
- 支持全文搜索
- 支持窗口函数

### SQL Server
- 支持 JSON 数据类型
- 支持内存优化表
- 支持列存储索引
- 支持 Always Encrypted

### MySQL
- 支持 JSON 数据类型
- 支持全文搜索
- 支持分区表
- 支持存储过程

### SQLite
- 轻量级数据库
- 支持内存数据库
- 支持事务
- 支持外键约束

## 性能优化建议

### PostgreSQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;CommandTimeout=30"
  }
}
```

### SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;Command Timeout=30"
  }
}
```

### MySQL
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Timeout=30;Command Timeout=30"
  }
}
```

## 故障排除

### 常见问题

1. **连接超时**
   - 检查网络连接
   - 增加 CommandTimeout 值
   - 检查防火墙设置

2. **认证失败**
   - 检查用户名和密码
   - 检查数据库权限
   - 检查 SSL 配置

3. **迁移失败**
   - 检查数据库是否存在
   - 检查用户权限
   - 检查连接字符串格式

### 调试模式

在开发环境中启用详细日志：

```json
{
  "Database": {
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  },
  "Serilog": {
    "MinimumLevel": {
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

## 生产环境建议

1. **使用连接池**
2. **启用重试机制**
3. **配置超时时间**
4. **使用 SSL 连接**
5. **定期备份数据**
6. **监控性能指标**

## 示例项目

查看 `appsettings.Examples.json` 文件获取完整的配置示例。
