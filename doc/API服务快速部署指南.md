# 林龍香大米商城 - API服务快速部署指南

> **5分钟快速部署API服务到已有数据库环境**

---

## 📋 前提条件

✅ PostgreSQL数据库已运行  
✅ Redis缓存已运行  
✅ Docker已安装  
✅ 已有数据库连接信息  

---

## 🚀 快速部署（3步完成）

### 第1步：配置连接信息（1分钟）

```powershell
# 1. 进入项目目录
cd E:\资料\学习代码\LLX.Server

# 2. 复制配置文件
copy env.api-only.example .env

# 3. 编辑配置
notepad .env
```

**修改这两行**（替换为您的实际信息）：

```bash
# 数据库连接（修改IP、用户名、密码）
DB_CONNECTION_STRING=Host=192.168.1.100;Port=5432;Database=llxrice;Username=your_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# Redis连接（修改IP、密码）
REDIS_CONNECTION_STRING=192.168.1.101:6379,password=your_redis_password,ssl=false,abortConnect=false
```

### 第2步：运行部署脚本（2-3分钟）

```powershell
# 运行一键部署脚本
.\LLX.Server\scripts\deploy-api-only.ps1
```

脚本会自动：
- ✅ 检查环境
- ✅ 编译项目
- ✅ 构建Docker镜像
- ✅ 启动服务

### 第3步：验证部署（30秒）

```powershell
# 访问健康检查
curl http://localhost:8080/health

# 或在浏览器访问
# http://localhost:8080/swagger
```

看到 `{"status":"Healthy"}` 表示部署成功！ 🎉

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
```powershell
docker ps | findstr llxrice_api
```

### 查看日志
```powershell
docker logs -f llxrice_api
```

### 重启服务
```powershell
docker restart llxrice_api
```

### 停止服务
```powershell
docker stop llxrice_api
```

### 重新部署
```powershell
.\LLX.Server\scripts\deploy-api-only.ps1 -Force
```

---

## ❌ 常见问题

### 问题1：数据库连接失败

**检查**：
```powershell
# 测试数据库连接
Test-NetConnection -ComputerName 192.168.1.100 -Port 5432
```

**解决**：
- 确认数据库IP和端口正确
- 确认用户名密码正确
- 检查防火墙是否开放5432端口

### 问题2：Redis连接失败

**检查**：
```powershell
# 测试Redis连接
Test-NetConnection -ComputerName 192.168.1.101 -Port 6379
```

**解决**：
- 确认Redis IP和端口正确
- 确认密码正确
- 检查防火墙是否开放6379端口

### 问题3：端口被占用

**检查**：
```powershell
netstat -ano | findstr :8080
```

**解决**：
```bash
# 修改.env文件，使用其他端口
API_PORT=8081
```

### 问题4：容器无法启动

**查看日志**：
```powershell
docker logs llxrice_api
```

**强制重新部署**：
```powershell
.\LLX.Server\scripts\deploy-api-only.ps1 -Build -Force
```

---

## 📞 需要帮助？

### 查看详细文档
- **完整部署指南**: `doc/API服务独立部署指南.md`
- **部署脚本帮助**: `.\LLX.Server\scripts\deploy-api-only.ps1 -Help`

### 查看日志
- **Docker日志**: `docker logs llxrice_api`
- **应用日志**: `logs/app-*.log`

### 健康检查
- **API健康**: http://localhost:8080/health
- **Swagger文档**: http://localhost:8080/swagger

---

## ✅ 验证清单

部署成功的标志：

- [ ] 容器正常运行 `docker ps | findstr llxrice_api`
- [ ] 健康检查通过 `curl http://localhost:8080/health`
- [ ] 可以访问API `curl http://localhost:8080/api/products`
- [ ] Swagger文档可访问 http://localhost:8080/swagger

---

## 🎯 下一步

部署成功后，您可以：

1. **测试API接口**：访问 http://localhost:8080/swagger
2. **查看应用日志**：`docker logs -f llxrice_api`
3. **运行性能测试**：`.\LLX.Server\scripts\test-api.ps1`
4. **配置Nginx反向代理**：参考完整部署指南

---

**恭喜！🎉 您已成功部署林龍香大米商城API服务！**
