# 林龍香大米商城 - Linux部署方案 README

## 🎯 项目说明

本项目提供了林龍香大米商城后端API服务在Linux服务器上的完整部署方案。

**适用场景**: 
- PostgreSQL数据库已经在其他地方运行
- Redis缓存已经在其他地方运行
- 只需要在Linux服务器上部署API服务

---

## 📦 部署文件说明

### 核心文件

```
LLX.Server/
├── docker-compose.api-only.yml          # Docker Compose配置
├── env.api-only.example                 # 环境变量配置示例
├── LLX.Server/
│   ├── Dockerfile                       # Docker镜像构建文件
│   └── scripts/
│       └── deploy-api-only.sh           # Linux部署脚本（可执行）
└── doc/
    ├── API服务Linux快速部署.md          # 快速入门（5分钟）
    ├── API服务Linux部署指南.md          # 详细指南（100+页）
    └── Linux部署方案总结.md             # 方案总结
```

---

## 🚀 快速开始

### 1. 系统要求

- **操作系统**: Ubuntu 20.04+ / CentOS 8+ / Debian 10+
- **硬件**: 最低2核CPU, 4GB内存, 10GB存储
- **软件**: Docker 20.10+, Docker Compose 2.0+

### 2. 一键部署（3步）

```bash
# 步骤1: 安装Docker（Ubuntu为例）
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo apt-get install -y docker-compose
sudo usermod -aG docker $USER
newgrp docker

# 步骤2: 配置环境变量
cd /path/to/LLX.Server
cp env.api-only.example .env
nano .env
# 修改数据库和Redis连接信息

# 步骤3: 运行部署脚本
chmod +x LLX.Server/scripts/deploy-api-only.sh
./LLX.Server/scripts/deploy-api-only.sh

# 验证部署
curl http://localhost:8080/health
```

---

## 📖 文档导航

### 新手入门 👶

**推荐阅读**: `doc/API服务Linux快速部署.md`

这是5分钟快速入门指南，包括：
- ✅ 快速安装Docker
- ✅ 简化的配置说明
- ✅ 一键部署命令
- ✅ 常见问题快速解决

### 完整指南 📚

**详细文档**: `doc/API服务Linux部署指南.md`

这是100+页完整部署指南，包括：
- ✅ Ubuntu/CentOS详细安装
- ✅ 三种部署方式对比
- ✅ 环境变量详细说明
- ✅ Linux特定故障排查
- ✅ 安全配置和监控
- ✅ 日常维护操作

### 方案总结 📋

**总结文档**: `doc/Linux部署方案总结.md`

快速查阅各种信息：
- ✅ 文件清单
- ✅ 部署方式对比
- ✅ 常用命令参考
- ✅ 配置示例

---

## 🔧 配置说明

### 必需配置项

编辑 `.env` 文件，修改以下两行：

```bash
# 数据库连接（替换为实际值）
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# Redis连接（替换为实际值）
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=your_redis_password,ssl=false,abortConnect=false
```

### 可选配置项

```bash
# API端口（默认8080）
API_PORT=8080

# 应用环境（Development/Production）
ASPNETCORE_ENVIRONMENT=Production

# 资源限制
CONTAINER_MEMORY_LIMIT=512M
CONTAINER_CPU_LIMIT=0.5
```

---

## 📝 部署方式

### 方式一：自动化脚本（推荐）⭐⭐⭐⭐⭐

```bash
# 标准部署
./LLX.Server/scripts/deploy-api-only.sh

# 强制重建
./LLX.Server/scripts/deploy-api-only.sh -b -f

# 查看帮助
./LLX.Server/scripts/deploy-api-only.sh -h
```

**优点**: 完全自动化，自动检查环境，友好的错误提示

### 方式二：Docker Compose

```bash
# 构建并启动
docker-compose -f docker-compose.api-only.yml up -d

# 查看日志
docker-compose -f docker-compose.api-only.yml logs -f

# 停止服务
docker-compose -f docker-compose.api-only.yml down
```

**优点**: 标准流程，易于集成CI/CD

### 方式三：Docker命令

```bash
# 构建镜像
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .

# 运行容器
docker run -d \
  --name llxrice_api \
  -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=..." \
  -e "ConnectionStrings__Redis=..." \
  llxrice/api:latest
```

**优点**: 完全控制，灵活配置

---

## ✅ 验证部署

### 健康检查
```bash
curl http://localhost:8080/health
```

**预期响应**:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

### API测试
```bash
# 获取商品列表
curl http://localhost:8080/api/products

# Swagger文档
curl http://localhost:8080/swagger
```

### 查看日志
```bash
# 实时日志
docker logs -f llxrice_api

# 最近100行
docker logs --tail 100 llxrice_api
```

---

## 🔧 常用命令

### 日常操作

```bash
# 查看状态
docker ps | grep llxrice_api

# 重启服务
docker restart llxrice_api

# 停止服务
docker stop llxrice_api

# 查看日志
docker logs -f llxrice_api

# 查看资源
docker stats llxrice_api
```

### 更新部署

```bash
# 使用脚本更新（推荐）
./LLX.Server/scripts/deploy-api-only.sh -b -f

# 手动更新
docker stop llxrice_api && docker rm llxrice_api
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .
docker-compose -f docker-compose.api-only.yml up -d
```

---

## ❌ 常见问题

### 1. 权限被拒绝
```bash
# 错误: permission denied
# 解决:
sudo usermod -aG docker $USER
newgrp docker
```

### 2. 数据库连接失败
```bash
# 测试连接
telnet 192.168.1.100 5432
nc -zv 192.168.1.100 5432

# 检查配置
grep "DB_CONNECTION" .env
```

### 3. Redis连接失败
```bash
# 测试连接
telnet 192.168.1.101 6379
nc -zv 192.168.1.101 6379

# 检查配置
grep "REDIS_CONNECTION" .env
```

### 4. 端口被占用
```bash
# 检查端口
sudo netstat -tulpn | grep :8080
sudo lsof -i :8080

# 杀死进程
sudo kill -9 <PID>
```

### 5. 容器无法启动
```bash
# 查看日志
docker logs llxrice_api

# 强制重新部署
./LLX.Server/scripts/deploy-api-only.sh -f
```

---

## 🔐 安全配置

### 配置防火墙

**Ubuntu/Debian**:
```bash
sudo ufw enable
sudo ufw allow 22/tcp
sudo ufw allow 8080/tcp
```

**CentOS/RHEL**:
```bash
sudo systemctl start firewalld
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload
```

### 使用强密码

- ✅ 数据库密码: 至少12位，包含大小写字母、数字、特殊字符
- ✅ Redis密码: 至少12位，包含大小写字母、数字、特殊字符
- ❌ 不要使用默认密码或弱密码

---

## 📊 监控和维护

### 健康监控

```bash
# 创建监控脚本
cat > monitor.sh << 'EOF'
#!/bin/bash
while true; do
    if curl -f -s http://localhost:8080/health > /dev/null; then
        echo "$(date) - Service Healthy"
    else
        echo "$(date) - Service Unhealthy"
    fi
    sleep 30
done
EOF

chmod +x monitor.sh
./monitor.sh
```

### 日志管理

```bash
# 备份日志
tar -czf logs_backup_$(date +%Y%m%d).tar.gz logs/

# 清理旧日志
find logs/ -name "*.log" -mtime +7 -delete

# 清理Docker日志
docker system prune -f
```

---

## 📞 获取帮助

### 文档
- **快速入门**: `doc/API服务Linux快速部署.md`
- **详细指南**: `doc/API服务Linux部署指南.md`
- **方案总结**: `doc/Linux部署方案总结.md`

### 命令帮助
```bash
# 部署脚本帮助
./LLX.Server/scripts/deploy-api-only.sh -h

# Docker命令帮助
docker --help
docker-compose --help
```

### 在线资源
- **API文档**: http://服务器IP:8080/swagger
- **健康检查**: http://服务器IP:8080/health
- **Docker文档**: https://docs.docker.com/

---

## ✅ 部署检查清单

### 部署前
- [ ] Linux服务器已准备（Ubuntu/CentOS）
- [ ] Docker已安装并运行
- [ ] Docker Compose已安装
- [ ] PostgreSQL数据库可访问
- [ ] Redis缓存可访问
- [ ] 已获取数据库连接信息
- [ ] 已获取Redis连接信息
- [ ] 防火墙已配置

### 部署后
- [ ] 容器正常运行
- [ ] 健康检查通过
- [ ] API可以访问
- [ ] Swagger文档可访问
- [ ] 日志正常输出
- [ ] 资源使用正常

---

## 🎉 总结

您现在拥有完整的Linux部署方案：

✅ **自动化部署** - 一键部署脚本  
✅ **完整文档** - 快速入门 + 详细指南  
✅ **多种方式** - 脚本/Compose/命令  
✅ **故障排查** - 详细的问题解决方案  
✅ **安全配置** - 防火墙和权限管理  
✅ **运维支持** - 监控、日志、维护  

**立即开始**: 查看 `doc/API服务Linux快速部署.md` 5分钟快速上手！

---

**版本**: v1.0.0  
**更新时间**: 2024-01-01  
**支持系统**: Ubuntu 20.04+, CentOS 8+, Debian 10+
