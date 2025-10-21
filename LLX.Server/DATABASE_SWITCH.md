# 数据库切换快速指南

本指南将帮助您快速切换项目使用的数据库。

## 🚀 快速切换步骤

### 1. 选择目标数据库

根据您的需求选择以下数据库之一：

- **PostgreSQL** (推荐) - 功能强大，性能优秀
- **SQL Server** - 企业级数据库，Windows 环境友好
- **MySQL** - 开源数据库，广泛使用
- **SQLite** - 轻量级数据库，适合开发和测试

### 2. 修改配置文件

编辑 `appsettings.json` 或 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "你的连接字符串"
  },
  "Database": {
    "Provider": "数据库类型"
  }
}
```

### 3. 数据库特定配置

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

### 4. 运行数据库迁移

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

### 5. 启动应用

```bash
dotnet run
```

## 🔧 高级配置

### 自动检测数据库类型

如果不指定 `Database:Provider`，系统会根据连接字符串自动检测：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "你的连接字符串"
  }
  // 不指定 Database:Provider，系统自动检测
}
```

### 开发环境配置

在 `appsettings.Development.json` 中配置开发环境特定的设置：

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true,
    "CommandTimeout": 30
  }
}
```

### 生产环境配置

在 `appsettings.Production.json` 中配置生产环境设置：

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

## 🧪 测试数据库连接

使用内置的数据库测试功能：

```csharp
// 测试连接
var isConnected = await DatabaseTest.TestConnectionAsync(connectionString, provider);

// 测试迁移
var migrationSuccess = await DatabaseTest.TestMigrationAsync(connectionString, provider);

// 测试 CRUD 操作
var crudSuccess = await DatabaseTest.TestCrudOperationsAsync(connectionString, provider);
```

## 📊 性能优化建议

### PostgreSQL
- 使用连接池
- 配置适当的超时时间
- 启用查询计划缓存

### SQL Server
- 使用 Always On 可用性组
- 配置列存储索引
- 启用查询存储

### MySQL
- 配置 InnoDB 缓冲池
- 使用读写分离
- 启用查询缓存

### SQLite
- 使用 WAL 模式
- 配置适当的页面大小
- 定期执行 VACUUM

## 🚨 常见问题

### 1. 连接失败
- 检查连接字符串格式
- 确认数据库服务正在运行
- 检查网络连接和防火墙设置

### 2. 迁移失败
- 确认数据库用户有足够权限
- 检查数据库是否存在
- 查看详细错误日志

### 3. 性能问题
- 检查连接池配置
- 优化查询语句
- 添加适当的索引

### 4. 数据类型不兼容
- 检查 decimal 类型配置
- 确认字符串长度限制
- 验证日期时间格式

## 📚 更多资源

- [Entity Framework Core 文档](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL 文档](https://www.postgresql.org/docs/)
- [SQL Server 文档](https://docs.microsoft.com/en-us/sql/)
- [MySQL 文档](https://dev.mysql.com/doc/)
- [SQLite 文档](https://www.sqlite.org/docs.html)

## 🆘 获取帮助

如果遇到问题，请：

1. 查看应用日志
2. 检查数据库日志
3. 参考 [DATABASE.md](DATABASE.md) 详细文档
4. 联系技术支持团队
