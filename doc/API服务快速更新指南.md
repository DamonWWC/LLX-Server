# API服务快速更新指南

## 🚀 快速更新（推荐）

### Linux/Mac 环境

```bash
# 1. 进入项目目录
cd /path/to/LLX.Server

# 2. 执行快速更新
./LLX.Server/scripts/update-api.sh

# 3. 带备份的更新
./LLX.Server/scripts/update-api.sh -b

# 4. 强制更新（不询问确认）
./LLX.Server/scripts/update-api.sh -f

# 5. 回滚到上一个版本
./LLX.Server/scripts/update-api.sh -r
```

### Windows 环境

```powershell
# 1. 进入项目目录
cd E:\资料\学习代码\LLX.Server

# 2. 执行快速更新
.\LLX.Server\scripts\update-api.ps1

# 3. 带备份的更新
.\LLX.Server\scripts\update-api.ps1 -Backup

# 4. 强制更新（不询问确认）
.\LLX.Server\scripts\update-api.ps1 -Force

# 5. 回滚到上一个版本
.\LLX.Server\scripts\update-api.ps1 -Rollback
```

---

## 📋 更新场景

### 1. 代码更新（最常见）

**场景**：修复Bug、添加新功能、性能优化

```bash
# Linux/Mac
./LLX.Server/scripts/update-api.sh -b

# Windows
.\LLX.Server\scripts\update-api.ps1 -Backup
```

**步骤**：
1. 自动拉取最新代码
2. 创建备份
3. 重新构建Docker镜像
4. 停止旧服务
5. 启动新服务
6. 验证更新

### 2. 配置更新

**场景**：修改环境变量、数据库连接、Redis配置

```bash
# 1. 编辑配置文件
vim .env  # Linux/Mac
notepad .env  # Windows

# 2. 执行更新
./LLX.Server/scripts/update-api.sh -f  # Linux/Mac
.\LLX.Server\scripts\update-api.ps1 -Force  # Windows
```

### 3. 紧急回滚

**场景**：更新后发现问题，需要快速回滚

```bash
# Linux/Mac
./LLX.Server/scripts/update-api.sh -r

# Windows
.\LLX.Server\scripts\update-api.ps1 -Rollback
```

### 4. 版本更新

**场景**：更新到特定版本

```bash
# Linux/Mac
./LLX.Server/scripts/update-api.sh -v v1.2.3

# Windows
.\LLX.Server\scripts\update-api.ps1 -Version v1.2.3
```

---

## ⚡ 一键更新命令

### 最常用的更新命令

```bash
# Linux/Mac - 带备份的标准更新
./LLX.Server/scripts/update-api.sh -b

# Windows - 带备份的标准更新
.\LLX.Server\scripts\update-api.ps1 -Backup
```

### 快速更新（无备份）

```bash
# Linux/Mac
./LLX.Server/scripts/update-api.sh -f

# Windows
.\LLX.Server\scripts\update-api.ps1 -Force
```

---

## 🔍 更新验证

### 自动验证

更新脚本会自动进行以下验证：

1. **健康检查**：`GET /health`
2. **API测试**：`GET /api/products`
3. **服务状态**：Docker容器状态
4. **端口监听**：检查服务端口

### 手动验证

```bash
# 检查服务状态
docker ps --filter "name=llxrice_api"

# 健康检查
curl http://localhost:8080/health

# 查看日志
docker logs llxrice_api

# 测试API
curl http://localhost:8080/api/products
```

---

## 🛠️ 故障排除

### 常见问题

#### 1. 更新失败

```bash
# 查看详细错误
docker logs llxrice_api

# 检查镜像构建
docker images llxrice/api

# 检查配置文件
cat .env
```

#### 2. 服务无法启动

```bash
# 检查端口占用
netstat -tlnp | grep 8080  # Linux/Mac
netstat -an | findstr 8080  # Windows

# 检查Docker服务
docker ps
systemctl status docker  # Linux
Get-Service docker  # Windows
```

#### 3. 数据库连接失败

```bash
# 检查环境变量
docker exec llxrice_api env | grep -i connection

# 测试数据库连接
docker exec llxrice_api ping your_db_host
```

### 紧急恢复

```bash
# 1. 立即停止服务
docker stop llxrice_api

# 2. 回滚到备份
./LLX.Server/scripts/update-api.sh -r  # Linux/Mac
.\LLX.Server\scripts\update-api.ps1 -Rollback  # Windows

# 3. 或者手动恢复
docker load < backups/YYYYMMDD_HHMMSS/api-image-backup.tar
docker-compose -f docker-compose.api-only.yml up -d
```

---

## 📊 更新监控

### 实时监控

```bash
# 监控容器状态
watch -n 5 'docker ps --filter "name=llxrice_api"'

# 监控资源使用
watch -n 5 'docker stats llxrice_api --no-stream'

# 监控日志
tail -f logs/info-$(date +%Y-%m-%d).log
```

### 性能测试

```bash
# 健康检查响应时间
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:8080/health

# 压力测试
ab -n 1000 -c 10 http://localhost:8080/health
```

---

## 📝 更新检查清单

### 更新前

- [ ] 代码已提交到Git
- [ ] 测试环境验证通过
- [ ] 备份策略已确认
- [ ] 回滚计划已准备

### 更新中

- [ ] 服务正常停止
- [ ] 新镜像构建成功
- [ ] 服务正常启动
- [ ] 健康检查通过

### 更新后

- [ ] 所有API端点正常
- [ ] 日志无错误信息
- [ ] 性能指标正常
- [ ] 用户访问正常

---

## 🎯 最佳实践

### 1. 更新频率

- **开发环境**：随时更新
- **测试环境**：每日更新
- **生产环境**：每周更新（或按需）

### 2. 备份策略

- **重要更新**：必须备份
- **小修复**：可选备份
- **紧急修复**：可跳过备份

### 3. 更新时机

- **业务低峰期**：凌晨2-6点
- **维护窗口**：周末或节假日
- **紧急情况**：随时更新

### 4. 通知机制

- **更新前**：通知相关人员
- **更新中**：实时状态更新
- **更新后**：确认通知

---

## 📞 技术支持

**遇到问题？**

1. **查看日志**：`docker logs llxrice_api`
2. **检查状态**：`docker ps`
3. **回滚恢复**：使用 `-r` 参数
4. **联系支持**：support@llxrice.com

---

**文档版本**：v1.0  
**最后更新**：2025-10-22  
**适用环境**：Linux, Windows, macOS
