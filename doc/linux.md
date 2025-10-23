# Linux 脚本使用指南

## 📋 概述

本文档介绍了林龍香大米商城后端服务在 Linux 环境下的所有脚本工具，包括部署、更新、测试、维护等功能。

## 🚀 部署脚本

### 1. deploy-api-only.sh - API服务独立部署脚本

**功能**：仅部署 API 服务，不部署数据库和 Redis（适用于已有外部数据库和缓存的情况）

**用法**：
```bash
./deploy-api-only.sh [选项]
```

**选项**：
- `-e, --environment <env>` - 部署环境 (development/production) [默认: production]
- `-b, --build` - 强制重新构建 Docker 镜像
- `-n, --no-build` - 使用已有的构建文件，不重新编译 .NET 项目
- `-f, --force` - 强制重新部署（停止并删除现有容器）
- `-h, --help` - 显示帮助信息

**示例**：
```bash
# 标准部署
./deploy-api-only.sh

# 重新构建镜像并部署
./deploy-api-only.sh -b

# 使用已构建的文件部署
./deploy-api-only.sh -n

# 强制重新部署
./deploy-api-only.sh -f

# 开发环境部署
./deploy-api-only.sh -e development
```

**前提条件**：
1. PostgreSQL 数据库已运行并可访问
2. Redis 缓存已运行并可访问
3. .env 文件已正确配置数据库和 Redis 连接信息

### 2. deploy.sh - 完整服务部署脚本

**功能**：部署完整的后端服务，包括 API、数据库和 Redis

**用法**：
```bash
./deploy.sh [选项]
```

**选项**：
- `-e, --environment <env>` - 部署环境 (development/production) [默认: production]
- `-b, --build` - 强制重新构建镜像
- `-p, --pull` - 拉取最新镜像
- `-f, --force` - 强制重新部署（停止并删除现有容器）
- `-h, --help` - 显示帮助信息

**示例**：
```bash
# 生产环境部署
./deploy.sh

# 开发环境部署
./deploy.sh -e development

# 强制重新构建并部署
./deploy.sh -b -f

# 拉取最新镜像并部署
./deploy.sh -p
```

## 🔄 更新脚本

### 3. update-api.sh - API服务快速更新脚本

**功能**：快速更新已部署的 API 服务

**用法**：
```bash
./update-api.sh [选项]
```

**选项**：
- `-b, --backup` - 更新前创建备份
- `-f, --force` - 强制更新（跳过确认）
- `-r, --rollback` - 回滚到上一个版本
- `-v, --version <version>` - 指定版本号
- `-h, --help` - 显示帮助信息

**示例**：
```bash
# 标准更新
./update-api.sh

# 创建备份后更新
./update-api.sh -b

# 强制更新
./update-api.sh -f

# 回滚到上一个版本
./update-api.sh -r

# 更新到指定版本
./update-api.sh -v 1.2.3
```

## 🔧 维护脚本

### 4. fix-container-issues.sh - 容器运行问题修复脚本

**功能**：解决容器运行中的常见问题，如日志权限和网络连接问题

**用法**：
```bash
./fix-container-issues.sh [选项]
```

**选项**：
- `-f, --force` - 强制修复（跳过确认）
- `-l, --logs` - 仅修复日志权限问题
- `-n, --network` - 仅修复网络连接问题
- `-h, --help` - 显示帮助信息

**示例**：
```bash
# 修复所有问题
./fix-container-issues.sh

# 强制修复
./fix-container-issues.sh -f

# 仅修复日志权限
./fix-container-issues.sh -l

# 仅修复网络连接
./fix-container-issues.sh -n
```

**修复的问题**：
- 日志文件权限问题
- 数据库连接问题（localhost 改为 host.docker.internal）
- Redis 连接问题

### 5. setup-permissions.sh - 权限设置脚本

**功能**：为所有脚本文件设置正确的执行权限

**用法**：
```bash
./setup-permissions.sh
```

**功能**：
- 为所有 .sh 脚本添加执行权限
- 检查项目目录结构
- 验证脚本文件完整性

## 🗄️ 数据库脚本

### 6. migrate-database.sh - 数据库迁移脚本

**功能**：执行数据库迁移操作

**用法**：
```bash
./migrate-database.sh <数据库类型> <连接字符串> [迁移名称]
```

**参数**：
- `数据库类型` - PostgreSQL, SqlServer, MySql, Sqlite
- `连接字符串` - 数据库连接字符串
- `迁移名称` - 迁移名称（可选，默认为 InitialCreate）

**示例**：
```bash
# PostgreSQL 迁移
./migrate-database.sh PostgreSQL "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass" InitialCreate

# SqlServer 迁移
./migrate-database.sh SqlServer "Server=localhost;Database=llxrice;User Id=user;Password=pass" InitialCreate
```

### 7. switch-database.sh - 数据库切换脚本

**功能**：切换数据库类型和连接字符串

**用法**：
```bash
./switch-database.sh <数据库类型> <连接字符串>
```

**参数**：
- `数据库类型` - PostgreSQL, SqlServer, MySql, Sqlite
- `连接字符串` - 新的数据库连接字符串

**示例**：
```bash
# 切换到 PostgreSQL
./switch-database.sh PostgreSQL "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass"

# 切换到 SqlServer
./switch-database.sh SqlServer "Server=localhost;Database=llxrice;User Id=user;Password=pass"
```

## 📋 脚本使用流程

### 首次部署流程
```bash
# 1. 设置脚本权限
./setup-permissions.sh

# 2. 选择部署方式
# 方式A：完整部署（包含数据库和Redis）
./deploy.sh -e production

# 方式B：仅部署API服务（使用外部数据库和Redis）
./deploy-api-only.sh -e production
```

### 日常维护流程
```bash
# 1. 更新API服务
./update-api.sh -b

# 2. 修复容器问题（如需要）
./fix-container-issues.sh

# 3. 数据库迁移（如需要）
./migrate-database.sh PostgreSQL "your_connection_string" "MigrationName"
```

### 故障排除流程
```bash
# 1. 检查容器状态
docker ps -a

# 2. 查看容器日志
docker logs llxrice_api

# 3. 修复容器问题
./fix-container-issues.sh -f

# 4. 重启服务
docker restart llxrice_api
```

## ⚠️ 注意事项

### 权限问题
- 确保脚本有执行权限：`chmod +x *.sh`
- 使用 `./setup-permissions.sh` 自动设置权限

### 环境配置
- 确保 `.env` 文件在项目根目录
- 验证数据库和 Redis 连接配置
- 检查 Docker 和 Docker Compose 是否安装

### 网络问题
- 容器内使用 `host.docker.internal` 访问宿主机服务
- 确保防火墙允许相应端口访问

### 日志问题
- 日志文件权限问题可通过 `./fix-container-issues.sh -l` 修复
- 日志目录：`./logs/`

## 🔗 相关文档

- [API服务Linux部署指南](./API服务Linux部署指南.md)
- [API服务Linux快速部署](./API服务Linux快速部署.md)
- [Linux部署方案总结](./Linux部署方案总结.md)
- [容器运行问题修复指南](./容器运行问题修复指南.md)

## 📞 技术支持

如遇到问题，请：
1. 查看脚本帮助信息：`./script-name.sh -h`
2. 检查容器日志：`docker logs llxrice_api`
3. 运行问题修复脚本：`./fix-container-issues.sh`
4. 参考相关文档和故障排除指南
