# 林龍香大米商城 - API服务独立部署指南

## 📋 文档说明

本文档专门针对**仅部署API服务**的场景，适用于以下情况：
- PostgreSQL数据库已经在其他服务器或容器中运行
- Redis缓存已经在其他服务器或容器中运行
- 只需要部署或更新API服务

---

## 🎯 部署前提条件

### 1. 外部服务要求

#### PostgreSQL 数据库
- **版本**: PostgreSQL 12+ (推荐 16+)
- **状态**: 必须正在运行且可通过网络访问
- **要求**:
  - 数据库已创建（数据库名：llxrice）
  - 用户已创建并具有足够权限
  - 防火墙已开放PostgreSQL端口（默认5432）
  
#### Redis 缓存
- **版本**: Redis 6+ (推荐 7.2+)
- **状态**: 必须正在运行且可通过网络访问
- **要求**:
  - Redis服务可访问
  - 如果设置了密码，需要知道密码
  - 防火墙已开放Redis端口（默认6379）

### 2. 本地环境要求

#### 必需软件
- **Docker Desktop**: 20.10.0+
  - Windows: Docker Desktop for Windows
  - Linux: Docker Engine + Docker Compose
  - macOS: Docker Desktop for Mac

#### 硬件要求
- **最低配置**: 2核CPU, 4GB内存, 10GB存储
- **推荐配置**: 4核CPU, 8GB内存, 20GB存储

---

## 📦 部署文件清单

在开始部署前，请确保以下文件存在：

```
LLX.Server/
├── docker-compose.api-only.yml   # API服务Docker Compose配置
├── env.api-only.example           # 环境变量配置示例
├── LLX.Server/
│   ├── Dockerfile                 # Docker镜像构建文件
│   └── scripts/
│       └── deploy-api-only.ps1    # 部署脚本
└── ...
```

---

## 🚀 快速部署步骤

### 步骤1: 准备环境变量配置

```powershell
# 1. 进入项目根目录
cd E:\资料\学习代码\LLX.Server

# 2. 复制环境变量示例文件
copy env.api-only.example .env

# 3. 编辑环境变量文件
notepad .env
```

### 步骤2: 配置数据库和Redis连接

编辑 `.env` 文件，修改以下关键配置：

```bash
# 数据库连接配置
# 格式：Host={IP地址或主机名};Port={端口};Database={数据库名};Username={用户名};Password={密码};其他选项
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=MySecurePassword123;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# Redis连接配置
# 格式：{IP地址或主机名}:{端口},password={密码},ssl={是否SSL},abortConnect=false
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=MyRedisPassword123,ssl=false,abortConnect=false
```

### 步骤3: 运行部署脚本

```powershell
# 运行部署脚本（推荐）
.\LLX.Server\scripts\deploy-api-only.ps1

# 或者强制重新构建
.\LLX.Server\scripts\deploy-api-only.ps1 -Build -Force
```

### 步骤4: 验证部署

部署完成后，访问以下地址验证：

- **健康检查**: http://localhost:8080/health
- **Swagger文档**: http://localhost:8080/swagger
- **API测试**: http://localhost:8080/api/products

---

## 📝 详细部署步骤

### 方式一：使用自动化部署脚本（推荐）

#### 1. 查看帮助信息

```powershell
.\LLX.Server\scripts\deploy-api-only.ps1 -Help
```

#### 2. 标准部署

```powershell
# 进入项目根目录
cd E:\资料\学习代码\LLX.Server

# 运行部署脚本
.\LLX.Server\scripts\deploy-api-only.ps1
```

脚本会自动执行以下步骤：
1. ✅ 检查Docker是否安装和运行
2. ✅ 检查是否在正确的目录
3. ✅ 检查环境变量文件是否存在
4. ✅ 验证数据库和Redis配置
5. ✅ 停止现有容器（如果存在）
6. ✅ 编译.NET项目
7. ✅ 构建Docker镜像
8. ✅ 启动API服务
9. ✅ 等待服务就绪
10. ✅ 显示服务状态

#### 3. 部署参数说明

```powershell
# 强制重新构建Docker镜像
.\LLX.Server\scripts\deploy-api-only.ps1 -Build

# 使用已有的构建文件，不重新编译
.\LLX.Server\scripts\deploy-api-only.ps1 -NoBuild

# 强制重新部署（停止并删除现有容器）
.\LLX.Server\scripts\deploy-api-only.ps1 -Force

# 组合使用
.\LLX.Server\scripts\deploy-api-only.ps1 -Build -Force
```

### 方式二：手动使用Docker Compose部署

#### 1. 编译.NET项目

```powershell
# 进入项目目录
cd LLX.Server

# 清理旧文件
dotnet clean --configuration Release

# 编译项目
dotnet build --configuration Release

# 返回根目录
cd ..
```

#### 2. 构建Docker镜像

```powershell
# 构建镜像
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .

# 查看镜像
docker images | findstr llxrice
```

#### 3. 启动服务

```powershell
# 使用docker-compose启动
docker-compose -f docker-compose.api-only.yml up -d

# 查看容器状态
docker ps

# 查看日志
docker logs -f llxrice_api
```

### 方式三：直接使用Docker命令部署

```powershell
# 1. 停止并删除旧容器
docker stop llxrice_api 2>$null
docker rm llxrice_api 2>$null

# 2. 运行新容器
docker run -d `
  --name llxrice_api `
  --restart unless-stopped `
  -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e "ConnectionStrings__DefaultConnection=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=MyPassword;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100" `
  -e "ConnectionStrings__Redis=192.168.1.101:6379,password=MyRedisPassword,ssl=false,abortConnect=false" `
  -v ${PWD}/logs:/app/logs `
  llxrice/api:latest

# 3. 查看容器状态
docker ps

# 4. 查看日志
docker logs -f llxrice_api
```

---

## ⚙️ 环境变量配置详解

### 必需配置

#### 1. 数据库连接字符串 (DB_CONNECTION_STRING)

**格式说明**:
```
Host={主机地址};Port={端口};Database={数据库名};Username={用户名};Password={密码};Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30
```

**配置示例**:

```bash
# 本地数据库
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=llxrice;Username=postgres;Password=postgres;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# 局域网内数据库
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=MySecurePassword123;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# 云数据库（AWS RDS示例）
DB_CONNECTION_STRING=Host=mydb.abc123.us-east-1.rds.amazonaws.com;Port=5432;Database=llxrice;Username=admin;Password=MyCloudDbPassword;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30;SSL Mode=Require
```

**参数说明**:
- `Host`: 数据库服务器地址（IP地址或域名）
- `Port`: 数据库端口（PostgreSQL默认5432）
- `Database`: 数据库名称
- `Username`: 数据库用户名
- `Password`: 数据库密码
- `Pooling`: 是否启用连接池（建议true）
- `Minimum Pool Size`: 最小连接数（建议5）
- `Maximum Pool Size`: 最大连接数（建议100）
- `Command Timeout`: 命令超时时间（秒，建议30）

#### 2. Redis连接字符串 (REDIS_CONNECTION_STRING)

**格式说明**:
```
{主机地址}:{端口},password={密码},ssl={是否SSL},abortConnect=false
```

**配置示例**:

```bash
# 本地Redis（无密码）
REDIS_CONNECTION_STRING=localhost:6379,ssl=false,abortConnect=false

# 本地Redis（有密码）
REDIS_CONNECTION_STRING=localhost:6379,password=MyRedisPassword,ssl=false,abortConnect=false

# 局域网内Redis
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=MySecureRedisPassword,ssl=false,abortConnect=false

# 云Redis（AWS ElastiCache示例，使用SSL）
REDIS_CONNECTION_STRING=myredis.abc123.0001.use1.cache.amazonaws.com:6379,password=MyCloudRedisPassword,ssl=true,abortConnect=false
```

**参数说明**:
- `主机地址:端口`: Redis服务器地址和端口（默认6379）
- `password`: Redis密码（如果设置了的话）
- `ssl`: 是否使用SSL连接（云服务通常需要）
- `abortConnect`: 连接失败时是否中止（建议false）

### 可选配置

#### 应用配置
```bash
# 应用环境
ASPNETCORE_ENVIRONMENT=Production

# API端口
API_PORT=8080
```

#### 性能配置
```bash
# 慢请求阈值（毫秒）
PERFORMANCE_SLOW_REQUEST_THRESHOLD=1000

# 日志级别
LOG_LEVEL=Information
```

#### 资源配置
```bash
# 内存限制
CONTAINER_MEMORY_LIMIT=512M
CONTAINER_MEMORY_RESERVATION=256M

# CPU限制
CONTAINER_CPU_LIMIT=0.5
CONTAINER_CPU_RESERVATION=0.25
```

---

## ✅ 部署验证

### 1. 检查容器状态

```powershell
# 查看容器是否运行
docker ps | findstr llxrice_api

# 预期输出类似：
# llxrice_api   Up 2 minutes   0.0.0.0:8080->8080/tcp
```

### 2. 健康检查

```powershell
# 使用curl检查
curl http://localhost:8080/health

# 使用PowerShell检查
Invoke-RestMethod -Uri "http://localhost:8080/health"

# 预期响应：
# {
#   "status": "Healthy",
#   "checks": {
#     "database": "Healthy",
#     "redis": "Healthy"
#   }
# }
```

### 3. API测试

```powershell
# 测试获取商品列表
curl http://localhost:8080/api/products

# 使用PowerShell
Invoke-RestMethod -Uri "http://localhost:8080/api/products"
```

### 4. Swagger文档

在浏览器中访问：http://localhost:8080/swagger

### 5. 查看日志

```powershell
# 实时日志
docker logs -f llxrice_api

# 最后100行日志
docker logs --tail 100 llxrice_api

# 查看本地日志文件
cd logs
dir
notepad app-*.log
```

---

## 🔧 常见问题排查

### 问题1: 无法连接到数据库

**症状**: 
```
System.InvalidOperationException: An error occurred using the connection to database 'llxrice' on server 'xxx'
```

**排查步骤**:

1. 检查数据库服务是否运行
```powershell
# 如果数据库在Docker中
docker ps | findstr postgres

# 如果数据库是本地服务
Get-Service postgresql*
```

2. 测试网络连接
```powershell
# 测试端口是否开放
Test-NetConnection -ComputerName 192.168.1.100 -Port 5432
```

3. 验证连接字符串
```powershell
# 检查.env文件中的配置
Get-Content .env | Select-String "DB_CONNECTION_STRING"
```

4. 检查防火墙
```powershell
# Windows防火墙
Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*5432*"}
```

**解决方案**:
- 确保数据库服务正在运行
- 确保网络可达
- 检查用户名和密码是否正确
- 确保数据库已创建
- 检查防火墙规则

### 问题2: 无法连接到Redis

**症状**:
```
StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s)
```

**排查步骤**:

1. 检查Redis服务是否运行
```powershell
# 如果Redis在Docker中
docker ps | findstr redis

# 测试连接
Test-NetConnection -ComputerName 192.168.1.101 -Port 6379
```

2. 验证Redis密码
```bash
# 使用redis-cli测试
docker exec -it <redis-container> redis-cli
AUTH your_password
PING
```

**解决方案**:
- 确保Redis服务正在运行
- 确保网络可达
- 检查Redis密码是否正确
- 检查防火墙规则

### 问题3: 容器启动失败

**排查步骤**:

```powershell
# 查看容器状态
docker ps -a | findstr llxrice_api

# 查看容器日志
docker logs llxrice_api

# 查看详细信息
docker inspect llxrice_api
```

**常见原因**:
- 端口被占用
- 环境变量配置错误
- 镜像构建失败
- 内存不足

**解决方案**:
```powershell
# 检查端口占用
netstat -ano | findstr :8080

# 更换端口（修改.env文件）
API_PORT=8081

# 重新部署
.\LLX.Server\scripts\deploy-api-only.ps1 -Force
```

### 问题4: API响应慢

**排查步骤**:

```powershell
# 查看容器资源使用
docker stats llxrice_api

# 查看日志中的慢请求
docker logs llxrice_api | Select-String "Slow request"
```

**解决方案**:

1. 增加资源限制（修改.env）
```bash
CONTAINER_MEMORY_LIMIT=1G
CONTAINER_CPU_LIMIT=1.0
```

2. 优化数据库连接池
```bash
DB_MAX_POOL_SIZE=200
```

3. 检查网络延迟
```powershell
ping 192.168.1.100
```

### 问题5: 数据库迁移失败

**症状**:
```
Npgsql.PostgresException: 42P01: relation "Products" does not exist
```

**解决方案**:

1. 手动运行数据库迁移
```powershell
# 进入容器
docker exec -it llxrice_api bash

# 运行迁移
dotnet ef database update
```

2. 或者在启动前运行迁移
```powershell
cd LLX.Server
dotnet ef database update --connection "Host=192.168.1.100;Port=5432;Database=llxrice;Username=user;Password=pass"
cd ..
```

---

## 🔄 日常维护操作

### 查看服务状态

```powershell
# 查看容器状态
docker ps -a | findstr llxrice_api

# 查看容器详细信息
docker inspect llxrice_api

# 查看资源使用
docker stats llxrice_api
```

### 重启服务

```powershell
# 重启容器
docker restart llxrice_api

# 查看重启后的日志
docker logs -f llxrice_api
```

### 停止服务

```powershell
# 停止容器
docker stop llxrice_api

# 停止并删除容器
docker stop llxrice_api
docker rm llxrice_api
```

### 更新服务

```powershell
# 方法1: 使用部署脚本（推荐）
.\LLX.Server\scripts\deploy-api-only.ps1 -Build -Force

# 方法2: 手动更新
docker stop llxrice_api
docker rm llxrice_api
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .
docker-compose -f docker-compose.api-only.yml up -d
```

### 备份日志

```powershell
# 创建日志备份目录
mkdir logs_backup

# 备份日志文件
copy logs\*.log logs_backup\

# 压缩备份
Compress-Archive -Path logs_backup\* -DestinationPath "logs_backup_$(Get-Date -Format 'yyyyMMdd').zip"
```

### 清理旧日志

```powershell
# 删除7天前的日志
Get-ChildItem logs\*.log | Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | Remove-Item

# 清理Docker日志
docker system prune -f
```

---

## 📊 监控和告警

### 手动监控

```powershell
# 每30秒检查一次健康状态
while($true) {
    $response = Invoke-RestMethod -Uri "http://localhost:8080/health" -ErrorAction SilentlyContinue
    if($response.status -eq "Healthy") {
        Write-Host "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Service Healthy" -ForegroundColor Green
    } else {
        Write-Host "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Service Unhealthy" -ForegroundColor Red
    }
    Start-Sleep -Seconds 30
}
```

### 性能监控

```powershell
# 运行性能测试脚本
.\LLX.Server\scripts\test-api.ps1 -BaseUrl "http://localhost:8080"

# 查看性能日志
docker logs llxrice_api | Select-String "executed in"
```

---

## 🚨 紧急情况处理

### 服务崩溃

```powershell
# 1. 查看日志
docker logs --tail 500 llxrice_api > error.log

# 2. 重启服务
docker restart llxrice_api

# 3. 如果无法重启，强制重新部署
.\LLX.Server\scripts\deploy-api-only.ps1 -Force
```

### 数据库连接丢失

```powershell
# 1. 检查数据库状态
Test-NetConnection -ComputerName 192.168.1.100 -Port 5432

# 2. 重启API服务
docker restart llxrice_api

# 3. 检查连接配置
Get-Content .env | Select-String "DB_CONNECTION"
```

### 内存不足

```powershell
# 1. 查看内存使用
docker stats llxrice_api

# 2. 增加内存限制（修改.env）
# CONTAINER_MEMORY_LIMIT=1G

# 3. 重新部署
.\LLX.Server\scripts\deploy-api-only.ps1 -Force
```

---

## 📞 技术支持

### 获取帮助

- **查看部署脚本帮助**: `.\LLX.Server\scripts\deploy-api-only.ps1 -Help`
- **查看Docker日志**: `docker logs -f llxrice_api`
- **查看应用日志**: `logs/app-*.log`

### 常用资源

- **API文档**: http://localhost:8080/swagger
- **健康检查**: http://localhost:8080/health
- **项目文档**: `doc/`目录下的其他文档

---

## ✅ 部署检查清单

部署前请确认：

- [ ] PostgreSQL数据库已运行且可访问
- [ ] Redis缓存已运行且可访问
- [ ] Docker Desktop已安装并运行
- [ ] 项目代码已下载到本地
- [ ] `.env`文件已正确配置
- [ ] 数据库连接字符串正确
- [ ] Redis连接字符串正确
- [ ] 端口8080未被占用（或已修改端口配置）

部署后请验证：

- [ ] 容器正常运行 (`docker ps`)
- [ ] 健康检查通过 (`curl http://localhost:8080/health`)
- [ ] API可以访问 (`curl http://localhost:8080/api/products`)
- [ ] Swagger文档可访问 (`http://localhost:8080/swagger`)
- [ ] 日志正常输出 (`docker logs llxrice_api`)

---

## 🎉 总结

本指南提供了详细的API服务独立部署方案，包括：

1. ✅ **完整的前提条件说明**
2. ✅ **三种部署方式（脚本/Docker Compose/Docker命令）**
3. ✅ **详细的环境变量配置说明**
4. ✅ **完整的部署验证步骤**
5. ✅ **常见问题排查和解决方案**
6. ✅ **日常维护操作指南**
7. ✅ **监控和紧急情况处理**

按照本指南操作，您可以成功部署林龍香大米商城的API服务到任何已有PostgreSQL和Redis的环境中！
