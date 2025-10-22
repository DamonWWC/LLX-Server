# API服务更新操作指南

## 📋 概述

本文档详细说明如何更新已部署的API服务，包括代码更新、配置更新、数据库迁移等场景。适用于Linux环境下的Docker容器化部署。

---

## 🎯 更新场景

### 1. 代码更新
- 修复Bug
- 添加新功能
- 性能优化
- 安全更新

### 2. 配置更新
- 环境变量调整
- 数据库连接配置
- Redis配置
- 日志配置

### 3. 数据库更新
- 数据库结构变更
- 数据迁移
- 索引优化

### 4. 依赖更新
- .NET版本升级
- NuGet包更新
- 系统依赖更新

---

## 🚀 快速更新（推荐）

### 方法一：使用更新脚本

```bash
# 1. 进入项目目录
cd /path/to/LLX.Server

# 2. 拉取最新代码
git pull origin main

# 3. 执行更新部署
./LLX.Server/scripts/deploy-api-only.sh -b -f
```

**参数说明**：
- `-b`：强制重新构建Docker镜像
- `-f`：强制重新部署（停止并删除现有容器）

### 方法二：分步更新

```bash
# 1. 拉取最新代码
git pull origin main

# 2. 停止现有服务
docker stop llxrice_api

# 3. 重新构建镜像
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .

# 4. 启动新服务
docker-compose -f docker-compose.api-only.yml up -d

# 5. 验证服务
curl http://localhost:8080/health
```

---

## 🔧 详细更新步骤

### 步骤1：准备工作

#### 1.1 备份当前状态

```bash
# 创建备份目录
mkdir -p backups/$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="backups/$(date +%Y%m%d_%H%M%S)"

# 备份当前配置
cp .env "$BACKUP_DIR/"
cp docker-compose.api-only.yml "$BACKUP_DIR/"

# 备份日志
cp -r logs "$BACKUP_DIR/" 2>/dev/null || true

# 导出当前镜像
docker save llxrice/api:latest > "$BACKUP_DIR/api-image-backup.tar"

echo "备份完成：$BACKUP_DIR"
```

#### 1.2 检查当前服务状态

```bash
# 查看容器状态
docker ps --filter "name=llxrice_api"

# 查看服务日志
docker logs --tail 50 llxrice_api

# 检查服务健康状态
curl -f http://localhost:8080/health
```

### 步骤2：代码更新

#### 2.1 拉取最新代码

```bash
# 切换到主分支
git checkout main

# 拉取最新代码
git pull origin main

# 查看更新内容
git log --oneline -10
```

#### 2.2 检查更新内容

```bash
# 查看文件变更
git diff HEAD~1 --name-only

# 查看具体变更内容
git diff HEAD~1

# 检查是否有破坏性变更
git log --oneline --grep="BREAKING" HEAD~5..HEAD
```

### 步骤3：环境检查

#### 3.1 验证环境配置

```bash
# 检查环境变量文件
cat .env

# 验证数据库连接
docker exec llxrice_api dotnet LLX.Server.dll --check-db

# 验证Redis连接
docker exec llxrice_api dotnet LLX.Server.dll --check-redis
```

#### 3.2 检查依赖更新

```bash
# 检查.NET项目依赖
cd LLX.Server
dotnet list package --outdated

# 检查Docker基础镜像更新
docker pull mcr.microsoft.com/dotnet/aspnet:8.0
docker pull mcr.microsoft.com/dotnet/sdk:8.0
```

### 步骤4：数据库更新（如需要）

#### 4.1 数据库迁移

```bash
# 进入容器执行迁移
docker exec -it llxrice_api bash

# 在容器内执行迁移
dotnet ef database update

# 或者使用迁移脚本
dotnet run --project LLX.Server -- migrate-database
```

#### 4.2 数据备份（重要）

```bash
# 备份数据库
pg_dump -h your_db_host -U your_username -d your_database > "backups/$(date +%Y%m%d_%H%M%S)/database_backup.sql"

# 备份Redis数据
redis-cli -h your_redis_host --rdb "backups/$(date +%Y%m%d_%H%M%S)/redis_backup.rdb"
```

### 步骤5：构建和部署

#### 5.1 编译项目

```bash
# 清理旧文件
cd LLX.Server
dotnet clean --configuration Release

# 编译项目
dotnet build --configuration Release

# 运行测试（如果有）
dotnet test --configuration Release
```

#### 5.2 构建Docker镜像

```bash
# 回到项目根目录
cd ..

# 构建新镜像
docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .

# 或者带版本标签
VERSION=$(date +%Y%m%d_%H%M%S)
docker build -f LLX.Server/Dockerfile -t llxrice/api:$VERSION .
docker tag llxrice/api:$VERSION llxrice/api:latest
```

#### 5.3 滚动更新部署

```bash
# 方法1：零停机更新（推荐）
# 启动新容器
docker-compose -f docker-compose.api-only.yml up -d --scale api=2

# 等待新容器就绪
sleep 30

# 停止旧容器
docker stop llxrice_api_old 2>/dev/null || true

# 重命名容器
docker rename llxrice_api llxrice_api_old
docker rename llxrice_api_1 llxrice_api

# 清理旧容器
docker rm llxrice_api_old

# 方法2：简单重启
docker-compose -f docker-compose.api-only.yml down
docker-compose -f docker-compose.api-only.yml up -d
```

### 步骤6：验证更新

#### 6.1 健康检查

```bash
# 等待服务启动
sleep 30

# 检查容器状态
docker ps --filter "name=llxrice_api"

# 检查服务健康
curl -f http://localhost:8080/health

# 检查API响应
curl http://localhost:8080/api/products
```

#### 6.2 功能验证

```bash
# 测试关键API端点
curl -X GET http://localhost:8080/api/products
curl -X GET http://localhost:8080/api/orders
curl -X GET http://localhost:8080/api/addresses

# 检查日志
docker logs --tail 100 llxrice_api

# 检查性能
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:8080/health
```

#### 6.3 监控检查

```bash
# 检查资源使用
docker stats llxrice_api --no-stream

# 检查端口监听
netstat -tlnp | grep 8080

# 检查日志文件
tail -f logs/info-$(date +%Y-%m-%d).log
```

---

## 🔄 回滚操作

### 快速回滚

```bash
# 1. 停止当前服务
docker stop llxrice_api

# 2. 恢复之前的镜像
docker load < backups/YYYYMMDD_HHMMSS/api-image-backup.tar

# 3. 启动旧版本
docker-compose -f docker-compose.api-only.yml up -d

# 4. 验证回滚
curl http://localhost:8080/health
```

### 数据库回滚

```bash
# 恢复数据库备份
psql -h your_db_host -U your_username -d your_database < backups/YYYYMMDD_HHMMSS/database_backup.sql

# 恢复Redis备份
redis-cli -h your_redis_host --rdb backups/YYYYMMDD_HHMMSS/redis_backup.rdb
```

---

## 🛠️ 更新脚本

### 创建自动更新脚本

```bash
#!/bin/bash
# update-api.sh - API服务自动更新脚本

set -e

# 配置
PROJECT_DIR="/path/to/LLX.Server"
BACKUP_DIR="backups/$(date +%Y%m%d_%H%M%S)"
LOG_FILE="update-$(date +%Y%m%d_%H%M%S).log"

# 日志函数
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

# 错误处理
error_exit() {
    log "ERROR: $1"
    exit 1
}

# 主更新流程
main() {
    log "开始API服务更新..."
    
    # 1. 进入项目目录
    cd "$PROJECT_DIR" || error_exit "无法进入项目目录"
    
    # 2. 创建备份
    log "创建备份..."
    mkdir -p "$BACKUP_DIR"
    cp .env "$BACKUP_DIR/" 2>/dev/null || true
    docker save llxrice/api:latest > "$BACKUP_DIR/api-image-backup.tar" 2>/dev/null || true
    
    # 3. 拉取代码
    log "拉取最新代码..."
    git pull origin main || error_exit "代码拉取失败"
    
    # 4. 停止服务
    log "停止现有服务..."
    docker-compose -f docker-compose.api-only.yml down || true
    
    # 5. 构建镜像
    log "构建新镜像..."
    docker build -f LLX.Server/Dockerfile -t llxrice/api:latest . || error_exit "镜像构建失败"
    
    # 6. 启动服务
    log "启动新服务..."
    docker-compose -f docker-compose.api-only.yml up -d || error_exit "服务启动失败"
    
    # 7. 等待就绪
    log "等待服务就绪..."
    sleep 30
    
    # 8. 验证服务
    if curl -f http://localhost:8080/health > /dev/null 2>&1; then
        log "✅ 更新成功！"
    else
        log "❌ 服务验证失败，开始回滚..."
        docker load < "$BACKUP_DIR/api-image-backup.tar"
        docker-compose -f docker-compose.api-only.yml up -d
        error_exit "更新失败，已回滚到之前版本"
    fi
    
    log "更新完成！备份位置：$BACKUP_DIR"
}

# 执行主函数
main "$@"
```

### 使用更新脚本

```bash
# 给脚本执行权限
chmod +x update-api.sh

# 执行更新
./update-api.sh

# 查看更新日志
tail -f update-YYYYMMDD_HHMMSS.log
```

---

## 📊 更新监控

### 监控指标

```bash
# 1. 服务状态监控
watch -n 5 'docker ps --filter "name=llxrice_api"'

# 2. 资源使用监控
watch -n 5 'docker stats llxrice_api --no-stream'

# 3. 日志监控
tail -f logs/info-$(date +%Y-%m-%d).log

# 4. 健康检查监控
watch -n 10 'curl -s http://localhost:8080/health | jq .'
```

### 性能测试

```bash
# 使用ab进行压力测试
ab -n 1000 -c 10 http://localhost:8080/health

# 使用wrk进行性能测试
wrk -t4 -c100 -d30s http://localhost:8080/health

# 使用curl测试响应时间
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:8080/health
```

---

## 🚨 故障排除

### 常见问题

#### 1. 服务启动失败

```bash
# 查看详细错误
docker logs llxrice_api

# 检查端口占用
netstat -tlnp | grep 8080

# 检查配置文件
docker exec llxrice_api cat /app/appsettings.Production.json
```

#### 2. 数据库连接失败

```bash
# 检查数据库连接
docker exec llxrice_api dotnet LLX.Server.dll --check-db

# 检查环境变量
docker exec llxrice_api env | grep -i connection

# 测试数据库连接
docker exec llxrice_api ping your_db_host
```

#### 3. 性能问题

```bash
# 检查资源使用
docker stats llxrice_api

# 检查日志中的错误
grep -i error logs/info-$(date +%Y-%m-%d).log

# 检查慢查询
grep -i "slow" logs/info-$(date +%Y-%m-%d).log
```

### 紧急恢复

```bash
# 1. 立即停止服务
docker stop llxrice_api

# 2. 恢复备份
docker load < backups/YYYYMMDD_HHMMSS/api-image-backup.tar

# 3. 启动旧版本
docker-compose -f docker-compose.api-only.yml up -d

# 4. 通知相关人员
echo "服务已回滚到备份版本" | mail -s "API服务紧急回滚" admin@example.com
```

---

## 📋 更新检查清单

### 更新前检查

- [ ] 代码已提交到版本控制系统
- [ ] 测试环境验证通过
- [ ] 数据库备份已完成
- [ ] 配置文件已更新
- [ ] 依赖项已检查
- [ ] 回滚计划已准备

### 更新中检查

- [ ] 服务正常停止
- [ ] 新镜像构建成功
- [ ] 服务正常启动
- [ ] 健康检查通过
- [ ] 关键功能正常
- [ ] 性能指标正常

### 更新后检查

- [ ] 所有API端点正常
- [ ] 日志无错误信息
- [ ] 监控指标正常
- [ ] 用户访问正常
- [ ] 数据库连接正常
- [ ] 缓存服务正常

---

## 📞 联系信息

**技术支持**：
- 邮箱：support@llxrice.com
- 电话：400-xxx-xxxx
- 文档：https://docs.llxrice.com

**紧急联系**：
- 24小时热线：xxx-xxxx-xxxx
- 微信群：LLX技术交流群

---

**文档版本**：v1.0  
**最后更新**：2025-10-22  
**维护人员**：技术团队
