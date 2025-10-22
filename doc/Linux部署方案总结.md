# 林龍香大米商城 - Linux部署方案总结

## 📦 已创建的部署文件

### 1. Docker配置文件
- ✅ **docker-compose.api-only.yml** - API服务专用Docker Compose配置
  - 仅包含API服务
  - 配置外部数据库和Redis连接
  - 包含健康检查和资源限制

### 2. 环境配置文件
- ✅ **env.api-only.example** - API服务专用环境变量配置示例
  - 详细的配置说明
  - 数据库连接字符串配置
  - Redis连接字符串配置
  - 性能和资源配置

### 3. 部署脚本
- ✅ **LLX.Server/scripts/deploy-api-only.sh** - Linux自动化部署脚本
  - 自动化部署流程
  - 环境检查
  - 配置验证
  - 服务启动和验证

### 4. 部署文档
- ✅ **doc/API服务Linux部署指南.md** - Linux详细部署指南（100+页）
  - Ubuntu/CentOS安装说明
  - 三种部署方式
  - 环境变量配置详解
  - Linux特定故障排查
  - 安全配置和监控
  - 日常维护操作

- ✅ **doc/API服务Linux快速部署.md** - Linux快速入门指南
  - 5分钟快速部署
  - Ubuntu/CentOS快速安装
  - 简化的配置说明
  - 常见问题快速解决

---

## 🚀 部署方式对比

### 方式一：自动化脚本部署（推荐）⭐⭐⭐⭐⭐

**适用场景**：首次部署、日常更新

**命令**：
```bash
./LLX.Server/scripts/deploy-api-only.sh
```

**优点**：
- ✅ 完全自动化
- ✅ 自动检查环境
- ✅ 自动验证配置
- ✅ 友好的错误提示

---

### 方式二：Docker Compose部署 ⭐⭐⭐⭐

**适用场景**：熟悉Docker Compose的用户

**命令**：
```bash
docker-compose -f docker-compose.api-only.yml up -d
```

**优点**：
- ✅ 标准Docker Compose流程
- ✅ 易于集成到CI/CD
- ✅ 配置文件化管理

---

### 方式三：Docker命令部署 ⭐⭐⭐

**适用场景**：熟悉Docker命令的用户

**命令**：
```bash
docker run -d --name llxrice_api -p 8080:8080 -e "..." llxrice/api:latest
```

**优点**：
- ✅ 完全控制
- ✅ 灵活配置
- ✅ 适合测试环境

---

## 📝 核心配置说明

### 数据库连接配置

**完整格式**：
```bash
Host={IP地址};Port={端口};Database={数据库名};Username={用户名};Password={密码};Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30
```

**配置示例**：
```bash
# 本地开发环境
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=llxrice;Username=postgres;Password=postgres;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# 生产环境
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=StrongPassword123!;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=200;Command Timeout=30
```

---

### Redis连接配置

**完整格式**：
```bash
{IP地址}:{端口},password={密码},ssl={是否SSL},abortConnect=false
```

**配置示例**：
```bash
# 本地开发环境（无密码）
REDIS_CONNECTION_STRING=localhost:6379,ssl=false,abortConnect=false

# 生产环境（有密码）
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=RedisPass123!,ssl=false,abortConnect=false

# 云环境（使用SSL）
REDIS_CONNECTION_STRING=myredis.cache.amazonaws.com:6379,password=CloudRedisPass,ssl=true,abortConnect=false
```

---

## ✅ Linux系统特定说明

### 支持的Linux发行版

- ✅ **Ubuntu** 20.04, 22.04
- ✅ **Debian** 10, 11
- ✅ **CentOS** 8, 9
- ✅ **RHEL** 8, 9
- ✅ **Rocky Linux** 8, 9
- ✅ **AlmaLinux** 8, 9

### Docker安装

#### Ubuntu/Debian
```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo apt-get install -y docker-compose
sudo usermod -aG docker $USER
newgrp docker
```

#### CentOS/RHEL
```bash
sudo yum install -y yum-utils
sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
sudo yum install -y docker-ce docker-ce-cli containerd.io
sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
sudo usermod -aG docker $USER
newgrp docker
```

### 防火墙配置

#### UFW (Ubuntu/Debian)
```bash
sudo ufw enable
sudo ufw allow 22/tcp
sudo ufw allow 8080/tcp
sudo ufw status
```

#### Firewalld (CentOS/RHEL)
```bash
sudo systemctl start firewalld
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload
sudo firewall-cmd --list-all
```

---

## 🎯 快速开始（3步）

### 步骤1：安装Docker
```bash
# Ubuntu/Debian
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo apt-get install -y docker-compose

# CentOS/RHEL
sudo yum install -y docker-ce docker-ce-cli containerd.io

# 配置权限
sudo usermod -aG docker $USER
newgrp docker
```

### 步骤2：配置连接信息
```bash
cd /path/to/LLX.Server
cp env.api-only.example .env
nano .env
```

**修改两行**：
```bash
DB_CONNECTION_STRING=Host=你的数据库IP;Port=5432;Database=llxrice;Username=用户名;Password=密码;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

REDIS_CONNECTION_STRING=你的RedisIP:6379,password=Redis密码,ssl=false,abortConnect=false
```

### 步骤3：运行部署
```bash
chmod +x LLX.Server/scripts/deploy-api-only.sh
./LLX.Server/scripts/deploy-api-only.sh
```

### 验证部署
```bash
curl http://localhost:8080/health
```

---

## 🔧 常用运维命令

### 日常操作

```bash
# 查看容器状态
docker ps | grep llxrice_api

# 查看日志
docker logs -f llxrice_api

# 重启服务
docker restart llxrice_api

# 停止服务
docker stop llxrice_api

# 删除容器
docker rm -f llxrice_api

# 查看资源使用
docker stats llxrice_api
```

### 更新部署

```bash
# 使用脚本更新（推荐）
./LLX.Server/scripts/deploy-api-only.sh -b -f

# 手动更新
docker stop llxrice_api
docker rm llxrice_api
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .
docker-compose -f docker-compose.api-only.yml up -d
```

### 故障排查

```bash
# 查看最近的日志
docker logs --tail 100 llxrice_api

# 查看完整日志
docker logs llxrice_api > api.log

# 进入容器
docker exec -it llxrice_api bash

# 测试数据库连接
telnet 192.168.1.100 5432
# 或
nc -zv 192.168.1.100 5432

# 测试Redis连接
telnet 192.168.1.101 6379
# 或
nc -zv 192.168.1.101 6379

# 检查端口占用
sudo netstat -tulpn | grep :8080
# 或
sudo lsof -i :8080

# 检查防火墙
sudo ufw status  # Ubuntu/Debian
sudo firewall-cmd --list-all  # CentOS/RHEL
```

---

## 📚 文档索引

### 快速入门
- **API服务Linux快速部署.md** - 5分钟快速部署
  - Ubuntu/CentOS快速安装
  - 简化流程
  - 快速配置
  - 常见问题

### 详细指南
- **API服务Linux部署指南.md** - 完整部署文档
  - 详细的前提条件
  - 系统安装说明
  - 三种部署方式
  - 完整的配置说明
  - Linux特定故障排查
  - 安全配置
  - 日常维护操作
  - 监控和告警

### 配置文件
- **docker-compose.api-only.yml** - Docker Compose配置
- **env.api-only.example** - 环境变量配置模板

### 部署脚本
- **deploy-api-only.sh** - Linux自动化部署脚本
  - 自动化部署
  - 环境检查
  - 配置验证

---

## ⚠️ 重要提示

### 安全建议

1. **数据库密码**
   - ❌ 不要使用弱密码
   - ✅ 使用强密码（字母+数字+特殊字符）
   - ✅ 定期更换密码

2. **Redis密码**
   - ❌ 不要使用空密码
   - ✅ 设置强密码
   - ✅ 限制访问IP

3. **网络安全**
   - ✅ 配置防火墙规则
   - ✅ 只开放必要端口
   - ✅ 使用VPN或内网访问

4. **用户权限**
   - ✅ 使用非root用户运行应用
   - ✅ 限制Docker用户权限
   - ✅ 定期审查权限

5. **系统更新**
   - ✅ 定期更新系统补丁
   - ✅ 及时更新Docker版本
   - ✅ 保持依赖库最新

### 性能建议

1. **资源配置**
   - 根据实际负载调整内存限制
   - 根据CPU使用情况调整CPU限制

2. **连接池配置**
   - 根据并发量调整连接池大小
   - 监控连接池使用情况

3. **缓存策略**
   - 合理配置Redis内存
   - 选择合适的缓存淘汰策略

### 监控建议

1. **健康检查**
   - 配置定时健康检查
   - 设置告警通知

2. **日志监控**
   - 定期检查错误日志
   - 配置日志轮转

3. **资源监控**
   - 监控CPU和内存使用
   - 监控磁盘空间

---

## 🎉 总结

您现在拥有完整的Linux系统API服务独立部署方案：

✅ **完整的Linux支持**
- Ubuntu/Debian系统支持
- CentOS/RHEL系统支持
- 详细的安装说明

✅ **3种部署方式**
- 自动化脚本部署（最简单）
- Docker Compose部署（标准方式）
- Docker命令部署（灵活方式）

✅ **完整的文档支持**
- 快速入门指南（5分钟上手）
- 详细部署指南（100+页完整文档）
- Linux特定配置和问题

✅ **自动化工具**
- Linux部署脚本
- 自动环境检查
- 自动配置验证

✅ **运维支持**
- 常用命令手册
- Linux特定故障排查
- 日常维护操作
- 安全配置指南

---

## 📞 获取帮助

### 查看文档
```bash
# 查看快速指南
cat doc/API服务Linux快速部署.md

# 查看详细指南
cat doc/API服务Linux部署指南.md

# 查看脚本帮助
./LLX.Server/scripts/deploy-api-only.sh -h
```

### 常用资源
- **API文档**: http://服务器IP:8080/swagger
- **健康检查**: http://服务器IP:8080/health
- **Docker文档**: https://docs.docker.com/
- **Linux命令参考**: https://man7.org/

---

**现在您可以快速在Linux服务器上部署API服务到任何已有PostgreSQL和Redis的环境中！**
