# Windows环境变量配置指南

## 🎯 概述

本指南详细说明如何在Windows系统上配置数据库Provider等环境变量，并在代码中正确获取。

## 🔧 配置方案

### 方案一：使用 .env 文件（推荐）

#### 1. 快速配置

```powershell
# 进入项目根目录
cd E:\资料\学习代码\LLX.Server

# 使用配置脚本快速设置
.\LLX.Server\scripts\setup-config.ps1 -DbPassword "your_db_password" -RedisPassword "your_redis_password"
```

#### 2. 手动配置

```powershell
# 复制示例文件
copy env.api-only.example .env

# 编辑配置文件
notepad .env
```

**关键配置项**：

```bash
# 数据库提供商配置
DB_PROVIDER=PostgreSQL

# 数据库连接配置
DB_CONNECTION_STRING=Host=host.docker.internal;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30

# Redis连接配置
REDIS_CONNECTION_STRING=host.docker.internal:6379,password=your_redis_password,ssl=false,abortConnect=false
```

### 方案二：Windows系统环境变量

#### 1. 通过系统设置

1. **打开系统属性**：
   - 右键"此电脑" → "属性"
   - 点击"高级系统设置"
   - 点击"环境变量"

2. **添加用户环境变量**：
   ```
   变量名: DB_PROVIDER
   变量值: PostgreSQL
   ```

3. **添加更多配置**：
   ```
   DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30
   REDIS_CONNECTION_STRING=localhost:6379,password=your_redis_password,ssl=false,abortConnect=false
   ```

#### 2. 通过PowerShell设置

```powershell
# 设置用户环境变量
[Environment]::SetEnvironmentVariable("DB_PROVIDER", "PostgreSQL", "User")
[Environment]::SetEnvironmentVariable("DB_CONNECTION_STRING", "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30", "User")
[Environment]::SetEnvironmentVariable("REDIS_CONNECTION_STRING", "localhost:6379,password=your_redis_password,ssl=false,abortConnect=false", "User")

# 设置系统环境变量（需要管理员权限）
[Environment]::SetEnvironmentVariable("DB_PROVIDER", "PostgreSQL", "Machine")
```

#### 3. 通过命令行设置

```cmd
# 设置用户环境变量
setx DB_PROVIDER "PostgreSQL"
setx DB_CONNECTION_STRING "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30"
setx REDIS_CONNECTION_STRING "localhost:6379,password=your_redis_password,ssl=false,abortConnect=false"

# 设置系统环境变量（需要管理员权限）
setx DB_PROVIDER "PostgreSQL" /M
```

## 🔍 代码中获取环境变量

### 当前实现

在 `ServiceCollectionExtensions.cs` 中已经正确实现：

```csharp
// 获取数据库配置
var databaseProvider = configuration.GetValue<string>("Database:Provider") ?? "PostgreSQL";
var provider = Enum.Parse<DatabaseProvider>(databaseProvider);
```

### 配置映射

Docker Compose 环境变量映射：

```yaml
environment:
  # 数据库提供商
  - Database__Provider=${DB_PROVIDER:-PostgreSQL}
  
  # 数据库连接配置
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
  
  # Redis连接配置
  - ConnectionStrings__Redis=${REDIS_CONNECTION_STRING}
```

### 配置优先级

1. **环境变量** (最高优先级)
2. **appsettings.json**
3. **默认值**

## 🛠️ 配置工具

### 1. 配置设置脚本

```powershell
# 基本配置
.\LLX.Server\scripts\setup-config.ps1 -DbPassword "your_password" -RedisPassword "your_redis_password"

# 自定义配置
.\LLX.Server\scripts\setup-config.ps1 -Provider "SqlServer" -DbHost "192.168.1.100" -DbPassword "your_password" -RedisPassword "your_redis_password"

# 查看帮助
.\LLX.Server\scripts\setup-config.ps1 -Help
```

### 2. 配置测试脚本

```powershell
# 测试当前配置
.\LLX.Server\scripts\test-config.ps1

# 查看帮助
.\LLX.Server\scripts\test-config.ps1 -Help
```

## 📋 支持的数据库提供商

### PostgreSQL

```bash
DB_PROVIDER=PostgreSQL
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30
```

### SQL Server

```bash
DB_PROVIDER=SqlServer
DB_CONNECTION_STRING=Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true;Pooling=true;Min Pool Size=5;Max Pool Size=100;Command Timeout=30
```

### MySQL

```bash
DB_PROVIDER=MySql
DB_CONNECTION_STRING=Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;DefaultCommandTimeout=30
```

### SQLite

```bash
DB_PROVIDER=Sqlite
DB_CONNECTION_STRING=Data Source=llxrice.db
```

## 🔍 验证配置

### 1. 检查环境变量

```powershell
# 检查用户环境变量
[Environment]::GetEnvironmentVariable("DB_PROVIDER", "User")

# 检查系统环境变量
[Environment]::GetEnvironmentVariable("DB_PROVIDER", "Machine")

# 检查所有环境变量
Get-ChildItem Env: | Where-Object { $_.Name -like "*DB_*" -or $_.Name -like "*REDIS_*" }
```

### 2. 检查.env文件

```powershell
# 查看.env文件内容
Get-Content .env

# 检查特定配置
Select-String -Path .env -Pattern "DB_PROVIDER"
Select-String -Path .env -Pattern "DB_CONNECTION_STRING"
```

### 3. 运行配置测试

```powershell
# 运行配置测试脚本
.\LLX.Server\scripts\test-config.ps1
```

## 🚀 快速开始

### 1. 一键配置

```powershell
# 进入项目目录
cd E:\资料\学习代码\LLX.Server

# 运行配置脚本
.\LLX.Server\scripts\setup-config.ps1 -DbPassword "your_db_password" -RedisPassword "your_redis_password"

# 测试配置
.\LLX.Server\scripts\test-config.ps1

# 部署服务
.\LLX.Server\scripts\deploy-api-only.ps1
```

### 2. 分步配置

```powershell
# 步骤1：创建.env文件
copy env.api-only.example .env

# 步骤2：编辑配置
notepad .env

# 步骤3：测试配置
.\LLX.Server\scripts\test-config.ps1

# 步骤4：部署服务
.\LLX.Server\scripts\deploy-api-only.ps1
```

## 🛠️ 故障排除

### 1. 环境变量未生效

```powershell
# 重启PowerShell会话
exit
# 重新打开PowerShell

# 或者刷新环境变量
$env:DB_PROVIDER = [Environment]::GetEnvironmentVariable("DB_PROVIDER", "User")
```

### 2. .env文件未读取

```powershell
# 检查文件位置
Test-Path .env

# 检查文件内容
Get-Content .env | Select-String "DB_PROVIDER"
```

### 3. Docker Compose配置问题

```powershell
# 检查Docker Compose文件
Get-Content docker-compose.api-only.yml | Select-String "Database__Provider"

# 验证环境变量
docker-compose -f docker-compose.api-only.yml config
```

## 📝 最佳实践

### 1. 配置管理

- 使用 `.env` 文件管理开发环境配置
- 使用系统环境变量管理生产环境配置
- 敏感信息使用加密存储

### 2. 安全考虑

- 不要在代码中硬编码密码
- 使用强密码
- 定期轮换密码

### 3. 版本控制

- 将 `.env.example` 加入版本控制
- 不要将 `.env` 文件加入版本控制
- 使用 `.gitignore` 忽略敏感文件

## 📞 技术支持

如果遇到问题，请提供：

1. **系统信息**：`Get-ComputerInfo`
2. **环境变量**：`Get-ChildItem Env: | Where-Object { $_.Name -like "*DB_*" }`
3. **配置文件**：`Get-Content .env`
4. **错误信息**：完整的错误输出

---

**文档版本**：v1.0  
**最后更新**：2025-10-22  
**适用环境**：Windows 10/11, PowerShell 5.1+
