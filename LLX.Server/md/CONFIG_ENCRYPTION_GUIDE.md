# 配置文件加密指南

本文档说明如何使用配置加密功能保护项目中的敏感信息。

## 📋 目录

- [概述](#概述)
- [快速开始](#快速开始)
- [详细步骤](#详细步骤)
- [命令参考](#命令参考)
- [环境变量](#环境变量)
- [常见问题](#常见问题)

## 概述

配置加密功能使用 **AES-256** 加密算法保护配置文件中的敏感信息，特别是数据库连接字符串。

### 特性

- ✅ AES-256 加密算法
- ✅ 自动加密/解密配置
- ✅ 支持多环境配置
- ✅ 命令行工具
- ✅ 自动备份原始文件
- ✅ 密码遮蔽显示

### 加密前缀

所有加密的值都以 `ENC:` 前缀标识：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "ENC:base64EncodedEncryptedValue..."
  }
}
```

## 快速开始

### 1. 生成加密密钥

```powershell
# 生成新的加密密钥
dotnet run -- generate-key

# 输出示例：
# 已生成新的加密密钥:
# a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6A7B8C9D0E1F2
```

### 2. 设置环境变量

**Windows PowerShell:**
```powershell
$env:RILEY_ENCRYPTION_KEY="你生成的密钥"
```

**Windows CMD:**
```cmd
set RILEY_ENCRYPTION_KEY=你生成的密钥
```

**Linux/Mac:**
```bash
export RILEY_ENCRYPTION_KEY="你生成的密钥"
```

### 3. 加密配置文件

```powershell
# 加密默认配置文件 (appsettings.json)
dotnet run -- encrypt-config

# 或加密指定配置文件
dotnet run -- encrypt-config appsettings.Production.json
```

## 详细步骤

### 步骤 1: 准备配置文件

确保你的配置文件包含 `ConnectionStrings` 节点：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mydb;Username=user;Password=secret",
    "Redis": "localhost:6379,password=myredispassword"
  }
}
```

### 步骤 2: 配置加密密钥

有两种方式配置加密密钥：

#### 方式 1: 环境变量（推荐用于生产环境）

```powershell
$env:RILEY_ENCRYPTION_KEY="your-64-character-encryption-key-here"
```

#### 方式 2: appsettings.json（可用于开发环境）

```json
{
  "EncryptionSettings": {
    "Key": "your-64-character-encryption-key-here"
  }
}
```

⚠️ **注意**: 生产环境必须使用环境变量，不要将密钥直接写入配置文件！

### 步骤 3: 运行加密命令

```powershell
cd LLX.Server
dotnet run -- encrypt-config appsettings.json
```

输出示例：
```
正在处理配置文件: appsettings.json

✓ [DefaultConnection] 加密成功
   原文: Host=localhost;Port=5432;Database=mydb;Username=user;Password=****
   密文: ENC:abc123...

✓ [Redis] 加密成功
   原文: localhost:6379,password=****
   密文: ENC:def456...

===========================================
处理完成:
  - 新加密字段: 2
  - 已加密字段: 0
  - 配置文件: appsettings.json
===========================================

✓ 原始配置文件已备份到: appsettings.json.backup.20251021123456
✓ 配置文件加密完成!
```

### 步骤 4: 验证加密结果

查看加密后的配置文件：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "ENC:YourEncryptedValueHere...",
    "Redis": "ENC:AnotherEncryptedValueHere..."
  }
}
```

### 步骤 5: 应用自动解密

应用程序会在启动时自动解密配置。无需手动解密！

```csharp
// 应用程序会自动解密并使用
var connectionString = configuration.GetConnectionString("DefaultConnection");
// 返回解密后的实际连接字符串
```

## 命令参考

### generate-key - 生成加密密钥

生成一个新的 64 字符随机密钥。

```powershell
dotnet run -- generate-key
```

**输出:**
- 新生成的密钥
- 设置环境变量的示例命令

### encrypt - 加密单个字符串

加密一个字符串并返回加密结果。

```powershell
dotnet run -- encrypt "要加密的字符串"
```

**示例:**
```powershell
dotnet run -- encrypt "Host=localhost;Database=test;Password=secret"
```

**输出:**
```
加密成功!
原始字符串: Host=localhost;Database=test;Password=secret
加密字符串: ENC:abc123def456...
```

### decrypt - 解密单个字符串

解密一个已加密的字符串。

```powershell
dotnet run -- decrypt "ENC:加密的字符串"
```

**示例:**
```powershell
dotnet run -- decrypt "ENC:abc123def456..."
```

### encrypt-config - 加密配置文件

自动扫描并加密配置文件中 `ConnectionStrings` 节点下的所有连接字符串。

```powershell
# 加密默认配置文件
dotnet run -- encrypt-config

# 加密指定配置文件
dotnet run -- encrypt-config appsettings.Production.json
```

**功能:**
- 自动识别未加密的连接字符串
- 跳过已加密的字符串
- 自动创建备份文件
- 保持 JSON 格式和缩进

### help - 显示帮助

显示所有可用命令的帮助信息。

```powershell
dotnet run -- help
```

## 环境变量

### RILEY_ENCRYPTION_KEY

**用途:** 存储配置加密密钥

**设置方法:**

**Windows PowerShell (临时):**
```powershell
$env:RILEY_ENCRYPTION_KEY="your-key-here"
```

**Windows PowerShell (永久):**
```powershell
[System.Environment]::SetEnvironmentVariable("RILEY_ENCRYPTION_KEY", "your-key-here", "User")
```

**Windows CMD:**
```cmd
set RILEY_ENCRYPTION_KEY=your-key-here
```

**Linux/Mac (临时):**
```bash
export RILEY_ENCRYPTION_KEY="your-key-here"
```

**Linux/Mac (永久 - 添加到 ~/.bashrc 或 ~/.zshrc):**
```bash
echo 'export RILEY_ENCRYPTION_KEY="your-key-here"' >> ~/.bashrc
source ~/.bashrc
```

**Docker/Docker Compose:**
```yaml
services:
  app:
    environment:
      - RILEY_ENCRYPTION_KEY=your-key-here
```

## 配置文件管理

### 开发环境配置

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dev;Username=dev;Password=dev",
    "Redis": "localhost:6379"
  },
  "EncryptionSettings": {
    "Key": "dev-key-can-be-stored-in-file"
  }
}
```

### 生产环境配置

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "ENC:encrypted-production-connection-string",
    "Redis": "ENC:encrypted-redis-connection-string"
  }
}
```

⚠️ **不要在生产配置中包含 EncryptionSettings，使用环境变量！**

## 常见问题

### Q1: 加密密钥应该多长？

**A:** 密钥长度必须至少 32 个字符。建议使用 64 字符的随机密钥，可以使用 `generate-key` 命令生成。

### Q2: 如何在生产环境部署？

**A:** 
1. 使用 `encrypt-config` 加密生产配置文件
2. 将加密后的配置文件部署到服务器
3. 在服务器设置 `RILEY_ENCRYPTION_KEY` 环境变量
4. 启动应用程序，自动解密配置

### Q3: 忘记加密密钥怎么办？

**A:** 如果忘记密钥，无法解密配置。建议：
1. 妥善保管备份文件（.backup.* 文件）
2. 将密钥存储在安全的密钥管理系统中
3. 团队成员共享密钥时使用安全渠道

### Q4: 如何更换加密密钥？

**A:**
1. 使用旧密钥解密配置文件（或使用备份文件）
2. 生成新密钥：`dotnet run -- generate-key`
3. 设置新的环境变量
4. 重新加密配置文件

### Q5: 配置文件已经加密，还能再次运行 encrypt-config 吗？

**A:** 可以！命令会自动识别已加密的字段并跳过它们，只加密未加密的字段。

### Q6: 解密失败怎么办？

**A:** 常见原因：
- 环境变量未设置或设置错误
- 使用了错误的密钥
- 配置文件损坏

解决方法：
1. 检查环境变量是否正确设置
2. 查看应用程序日志获取详细错误信息
3. 使用备份文件恢复配置

### Q7: 开发环境可以不加密吗？

**A:** 可以。开发环境可以使用明文配置。但建议加密，以便：
- 测试加密功能
- 保持环境一致性
- 培养安全意识

### Q8: 如何查看加密前的原始配置？

**A:** 
- 每次加密都会自动创建备份文件（.backup.*）
- 或使用 `decrypt` 命令解密单个字符串

## 安全建议

### ✅ 推荐做法

1. **生产环境使用环境变量存储密钥**
2. **定期轮换加密密钥**
3. **不要将密钥提交到版本控制系统**
4. **使用强随机密钥（64+ 字符）**
5. **妥善保管备份文件**
6. **限制配置文件访问权限**

### ❌ 不推荐做法

1. 在代码中硬编码密钥
2. 将密钥发送到日志或控制台
3. 使用简单或短的密钥
4. 在多个环境使用相同密钥
5. 通过不安全渠道传输密钥

## 支持与反馈

如有问题或建议，请联系开发团队。

---

**最后更新:** 2024年10月

