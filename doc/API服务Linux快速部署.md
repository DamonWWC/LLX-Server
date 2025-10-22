# 林龍香大米商城 - API服务Linux快速部署

> **5分钟在Linux服务器上快速部署API服务**

---

## 📋 前提条件

✅ Linux服务器（Ubuntu 20.04+ / CentOS 8+）  
✅ PostgreSQL数据库已运行  
✅ Redis缓存已运行  
✅ 已有数据库连接信息  

---

## 🚀 快速部署（3步完成）

### 第1步：安装Docker（2分钟）

#### Ubuntu/Debian
```bash
# 一键安装Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# 安装Docker Compose
sudo apt-get install -y docker-compose

# 添加当前用户到docker组
sudo usermod -aG docker $USER

# 启动Docker
sudo systemctl start docker
sudo systemctl enable docker

# 重新登录使权限生效
newgrp docker
```

#### CentOS/RHEL
```bash
# 安装Docker
sudo yum install -y yum-utils
sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
sudo yum install -y docker-ce docker-ce-cli containerd.io

# 安装Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 启动Docker
sudo systemctl start docker
sudo systemctl enable docker

# 添加当前用户到docker组
sudo usermod -aG docker $USER
newgrp docker
```

### 第2步：配置连接信息（1分钟）

```bash
# 进入项目目录
cd /path/to/LLX.Server

# 复制配置文件
cp env.api-only.example .env

# 编辑配置
nano .env
```

**修改这两行**（替换为您的实际信息）：

```bash
# 数据库连接（修改IP、用户名、密码）
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=your_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# Redis连接（修改IP、密码）
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=your_redis_password,ssl=false,abortConnect=false
```

保存并退出（Ctrl+X, Y, Enter）

### 第3步：运行部署脚本（2分钟）

```bash
# 添加执行权限
chmod +x LLX.Server/scripts/deploy-api-only.sh

# 运行部署脚本
./LLX.Server/scripts/deploy-api-only.sh
```

脚本会自动：
- ✅ 检查环境
- ✅ 编译项目
- ✅ 构建Docker镜像
- ✅ 启动服务

### 验证部署（30秒）

```bash
# 检查健康状态
curl http://localhost:8080/health

# 预期响应
# {"status":"Healthy"}
```

看到 Healthy 表示部署成功！ 🎉

---

## 📌 配置说明

### 数据库连接格式

```bash
Host={数据库IP};Port={端口};Database={库名};Username={用户名};Password={密码};Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30
```

**示例**：
```bash
# 本地数据库
Host=localhost;Port=5432;Database=llxrice;Username=postgres;Password=123456;...

# 远程数据库
Host=192.168.1.100;Port=5432;Database=llxrice;Username=llxrice_user;Password=MyPass123;...

# 云数据库
Host=mydb.abc.rds.amazonaws.com;Port=5432;Database=llxrice;Username=admin;Password=CloudPass;...
```

### Redis连接格式

```bash
{Redis IP}:{端口},password={密码},ssl={是否SSL},abortConnect=false
```

**示例**：
```bash
# 无密码
localhost:6379,ssl=false,abortConnect=false

# 有密码
localhost:6379,password=MyRedisPass,ssl=false,abortConnect=false

# 远程Redis
192.168.1.101:6379,password=SecurePass123,ssl=false,abortConnect=false

# 云Redis（使用SSL）
myredis.cache.amazonaws.com:6379,password=CloudPass,ssl=true,abortConnect=false
```

---

## 🔧 常用命令

### 查看状态
```bash
docker ps | grep llxrice_api
```

### 查看日志
```bash
docker logs -f llxrice_api
```

### 重启服务
```bash
docker restart llxrice_api
```

### 停止服务
```bash
docker stop llxrice_api
```

### 重新部署
```bash
./LLX.Server/scripts/deploy-api-only.sh -f
```

---

## ❌ 常见问题

### 问题1：权限被拒绝

**错误**：
```
permission denied while trying to connect to the Docker daemon socket
```

**解决**：
```bash
sudo usermod -aG docker $USER
newgrp docker
```

### 问题2：数据库连接失败

**检查**：
```bash
# 测试数据库连接
telnet 192.168.1.100 5432
# 或
nc -zv 192.168.1.100 5432
```

**解决**：
- 确认数据库IP和端口正确
- 确认用户名密码正确
- 检查防火墙是否开放5432端口

### 问题3：Redis连接失败

**检查**：
```bash
# 测试Redis连接
telnet 192.168.1.101 6379
# 或
nc -zv 192.168.1.101 6379
```

**解决**：
- 确认Redis IP和端口正确
- 确认密码正确
- 检查防火墙是否开放6379端口

### 问题4：端口被占用

**检查**：
```bash
sudo netstat -tulpn | grep :8080
# 或
sudo lsof -i :8080
```

**解决**：
```bash
# 杀死占用端口的进程
sudo kill -9 <PID>

# 或修改.env文件使用其他端口
# API_PORT=8081
```

### 问题5：容器无法启动

**查看日志**：
```bash
docker logs llxrice_api
```

**强制重新部署**：
```bash
./LLX.Server/scripts/deploy-api-only.sh -b -f
```

---

## 🔐 安全配置（可选）

### 配置防火墙

#### Ubuntu/Debian
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

#### CentOS/RHEL
```bash
# 启动防火墙
sudo systemctl start firewalld

# 允许API端口
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload

# 查看状态
sudo firewall-cmd --list-all
```

---

## 📞 需要帮助？

### 查看详细文档
- **完整部署指南**: `doc/API服务Linux部署指南.md`
- **部署脚本帮助**: `./LLX.Server/scripts/deploy-api-only.sh -h`

### 查看日志
- **Docker日志**: `docker logs llxrice_api`
- **应用日志**: `cat logs/app-*.log`

### 健康检查
- **API健康**: `curl http://localhost:8080/health`
- **Swagger文档**: http://服务器IP:8080/swagger

---

## ✅ 验证清单

部署成功的标志：

- [ ] 容器正常运行 `docker ps | grep llxrice_api`
- [ ] 健康检查通过 `curl http://localhost:8080/health`
- [ ] 可以访问API `curl http://localhost:8080/api/products`
- [ ] Swagger文档可访问 http://服务器IP:8080/swagger

---

## 🎯 下一步

部署成功后，您可以：

1. **测试API接口**：访问 http://服务器IP:8080/swagger
2. **查看应用日志**：`docker logs -f llxrice_api`
3. **配置反向代理**：使用Nginx提供HTTPS访问
4. **设置监控告警**：配置健康检查和日志监控

---

## 💡 常用运维命令

```bash
# 查看容器状态
docker ps -a

# 查看资源使用
docker stats llxrice_api

# 查看最近日志
docker logs --tail 100 llxrice_api

# 进入容器内部
docker exec -it llxrice_api bash

# 备份日志
tar -czf logs_backup_$(date +%Y%m%d).tar.gz logs/

# 清理Docker缓存
docker system prune -f
```

---

**恭喜！🎉 您已成功在Linux服务器上部署林龍香大米商城API服务！**
