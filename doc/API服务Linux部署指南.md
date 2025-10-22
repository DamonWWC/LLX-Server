# 林龍香大米商城 - API服务Linux部署指南

## 📋 文档说明

本文档专门针对**Linux系统下仅部署API服务**的场景，适用于以下情况：
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

### 2. Linux服务器要求

#### 系统要求
- **操作系统**: Ubuntu 20.04+, CentOS 8+, Debian 10+
- **架构**: x86_64 / amd64

#### 硬件要求
- **最低配置**: 2核CPU, 4GB内存, 10GB存储
- **推荐配置**: 4核CPU, 8GB内存, 20GB存储
- **生产环境**: 8核CPU, 16GB内存, 100GB存储

#### 必需软件
- **Docker**: 20.10.0+
- **Docker Compose**: 2.0.0+
- **Git**: 2.30.0+ (可选，用于克隆代码)

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
│       └── deploy-api-only.sh     # Linux部署脚本
└── ...
```

---

## 🚀 快速部署步骤

### 步骤1: 安装Docker和Docker Compose

#### Ubuntu/Debian系统
```bash
# 更新包索引
sudo apt-get update

# 安装Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# 安装Docker Compose
sudo apt-get install -y docker-compose

# 将当前用户添加到docker组
sudo usermod -aG docker $USER

# 启动Docker服务
sudo systemctl start docker
sudo systemctl enable docker

# 验证安装
docker --version
docker-compose --version
```

#### CentOS/RHEL系统
```bash
# 安装Docker
sudo yum install -y yum-utils
sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
sudo yum install -y docker-ce docker-ce-cli containerd.io

# 安装Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 启动Docker服务
sudo systemctl start docker
sudo systemctl enable docker

# 将当前用户添加到docker组
sudo usermod -aG docker $USER

# 验证安装
docker --version
docker-compose --version
```

**重要**: 添加用户到docker组后，需要重新登录以使权限生效：
```bash
# 重新登录或运行
newgrp docker
```

### 步骤2: 获取项目代码

```bash
# 如果使用Git克隆
git clone <repository-url>
cd LLX.Server

# 或者上传项目文件到服务器
# 使用scp、rsync或其他方式
```

### 步骤3: 配置环境变量

```bash
# 进入项目根目录
cd /path/to/LLX.Server

# 复制环境变量示例文件
cp env.api-only.example .env

# 编辑环境变量文件
nano .env
# 或使用 vim .env
```

### 步骤4: 配置数据库和Redis连接

编辑 `.env` 文件，修改以下关键配置：

```bash
# 数据库连接配置
# 格式：Host={IP地址或主机名};Port={端口};Database={库名};Username={用户名};Password={密码};其他选项
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=MySecurePassword123;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# Redis连接配置
# 格式：{IP地址或主机名}:{端口},password={密码},ssl={是否SSL},abortConnect=false
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=MyRedisPassword123,ssl=false,abortConnect=false
```

### 步骤5: 运行部署脚本

```bash
# 给脚本添加执行权限
chmod +x LLX.Server/scripts/deploy-api-only.sh

# 运行部署脚本
./LLX.Server/scripts/deploy-api-only.sh

# 或者强制重新构建
./LLX.Server/scripts/deploy-api-only.sh -b -f
```

### 步骤6: 验证部署

```bash
# 检查健康状态
curl http://localhost:8080/health

# 预期响应
# {"status":"Healthy","checks":{"database":"Healthy","redis":"Healthy"}}

# 访问Swagger文档
curl http://localhost:8080/swagger
```

---

## 📝 详细部署步骤

### 方式一：使用自动化部署脚本（推荐）

#### 1. 查看帮助信息

```bash
./LLX.Server/scripts/deploy-api-only.sh -h
```

#### 2. 标准部署

```bash
# 进入项目根目录
cd /path/to/LLX.Server

# 运行部署脚本
./LLX.Server/scripts/deploy-api-only.sh
```

脚本会自动执行以下步骤：
1. ✅ 检查Docker是否安装和运行
2. ✅ 检查Docker Compose是否安装
3. ✅ 检查是否在正确的目录
4. ✅ 检查环境变量文件是否存在
5. ✅ 验证数据库和Redis配置
6. ✅ 停止现有容器（如果存在）
7. ✅ 编译.NET项目
8. ✅ 构建Docker镜像
9. ✅ 启动API服务
10. ✅ 等待服务就绪
11. ✅ 显示服务状态

#### 3. 部署参数说明

```bash
# 强制重新构建Docker镜像
./LLX.Server/scripts/deploy-api-only.sh -b

# 使用已有的构建文件，不重新编译
./LLX.Server/scripts/deploy-api-only.sh -n

# 强制重新部署（停止并删除现有容器）
./LLX.Server/scripts/deploy-api-only.sh -f

# 组合使用
./LLX.Server/scripts/deploy-api-only.sh -b -f
```

### 方式二：手动使用Docker Compose部署

#### 1. 编译.NET项目

```bash
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

```bash
# 构建镜像
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .

# 查看镜像
docker images | grep llxrice
```

#### 3. 启动服务

```bash
# 使用docker-compose启动
docker-compose -f docker-compose.api-only.yml up -d

# 查看容器状态
docker ps

# 查看日志
docker logs -f llxrice_api
```

### 方式三：直接使用Docker命令部署

```bash
# 1. 停止并删除旧容器
docker stop llxrice_api 2>/dev/null || true
docker rm llxrice_api 2>/dev/null || true

# 2. 运行新容器
docker run -d \
  --name llxrice_api \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=MyPassword;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100" \
  -e "ConnectionStrings__Redis=192.168.1.101:6379,password=MyRedisPassword,ssl=false,abortConnect=false" \
  -v $(pwd)/logs:/app/logs \
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

---

## ✅ 部署验证

### 1. 检查容器状态

```bash
# 查看容器是否运行
docker ps | grep llxrice_api

# 预期输出类似：
# llxrice_api   Up 2 minutes   0.0.0.0:8080->8080/tcp
```

### 2. 健康检查

```bash
# 使用curl检查
curl http://localhost:8080/health

# 预期响应：
# {"status":"Healthy","checks":{"database":"Healthy","redis":"Healthy"}}

# 格式化输出
curl -s http://localhost:8080/health | python3 -m json.tool
```

### 3. API测试

```bash
# 测试获取商品列表
curl http://localhost:8080/api/products

# 格式化输出
curl -s http://localhost:8080/api/products | python3 -m json.tool
```

### 4. Swagger文档

```bash
# 在服务器上使用curl访问
curl http://localhost:8080/swagger

# 或在浏览器中访问
# http://服务器IP:8080/swagger
```

### 5. 查看日志

```bash
# 实时日志
docker logs -f llxrice_api

# 最后100行日志
docker logs --tail 100 llxrice_api

# 查看本地日志文件
cd logs
ls -lh
tail -f app-*.log
```

---

## 🔍 常见问题排查

### 问题1: 无法连接到数据库

**症状**: 
```
System.InvalidOperationException: An error occurred using the connection to database 'llxrice' on server 'xxx'
```

**排查步骤**:

1. 检查数据库服务是否运行
```bash
# 如果数据库在Docker中
docker ps | grep postgres

# 如果数据库是系统服务
sudo systemctl status postgresql
```

2. 测试网络连接
```bash
# 测试端口是否开放
telnet 192.168.1.100 5432

# 或使用nc
nc -zv 192.168.1.100 5432

# 或使用nmap
nmap -p 5432 192.168.1.100
```

3. 验证连接字符串
```bash
# 检查.env文件中的配置
grep "DB_CONNECTION_STRING" .env
```

4. 检查防火墙
```bash
# Ubuntu/Debian
sudo ufw status
sudo ufw allow 5432/tcp

# CentOS/RHEL
sudo firewall-cmd --list-all
sudo firewall-cmd --add-port=5432/tcp --permanent
sudo firewall-cmd --reload
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
```bash
# 如果Redis在Docker中
docker ps | grep redis

# 如果Redis是系统服务
sudo systemctl status redis

# 测试连接
telnet 192.168.1.101 6379
# 或
nc -zv 192.168.1.101 6379
```

2. 验证Redis密码
```bash
# 使用redis-cli测试
redis-cli -h 192.168.1.101 -p 6379
# 然后输入
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

```bash
# 查看容器状态
docker ps -a | grep llxrice_api

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
```bash
# 检查端口占用
sudo netstat -tulpn | grep :8080
# 或
sudo lsof -i :8080

# 杀死占用端口的进程
sudo kill -9 <PID>

# 更换端口（修改.env文件）
# API_PORT=8081

# 重新部署
./LLX.Server/scripts/deploy-api-only.sh -f
```

### 问题4: API响应慢

**排查步骤**:

```bash
# 查看容器资源使用
docker stats llxrice_api

# 查看日志中的慢请求
docker logs llxrice_api | grep "Slow request"

# 查看系统资源
top
htop
free -h
df -h
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
```bash
ping 192.168.1.100
traceroute 192.168.1.100
```

### 问题5: 权限问题

**症状**:
```
permission denied while trying to connect to the Docker daemon socket
```

**解决方案**:
```bash
# 将用户添加到docker组
sudo usermod -aG docker $USER

# 重新登录或运行
newgrp docker

# 或者使用sudo运行
sudo ./LLX.Server/scripts/deploy-api-only.sh
```

---

## 🔄 日常维护操作

### 查看服务状态

```bash
# 查看容器状态
docker ps -a | grep llxrice_api

# 查看容器详细信息
docker inspect llxrice_api

# 查看资源使用
docker stats llxrice_api

# 查看端口映射
docker port llxrice_api
```

### 重启服务

```bash
# 重启容器
docker restart llxrice_api

# 查看重启后的日志
docker logs -f llxrice_api
```

### 停止服务

```bash
# 停止容器
docker stop llxrice_api

# 停止并删除容器
docker stop llxrice_api
docker rm llxrice_api
```

### 更新服务

```bash
# 方法1: 使用部署脚本（推荐）
./LLX.Server/scripts/deploy-api-only.sh -b -f

# 方法2: 手动更新
docker stop llxrice_api
docker rm llxrice_api
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .
docker-compose -f docker-compose.api-only.yml up -d
```

### 备份日志

```bash
# 创建日志备份目录
mkdir -p logs_backup

# 备份日志文件
cp logs/*.log logs_backup/

# 压缩备份
tar -czf logs_backup_$(date +%Y%m%d).tar.gz logs_backup/

# 删除旧备份（保留最近7天）
find . -name "logs_backup_*.tar.gz" -mtime +7 -delete
```

### 清理旧日志

```bash
# 删除7天前的日志
find logs/ -name "*.log" -mtime +7 -delete

# 清理Docker日志
docker system prune -f

# 清理未使用的镜像
docker image prune -a -f
```

---

## 🔐 安全配置

### 1. 配置防火墙

#### UFW (Ubuntu/Debian)
```bash
# 启用防火墙
sudo ufw enable

# 允许SSH
sudo ufw allow 22/tcp

# 允许API端口
sudo ufw allow 8080/tcp

# 查看状态
sudo ufw status
```

#### Firewalld (CentOS/RHEL)
```bash
# 启动防火墙
sudo systemctl start firewalld
sudo systemctl enable firewalld

# 允许API端口
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload

# 查看状态
sudo firewall-cmd --list-all
```

### 2. 限制Docker容器资源

```bash
# 在.env文件中配置
CONTAINER_MEMORY_LIMIT=512M
CONTAINER_CPU_LIMIT=0.5
```

### 3. 使用非root用户运行

Dockerfile已经配置为使用非root用户(appuser)运行应用，无需额外配置。

### 4. 定期更新系统

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get upgrade -y

# CentOS/RHEL
sudo yum update -y
```

---

## 📊 监控和告警

### 1. 手动监控

```bash
# 创建监控脚本
cat > monitor.sh << 'EOF'
#!/bin/bash
while true; do
    if curl -f -s http://localhost:8080/health > /dev/null; then
        echo "$(date '+%Y-%m-%d %H:%M:%S') - Service Healthy"
    else
        echo "$(date '+%Y-%m-%d %H:%M:%S') - Service Unhealthy" >&2
    fi
    sleep 30
done
EOF

chmod +x monitor.sh
./monitor.sh
```

### 2. 使用Crontab定时检查

```bash
# 编辑crontab
crontab -e

# 添加以下行（每5分钟检查一次）
*/5 * * * * curl -f http://localhost:8080/health || echo "Service down at $(date)" >> /var/log/api_health.log
```

### 3. 查看系统日志

```bash
# 查看系统日志
sudo journalctl -u docker -f

# 查看容器日志
docker logs --since 1h llxrice_api
```

---

## 🚨 紧急情况处理

### 服务崩溃

```bash
# 1. 查看日志
docker logs --tail 500 llxrice_api > error.log

# 2. 重启服务
docker restart llxrice_api

# 3. 如果无法重启，强制重新部署
./LLX.Server/scripts/deploy-api-only.sh -f
```

### 数据库连接丢失

```bash
# 1. 检查数据库状态
telnet 192.168.1.100 5432

# 2. 重启API服务
docker restart llxrice_api

# 3. 检查连接配置
grep "DB_CONNECTION" .env
```

### 内存不足

```bash
# 1. 查看内存使用
free -h
docker stats llxrice_api

# 2. 增加内存限制（修改.env）
# CONTAINER_MEMORY_LIMIT=1G

# 3. 重新部署
./LLX.Server/scripts/deploy-api-only.sh -f

# 4. 清理系统缓存
sudo sync && echo 3 | sudo tee /proc/sys/vm/drop_caches
```

### 磁盘空间不足

```bash
# 查看磁盘使用
df -h

# 清理Docker数据
docker system prune -a -f --volumes

# 清理旧日志
find logs/ -name "*.log" -mtime +7 -delete

# 清理临时文件
sudo apt-get autoclean
sudo apt-get autoremove
```

---

## 📞 技术支持

### 查看帮助

```bash
# 部署脚本帮助
./LLX.Server/scripts/deploy-api-only.sh -h

# Docker命令帮助
docker --help
docker-compose --help
```

### 常用资源

- **API文档**: http://localhost:8080/swagger
- **健康检查**: http://localhost:8080/health
- **Docker文档**: https://docs.docker.com/
- **PostgreSQL文档**: https://www.postgresql.org/docs/
- **Redis文档**: https://redis.io/documentation

---

## ✅ 部署检查清单

### 部署前检查

- [ ] PostgreSQL数据库已运行且可访问
- [ ] Redis缓存已运行且可访问
- [ ] Docker已安装并运行
- [ ] Docker Compose已安装
- [ ] 项目代码已上传到服务器
- [ ] `.env`文件已正确配置
- [ ] 数据库连接字符串正确
- [ ] Redis连接字符串正确
- [ ] 端口8080未被占用（或已修改端口配置）
- [ ] 防火墙已配置允许API端口
- [ ] 当前用户有Docker权限

### 部署后验证

- [ ] 容器正常运行 (`docker ps`)
- [ ] 健康检查通过 (`curl http://localhost:8080/health`)
- [ ] API可以访问 (`curl http://localhost:8080/api/products`)
- [ ] Swagger文档可访问 (`curl http://localhost:8080/swagger`)
- [ ] 日志正常输出 (`docker logs llxrice_api`)
- [ ] 资源使用正常 (`docker stats llxrice_api`)

---

## 🎉 总结

本指南提供了在Linux系统下部署API服务的完整方案，包括：

1. ✅ **完整的前提条件说明**
2. ✅ **详细的安装步骤（Ubuntu/CentOS）**
3. ✅ **三种部署方式（脚本/Docker Compose/Docker命令）**
4. ✅ **详细的环境变量配置说明**
5. ✅ **完整的部署验证步骤**
6. ✅ **Linux特定的故障排查方案**
7. ✅ **日常维护操作指南**
8. ✅ **安全配置和监控指南**
9. ✅ **紧急情况处理流程**

按照本指南操作，您可以成功在Linux服务器上部署林龍香大米商城的API服务！
