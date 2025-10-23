# Windows 脚本使用指南

## 📋 概述

本文档介绍了林龍香大米商城后端服务在 Windows 环境下的所有脚本工具，包括部署、更新、测试、维护、配置管理等功能。

## 🚀 部署脚本

### 1. deploy-api-only.ps1 - API服务独立部署脚本

**功能**：仅部署 API 服务，不部署数据库和 Redis（适用于已有外部数据库和缓存的情况）

**用法**：
```powershell
.\deploy-api-only.ps1 [参数]
```

**参数**：
- `-Environment <string>` - 部署环境 (development/production) [默认: production]
- `-Build` - 强制重新构建 Docker 镜像
- `-NoBuild` - 使用已有的构建文件，不重新编译 .NET 项目
- `-Force` - 强制重新部署（停止并删除现有容器）
- `-Help` - 显示帮助信息

**示例**：
```powershell
# 标准部署
.\deploy-api-only.ps1

# 重新构建镜像并部署
.\deploy-api-only.ps1 -Build

# 使用已构建的文件部署
.\deploy-api-only.ps1 -NoBuild

# 强制重新部署
.\deploy-api-only.ps1 -Force

# 开发环境部署
.\deploy-api-only.ps1 -Environment development
```

**前提条件**：
1. PostgreSQL 数据库已运行并可访问
2. Redis 缓存已运行并可访问
3. .env 文件已正确配置数据库和 Redis 连接信息

### 2. deploy.ps1 - 完整服务部署脚本

**功能**：部署完整的后端服务，包括 API、数据库和 Redis

**用法**：
```powershell
.\deploy.ps1 [参数]
```

**参数**：
- `-Environment <string>` - 部署环境 (development/production) [默认: production]
- `-Build` - 强制重新构建镜像
- `-Pull` - 拉取最新镜像
- `-Force` - 强制重新部署（停止并删除现有容器）
- `-Help` - 显示帮助信息

**示例**：
```powershell
# 生产环境部署
.\deploy.ps1

# 开发环境部署
.\deploy.ps1 -Environment development

# 强制重新构建并部署
.\deploy.ps1 -Build -Force

# 拉取最新镜像并部署
.\deploy.ps1 -Pull
```

## 🔄 更新脚本

### 3. update-api.ps1 - API服务快速更新脚本

**功能**：快速更新已部署的 API 服务

**用法**：
```powershell
.\update-api.ps1 [参数]
```

**参数**：
- `-Backup` - 更新前创建备份
- `-Force` - 强制更新（跳过确认）
- `-Rollback` - 回滚到上一个版本
- `-Version <string>` - 指定版本号
- `-Help` - 显示帮助信息

**示例**：
```powershell
# 标准更新
.\update-api.ps1

# 创建备份后更新
.\update-api.ps1 -Backup

# 强制更新
.\update-api.ps1 -Force

# 回滚到上一个版本
.\update-api.ps1 -Rollback

# 更新到指定版本
.\update-api.ps1 -Version "1.2.3"
```

## 🔧 维护脚本

### 4. fix-container-issues.ps1 - 容器运行问题修复脚本

**功能**：解决容器运行中的常见问题，如日志权限和网络连接问题

**用法**：
```powershell
.\fix-container-issues.ps1 [参数]
```

**参数**：
- `-Force` - 强制修复（跳过确认）
- `-LogsOnly` - 仅修复日志权限问题
- `-NetworkOnly` - 仅修复网络连接问题
- `-Help` - 显示帮助信息

**示例**：
```powershell
# 修复所有问题
.\fix-container-issues.ps1

# 强制修复
.\fix-container-issues.ps1 -Force

# 仅修复日志权限
.\fix-container-issues.ps1 -LogsOnly

# 仅修复网络连接
.\fix-container-issues.ps1 -NetworkOnly
```

### 5. fix-deployment.ps1 - 部署问题修复脚本

**功能**：修复常见的部署配置问题

**用法**：
```powershell
.\fix-deployment.ps1
```

**修复的问题**：
- 缺失的 docker-compose.yml 文件
- 缺失的 .env 文件
- 配置文件路径问题

## 🗄️ 数据库脚本

### 6. migrate-database.ps1 - 数据库迁移脚本

**功能**：执行数据库迁移操作

**用法**：
```powershell
.\migrate-database.ps1 -DatabaseType <类型> -ConnectionString <连接字符串> [-MigrationName <名称>]
```

**参数**：
- `-DatabaseType` - 数据库类型 (PostgreSQL, SqlServer, MySql, Sqlite)
- `-ConnectionString` - 数据库连接字符串
- `-MigrationName` - 迁移名称（可选，默认为 InitialCreate）

**示例**：
```powershell
# PostgreSQL 迁移
.\migrate-database.ps1 -DatabaseType PostgreSQL -ConnectionString "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass" -MigrationName "InitialCreate"

# SqlServer 迁移
.\migrate-database.ps1 -DatabaseType SqlServer -ConnectionString "Server=localhost;Database=llxrice;User Id=user;Password=pass"
```

### 7. switch-database.ps1 - 数据库切换脚本

**功能**：切换数据库类型和连接字符串

**用法**：
```powershell
.\switch-database.ps1 -DatabaseType <类型> -ConnectionString <连接字符串> [-ConfigFile <配置文件>]
```

**参数**：
- `-DatabaseType` - 数据库类型 (PostgreSQL, SqlServer, MySql, Sqlite)
- `-ConnectionString` - 新的数据库连接字符串
- `-ConfigFile` - 配置文件路径（可选，默认为 appsettings.json）

**示例**：
```powershell
# 切换到 PostgreSQL
.\switch-database.ps1 -DatabaseType PostgreSQL -ConnectionString "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass"

# 切换到 SqlServer
.\switch-database.ps1 -DatabaseType SqlServer -ConnectionString "Server=localhost;Database=llxrice;User Id=user;Password=pass"
```

### 8. manage-migrations.ps1 - 迁移管理脚本

**功能**：管理数据库迁移的创建、删除和应用

**用法**：
```powershell
.\manage-migrations.ps1 [参数]
```

**参数**：
- `-Action` - 操作类型 (Add, Remove, Update, List)
- `-MigrationName` - 迁移名称
- `-ConnectionString` - 数据库连接字符串

## 🔐 配置管理脚本

### 9. encrypt-config.ps1 - 配置文件加密脚本

**功能**：加密配置文件中的敏感信息

**用法**：
```powershell
.\encrypt-config.ps1 [参数]
```

**参数**：
- `-ConfigFile` - 要加密的配置文件路径（默认: appsettings.json）
- `-EncryptionKey` - 加密密钥（可选，不提供则使用环境变量）
- `-GenerateKey` - 生成新的加密密钥
- `-Help` - 显示帮助信息

**示例**：
```powershell
# 加密默认配置文件
.\encrypt-config.ps1

# 加密指定配置文件
.\encrypt-config.ps1 -ConfigFile "appsettings.Production.json"

# 生成新密钥并加密
.\encrypt-config.ps1 -GenerateKey
```

### 10. encrypt-all-configs.ps1 - 批量配置文件加密脚本

**功能**：批量加密所有配置文件

**用法**：
```powershell
.\encrypt-all-configs.ps1 [参数]
```

**参数**：
- `-EncryptionKey` - 加密密钥
- `-Force` - 强制覆盖已加密的文件

### 11. generate-encryption-key.ps1 - 加密密钥生成脚本

**功能**：生成新的加密密钥

**用法**：
```powershell
.\generate-encryption-key.ps1
```

### 12. setup-config.ps1 - 配置设置脚本

**功能**：设置和验证配置文件

**用法**：
```powershell
.\setup-config.ps1 [参数]
```

## 🧪 测试脚本

### 13. test-database.ps1 - 数据库测试脚本

**功能**：测试数据库连接和基本操作

**用法**：
```powershell
.\test-database.ps1 [参数]
```

**参数**：
- `-ConnectionString` - 数据库连接字符串
- `-Verbose` - 显示详细输出
- `-Help` - 显示帮助信息

**示例**：
```powershell
# 使用默认连接字符串测试
.\test-database.ps1

# 使用自定义连接字符串测试
.\test-database.ps1 -ConnectionString "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass"

# 显示详细输出
.\test-database.ps1 -Verbose
```

### 14. test-cache.ps1 - 缓存测试脚本

**功能**：测试 Redis 缓存连接和操作

**用法**：
```powershell
.\test-cache.ps1 [参数]
```

**参数**：
- `-ConnectionString` - Redis 连接字符串
- `-Verbose` - 显示详细输出

### 15. test-api.ps1 - API测试脚本

**功能**：测试 API 端点的可用性和响应

**用法**：
```powershell
.\test-api.ps1 [参数]
```

**参数**：
- `-BaseUrl` - API 基础 URL
- `-Endpoints` - 要测试的端点列表

### 16. test-performance.ps1 - 性能测试脚本

**功能**：执行性能测试和基准测试

**用法**：
```powershell
.\test-performance.ps1 [参数]
```

**参数**：
- `-ConcurrentUsers` - 并发用户数
- `-Duration` - 测试持续时间
- `-Endpoint` - 测试端点

### 17. test-connection.ps1 - 连接测试脚本

**功能**：测试各种服务的连接性

**用法**：
```powershell
.\test-connection.ps1 [参数]
```

### 18. test-redis-connection.ps1 - Redis连接测试脚本

**功能**：专门测试 Redis 连接

**用法**：
```powershell
.\test-redis-connection.ps1 [参数]
```

### 19. test-logging.ps1 - 日志测试脚本

**功能**：测试日志记录功能

**用法**：
```powershell
.\test-logging.ps1 [参数]
```

## 📊 监控和维护脚本

### 20. check-database-health.ps1 - 数据库健康检查脚本

**功能**：检查数据库的健康状态和性能

**用法**：
```powershell
.\check-database-health.ps1 [参数]
```

**参数**：
- `-ConnectionString` - 数据库连接字符串
- `-Detailed` - 显示详细信息

### 21. benchmark-databases.ps1 - 数据库基准测试脚本

**功能**：对数据库进行基准测试和性能比较

**用法**：
```powershell
.\benchmark-databases.ps1 [参数]
```

### 22. backup-restore.ps1 - 备份恢复脚本

**功能**：备份和恢复数据库

**用法**：
```powershell
.\backup-restore.ps1 [参数]
```

**参数**：
- `-Action` - 操作类型 (Backup, Restore)
- `-BackupPath` - 备份文件路径
- `-ConnectionString` - 数据库连接字符串

### 23. demo-database-switch.ps1 - 数据库切换演示脚本

**功能**：演示数据库切换过程

**用法**：
```powershell
.\demo-database-switch.ps1
```

## 📋 脚本使用流程

### 首次部署流程
```powershell
# 1. 修复部署配置问题
.\fix-deployment.ps1

# 2. 选择部署方式
# 方式A：完整部署（包含数据库和Redis）
.\deploy.ps1 -Environment production

# 方式B：仅部署API服务（使用外部数据库和Redis）
.\deploy-api-only.ps1 -Environment production
```

### 配置管理流程
```powershell
# 1. 生成加密密钥
.\generate-encryption-key.ps1

# 2. 加密配置文件
.\encrypt-config.ps1 -ConfigFile "appsettings.Production.json"

# 3. 设置配置
.\setup-config.ps1
```

### 数据库管理流程
```powershell
# 1. 测试数据库连接
.\test-database.ps1

# 2. 执行数据库迁移
.\migrate-database.ps1 -DatabaseType PostgreSQL -ConnectionString "your_connection_string"

# 3. 检查数据库健康状态
.\check-database-health.ps1
```

### 日常维护流程
```powershell
# 1. 更新API服务
.\update-api.ps1 -Backup

# 2. 修复容器问题（如需要）
.\fix-container-issues.ps1

# 3. 测试服务状态
.\test-api.ps1
.\test-cache.ps1
```

### 故障排除流程
```powershell
# 1. 检查容器状态
docker ps -a

# 2. 查看容器日志
docker logs llxrice_api

# 3. 修复容器问题
.\fix-container-issues.ps1 -Force

# 4. 测试连接
.\test-connection.ps1
.\test-database.ps1
.\test-redis-connection.ps1

# 5. 重启服务
docker restart llxrice_api
```

## ⚠️ 注意事项

### PowerShell 执行策略
```powershell
# 设置执行策略（以管理员身份运行）
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### 环境配置
- 确保 `.env` 文件在项目根目录
- 验证数据库和 Redis 连接配置
- 检查 Docker Desktop 是否安装并运行

### 网络问题
- 容器内使用 `host.docker.internal` 访问宿主机服务
- 确保 Windows 防火墙允许相应端口访问

### 日志问题
- 日志文件权限问题可通过 `.\fix-container-issues.ps1 -LogsOnly` 修复
- 日志目录：`.\logs\`

## 🔗 相关文档

- [API服务独立部署指南](./API服务独立部署指南.md)
- [API服务快速部署指南](./API服务快速部署指南.md)
- [部署方案总结](./部署方案总结.md)
- [容器运行问题修复指南](./容器运行问题修复指南.md)
- [多环境配置管理指南](./多环境配置管理指南.md)

## 📞 技术支持

如遇到问题，请：
1. 查看脚本帮助信息：`.\script-name.ps1 -Help`
2. 检查容器日志：`docker logs llxrice_api`
3. 运行问题修复脚本：`.\fix-container-issues.ps1`
4. 运行连接测试：`.\test-connection.ps1`
5. 参考相关文档和故障排除指南
