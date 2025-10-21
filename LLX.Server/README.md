# 林龍香大米商城 - 后端服务

基于 .NET 8 Minimal API 的现代化后端服务，为微信小程序"林龍香大米商城"提供完整的 API 支持。

## 🚀 快速开始

### 环境要求
- .NET 8 SDK
- PostgreSQL 16
- Redis 7.2

### 运行项目
```bash
# 克隆项目
git clone <repository-url>
cd LLX.Server

# 恢复依赖
dotnet restore

# 配置数据库连接字符串
# 编辑 appsettings.json 中的 ConnectionStrings

# 运行项目
dotnet run
```

### 访问 API 文档
- Swagger UI: http://localhost:5000/swagger
- 健康检查: http://localhost:5000/health

## 📁 项目结构

```
LLX.Server/
├── Models/
│   ├── Entities/          # 数据库实体
│   │   ├── Product.cs
│   │   ├── Address.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   └── ShippingRate.cs
│   └── DTOs/             # 数据传输对象
│       ├── ApiResponse.cs
│       ├── ProductDtos.cs
│       ├── AddressDtos.cs
│       ├── OrderDtos.cs
│       └── ShippingDtos.cs
├── Data/
│   └── AppDbContext.cs   # EF Core 上下文
├── Services/             # 业务逻辑层
│   ├── ICacheService.cs
│   └── RedisCacheService.cs
├── Repositories/         # 数据访问层
├── Endpoints/           # Minimal API 端点
├── Utils/               # 工具类
│   ├── OrderNumberGenerator.cs
│   └── AddressParser.cs
├── Middleware/          # 中间件
│   ├── ExceptionMiddleware.cs
│   └── LoggingMiddleware.cs
├── Extensions/          # 扩展方法
│   ├── ServiceCollectionExtensions.cs
│   └── EndpointRouteBuilderExtensions.cs
├── Program.cs           # 应用入口
├── appsettings.json     # 配置文件
├── appsettings.Examples.json  # 数据库配置示例
├── DATABASE.md          # 数据库支持说明
├── DATABASE_SWITCH.md   # 数据库切换指南
├── DatabaseTest.cs      # 数据库测试工具
└── .cursorrules         # Cursor AI 开发规则
```

## 🛠️ 技术栈

- **后端框架**: .NET 8 Minimal API
- **数据库**: 支持多种数据库
  - PostgreSQL 16 (推荐)
  - SQL Server 2019+
  - MySQL 8.0+
  - SQLite 3.x
- **缓存**: Redis 7.2
- **ORM**: Entity Framework Core 8
- **日志**: Serilog
- **API 文档**: Swagger/OpenAPI

## 📋 功能特性

- ✅ 商品管理（CRUD）
- ✅ 地址管理（含智能识别）
- ✅ 订单管理（完整流程）
- ✅ 运费计算（动态计算）
- ✅ Redis 缓存优化
- ✅ 健康检查监控
- ✅ 结构化日志记录
- ✅ 全局异常处理

## 🔧 配置说明

### 数据库配置
项目支持多种数据库，请根据实际使用的数据库选择对应的配置：

#### PostgreSQL (推荐)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password"
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
    "DefaultConnection": "Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true"
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

### Redis 配置
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=your_redis_password"
  }
}
```

### 自动检测
如果不指定 `Database:Provider`，系统会根据连接字符串自动检测数据库类型。

详细配置说明请参考 [DATABASE.md](DATABASE.md)。

## 🗄️ 多数据库支持

本项目支持多种数据库，包括 PostgreSQL、SQL Server、MySQL 和 SQLite。

### 快速切换数据库
```bash
# 切换到 PostgreSQL
.\scripts\switch-database.ps1 -DatabaseType PostgreSQL -ConnectionString "your_connection_string"

# 切换到 SQL Server
.\scripts\switch-database.ps1 -DatabaseType SqlServer -ConnectionString "your_connection_string"

# 切换到 MySQL
.\scripts\switch-database.ps1 -DatabaseType MySql -ConnectionString "your_connection_string"

# 切换到 SQLite
.\scripts\switch-database.ps1 -DatabaseType Sqlite -ConnectionString "your_connection_string"
```

### 数据库管理工具
- **迁移管理**: `.\scripts\manage-migrations.ps1`
- **性能测试**: `.\scripts\benchmark-databases.ps1`
- **健康检查**: `.\scripts\check-database-health.ps1`
- **备份恢复**: `.\scripts\backup-restore.ps1`

详细说明请参考 [DATABASE.md](DATABASE.md) 和 [MULTI_DATABASE_SUPPORT.md](MULTI_DATABASE_SUPPORT.md)。

## 📚 API 文档

### 商品管理
- `GET /api/products` - 获取所有商品
- `GET /api/products/{id}` - 获取单个商品
- `POST /api/products` - 创建商品
- `PUT /api/products/{id}` - 更新商品
- `DELETE /api/products/{id}` - 删除商品

### 地址管理
- `GET /api/addresses` - 获取所有地址
- `POST /api/addresses` - 创建地址
- `POST /api/addresses/parse` - 智能识别地址

### 订单管理
- `GET /api/orders` - 获取订单列表
- `POST /api/orders` - 创建订单
- `PUT /api/orders/{id}/status` - 更新订单状态

### 运费计算
- `POST /api/shipping/calculate` - 计算运费

## 🐳 Docker 部署

```bash
# 构建镜像
docker build -t llx-server .

# 运行容器
docker run -p 8080:8080 llx-server
```

## 📝 开发规范

项目遵循严格的代码规范，详见 [.cursorrules](.cursorrules) 文件。

## 🤝 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 📞 联系方式

- 项目链接: [https://github.com/your-username/llx-server](https://github.com/your-username/llx-server)
- 问题反馈: [Issues](https://github.com/your-username/llx-server/issues)

---

**版本**: v1.0  
**最后更新**: 2025-01-17
