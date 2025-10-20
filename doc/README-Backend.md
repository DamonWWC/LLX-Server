# 林龍香大米商城 - 后端服务

<div align="center">

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=.net)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)
![Redis](https://img.shields.io/badge/Redis-7.2-DC382D?style=flat-square&logo=redis)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat-square&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

**为微信小程序"林龍香大米商城"提供强大的 RESTful API 支持**

[功能特性](#功能特性) • [快速开始](#快速开始) • [API 文档](#api-文档) • [部署指南](#部署指南) • [开发规范](#开发规范)

</div>

---

## 📖 项目简介

这是一个基于 **.NET 8 Minimal API** 构建的现代化后端服务，为微信小程序"林龍香大米商城"提供完整的 API 支持。采用最新的技术栈，提供高性能、可扩展、易维护的后端解决方案。

### 为什么选择这个方案？

✅ **高性能**: Minimal API 比传统 Controller 性能提升 20%+  
✅ **现代化**: 采用 .NET 8 最新特性和最佳实践  
✅ **易部署**: Docker 一键部署，支持多种部署方式  
✅ **易维护**: 清晰的分层架构，代码职责明确  
✅ **生产就绪**: 包含日志、监控、健康检查等完整功能

---

## ✨ 功能特性

### 核心功能

- 🛍️ **商品管理**: 完整的 CRUD 操作，支持图片上传
- 📍 **地址管理**: 智能地址识别，自动解析省市区
- 📦 **订单管理**: 订单创建、状态管理、批量操作
- 🚚 **运费计算**: 基于省份和重量的动态运费计算
- 🔐 **鉴权支持**: 预留 JWT 鉴权接口，可快速扩展

### 技术亮点

- ⚡ **缓存优化**: Redis 缓存热点数据，响应速度快
- 📊 **结构化日志**: Serilog 日志记录，便于分析和查询
- 🔍 **健康检查**: 内置健康检查端点，便于监控
- 📝 **API 文档**: Swagger/OpenAPI 自动生成文档
- 🐳 **容器化**: Docker Compose 一键部署

---

## 🛠️ 技术栈

| 技术 | 版本 | 说明 |
|------|------|------|
| [.NET](https://dotnet.microsoft.com/) | 8.0 | 后端框架 |
| [ASP.NET Core](https://docs.microsoft.com/aspnet/core/) | 8.0 | Web API 框架 |
| [Entity Framework Core](https://docs.microsoft.com/ef/core/) | 8.0 | ORM 框架 |
| [PostgreSQL](https://www.postgresql.org/) | 16 | 关系数据库 |
| [Redis](https://redis.io/) | 7.2 | 缓存服务 |
| [Serilog](https://serilog.net/) | 3.1 | 结构化日志 |
| [Docker](https://www.docker.com/) | 20.10+ | 容器化部署 |

---

## 🚀 快速开始

### 前置要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (推荐) 或 PostgreSQL 16 + Redis 7.2

### 方式一：Docker Compose 部署（推荐）

```bash
# 1. 克隆项目
git clone <repository-url>
cd LLXRice.Api

# 2. 配置环境变量
cat > .env << 'EOF'
DB_PASSWORD=your_strong_password
REDIS_PASSWORD=your_redis_password
EOF

# 3. 启动所有服务
docker-compose up -d

# 4. 查看服务状态
docker-compose ps

# 5. 访问 API 文档
# 浏览器打开: http://localhost:8080
```

就这么简单！🎉

### 方式二：本地开发

```bash
# 1. 安装依赖
cd src/LLXRice.Api
dotnet restore

# 2. 配置数据库连接
# 编辑 appsettings.Development.json

# 3. 应用数据库迁移
dotnet ef database update

# 4. 启动应用
dotnet run
```

---

## 📚 API 文档

### Swagger 文档

启动服务后，访问 Swagger 文档:

- **本地**: http://localhost:8080
- **生产**: https://api.llxrice.com

### API 端点概览

#### 商品管理

```http
GET    /api/products         # 获取所有商品
GET    /api/products/{id}    # 获取单个商品
POST   /api/products         # 创建商品
PUT    /api/products/{id}    # 更新商品
DELETE /api/products/{id}    # 删除商品
```

#### 地址管理

```http
GET    /api/addresses           # 获取所有地址
GET    /api/addresses/{id}      # 获取单个地址
POST   /api/addresses           # 创建地址
PUT    /api/addresses/{id}      # 更新地址
DELETE /api/addresses/{id}      # 删除地址
POST   /api/addresses/parse     # 智能识别地址
PUT    /api/addresses/{id}/set-default  # 设置默认地址
```

#### 订单管理

```http
GET    /api/orders                # 获取订单列表（分页）
GET    /api/orders/{id}           # 获取订单详情
POST   /api/orders                # 创建订单
PUT    /api/orders/{id}/status    # 更新订单状态
DELETE /api/orders/{id}           # 删除订单
DELETE /api/orders/batch          # 批量删除订单
```

#### 运费计算

```http
POST   /api/shipping/calculate    # 计算运费
GET    /api/shipping/rates        # 获取运费配置
PUT    /api/shipping/rates/{province}  # 更新运费配置
```

### 请求示例

#### 创建商品

```bash
curl -X POST "http://localhost:8080/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "稻花香",
    "price": 40.00,
    "unit": "袋",
    "weight": 10.00,
    "image": "data:image/svg+xml,..."
  }'
```

响应:

```json
{
  "success": true,
  "message": "创建成功",
  "data": {
    "id": 1,
    "name": "稻花香",
    "price": 40.00,
    "unit": "袋",
    "weight": 10.00,
    "createdAt": "2025-10-17T10:30:00Z"
  },
  "timestamp": "2025-10-17T10:30:00Z"
}
```

#### 智能识别地址

```bash
curl -X POST "http://localhost:8080/api/addresses/parse" \
  -H "Content-Type: application/json" \
  -d '{
    "text": "张三 13800138000 广东省深圳市南山区科技园南路1号"
  }'
```

响应:

```json
{
  "success": true,
  "message": "识别成功",
  "data": {
    "name": "张三",
    "phone": "13800138000",
    "province": "广东省",
    "city": "深圳市",
    "district": "南山区",
    "detail": "科技园南路1号"
  }
}
```

---

## 🏗️ 项目结构

```
LLXRice.Api/
├── src/
│   └── LLXRice.Api/              # 主项目
│       ├── Program.cs             # 应用入口
│       ├── Models/
│       │   ├── Entities/          # 数据库实体
│       │   └── DTOs/              # 数据传输对象
│       ├── Data/
│       │   ├── AppDbContext.cs    # EF Core 上下文
│       │   └── Migrations/        # 数据库迁移
│       ├── Services/              # 业务逻辑层
│       ├── Repositories/          # 数据访问层
│       ├── Endpoints/             # API 端点
│       ├── Utils/                 # 工具类
│       └── Middleware/            # 中间件
├── scripts/
│   ├── 数据库初始化脚本.sql       # 数据库初始化
│   ├── deploy-docker.sh           # Docker 部署脚本
│   └── deploy-service.sh          # Systemd 部署脚本
├── docker-compose.yml             # Docker Compose 配置
├── Dockerfile                     # Docker 镜像配置
├── .cursorrules                   # Cursor AI 开发规则
├── 后端服务设计方案.md            # 详细设计文档
├── DEPLOYMENT.md                  # 部署指南
└── README.md                      # 项目说明
```

---

## 🐳 部署指南

### Docker 部署（推荐）

详细步骤请参考 [DEPLOYMENT.md](./DEPLOYMENT.md)

**快速部署**:

```bash
# 1. 配置环境变量
echo "DB_PASSWORD=your_password" > .env
echo "REDIS_PASSWORD=your_redis_password" >> .env

# 2. 启动服务
docker-compose up -d

# 3. 查看日志
docker-compose logs -f

# 4. 健康检查
curl http://localhost:8080/health
```

### Linux 服务部署

支持 Ubuntu、CentOS、Debian 等主流 Linux 发行版。

```bash
# 安装运行时
sudo apt install -y aspnetcore-runtime-8.0

# 发布应用
dotnet publish -c Release -o /var/www/llxrice-api

# 创建 Systemd 服务
sudo systemctl enable llxrice-api
sudo systemctl start llxrice-api
```

详细步骤请参考 [DEPLOYMENT.md](./DEPLOYMENT.md#systemd-服务部署)

---

## 📖 开发指南

### 开发环境设置

```bash
# 1. 安装 .NET 8 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# 2. 安装数据库
# PostgreSQL: https://www.postgresql.org/download/
# Redis: https://redis.io/download

# 3. 克隆项目
git clone <repository-url>
cd LLXRice.Api

# 4. 恢复依赖
dotnet restore

# 5. 配置数据库连接
# 编辑 src/LLXRice.Api/appsettings.Development.json

# 6. 应用数据库迁移
cd src/LLXRice.Api
dotnet ef database update

# 7. 启动应用
dotnet run

# 8. 访问 Swagger
# http://localhost:5000
```

### 使用 Cursor AI 开发

项目包含 `.cursorrules` 文件，使用 [Cursor](https://cursor.sh/) 编辑器可以获得智能代码提示和自动补全。

**主要特性**:
- ✅ 自动遵循项目代码规范
- ✅ 智能生成符合架构的代码
- ✅ 自动添加日志和错误处理
- ✅ 提供最佳实践建议

### 数据库迁移

```bash
# 添加新迁移
dotnet ef migrations add MigrationName

# 应用迁移
dotnet ef database update

# 回滚迁移
dotnet ef database update PreviousMigration

# 删除最后一次迁移
dotnet ef migrations remove

# 生成 SQL 脚本
dotnet ef migrations script
```

---

## 📝 开发规范

### 代码规范

项目遵循严格的代码规范，详见 [.cursorrules](./.cursorrules)

**关键原则**:
- ✅ 所有 I/O 操作必须使用异步方法
- ✅ 统一使用 `ApiResponse<T>` 响应格式
- ✅ 使用结构化日志记录关键操作
- ✅ 服务层处理业务逻辑，端点层只负责路由
- ✅ 使用 Redis 缓存热点数据

### Git 提交规范

```bash
# 格式: <type>(<scope>): <subject>

feat(products): 添加商品分页查询功能
fix(orders): 修复订单金额计算错误
docs(api): 更新 API 文档
refactor(services): 重构商品服务
perf(cache): 优化 Redis 缓存策略
test(orders): 添加订单服务单元测试
```

---

## 🧪 测试

### 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "FullyQualifiedName~ProductService"

# 生成测试覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 监控和日志

### 健康检查

```bash
# 检查服务健康状态
curl http://localhost:8080/health
```

### 日志查看

```bash
# Docker 部署
docker-compose logs -f api

# Systemd 部署
sudo journalctl -u llxrice-api -f

# 应用日志文件
tail -f logs/log-$(date +%Y%m%d).txt
```

### 性能监控

推荐使用以下工具:
- **Prometheus + Grafana**: 指标监控
- **ELK Stack**: 日志聚合分析
- **Application Insights**: Azure 应用监控

---

## 🔧 配置说明

### 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `ASPNETCORE_ENVIRONMENT` | 环境名称 | Production |
| `ASPNETCORE_URLS` | 监听地址 | http://+:8080 |
| `ConnectionStrings__DefaultConnection` | PostgreSQL 连接串 | - |
| `ConnectionStrings__Redis` | Redis 连接串 | - |

### 配置文件

- `appsettings.json`: 通用配置
- `appsettings.Development.json`: 开发环境配置
- `appsettings.Production.json`: 生产环境配置

---

## 🤝 小程序对接

### 1. 配置合法域名

在微信公众平台配置服务器域名:

```
request合法域名: https://api.llxrice.com
```

### 2. 使用 HTTP 客户端

小程序中使用提供的 HTTP 客户端封装:

```javascript
const ApiService = require('../../utils/apiService.js')

// 获取商品列表
const products = await ApiService.getProducts()

// 创建订单
const order = await ApiService.createOrder({
  addressId: 1,
  items: [
    { productId: 1, quantity: 2 }
  ]
})
```

详细对接说明请参考 [后端服务设计方案.md](./后端服务设计方案.md#小程序对接说明)

---

## 🔐 安全性

- ✅ 使用 HTTPS 加密传输
- ✅ 密码使用环境变量配置
- ✅ 输入验证和参数化查询
- ✅ CORS 白名单配置
- ✅ 定期更新依赖包

---

## 📈 性能优化

- ✅ Redis 缓存热点数据
- ✅ 数据库连接池优化
- ✅ 异步编程提升并发
- ✅ 分页查询避免大数据量
- ✅ 使用 AsNoTracking 优化只读查询

---

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](./LICENSE) 文件。

---

## 🙏 致谢

感谢以下开源项目:
- [.NET](https://github.com/dotnet/runtime)
- [Entity Framework Core](https://github.com/dotnet/efcore)
- [PostgreSQL](https://www.postgresql.org/)
- [Redis](https://redis.io/)
- [Serilog](https://github.com/serilog/serilog)

---

## 📮 联系方式

- 📧 Email: your-email@example.com
- 🌐 Website: https://llxrice.com
- 💬 Issues: [GitHub Issues](https://github.com/your-username/llxrice-api/issues)

---

## 🗺️ Roadmap

- [ ] 添加用户认证（JWT）
- [ ] 实现订单支付功能
- [ ] 添加商品分类管理
- [ ] 实现商品搜索功能
- [ ] 添加数据统计和报表
- [ ] 实现消息推送功能
- [ ] 集成短信通知
- [ ] 添加优惠券系统

---

<div align="center">

**如果这个项目对您有帮助，请给一个 ⭐ Star！**

Made with ❤️ by LLXRice Team

</div>

---

**文档版本**: v1.0  
**最后更新**: 2025-10-17

---

© 2025 林龍香大米商城. All rights reserved.

