# 🚀 配置加密快速开始

5分钟内完成配置文件加密设置！

## 📌 第一步：生成加密密钥

运行密钥生成脚本：

```powershell
cd LLX.Server
.\scripts\generate-encryption-key.ps1
```

这将生成一个 64 字符的随机密钥，类似于：
```
a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6A7B8C9D0E1F2
```

**重要：** 请妥善保管此密钥！

## 📌 第二步：设置环境变量

### Windows PowerShell

```powershell
# 临时设置（当前会话）
$env:RILEY_ENCRYPTION_KEY="你的密钥"

# 永久设置（推荐）
[System.Environment]::SetEnvironmentVariable("RILEY_ENCRYPTION_KEY", "你的密钥", "User")
```

### Linux/Mac

```bash
# 临时设置
export RILEY_ENCRYPTION_KEY="你的密钥"

# 永久设置（添加到 ~/.bashrc）
echo 'export RILEY_ENCRYPTION_KEY="你的密钥"' >> ~/.bashrc
source ~/.bashrc
```

## 📌 第三步：加密配置文件

### 方法 1: 使用脚本（推荐）

**加密单个文件：**
```powershell
.\scripts\encrypt-config.ps1
```

**加密所有配置文件：**
```powershell
.\scripts\encrypt-all-configs.ps1
```

### 方法 2: 手动命令

```powershell
# 加密默认配置
dotnet run -- encrypt-config

# 加密生产环境配置
dotnet run -- encrypt-config appsettings.Production.json
```

## 📌 第四步：验证结果

打开配置文件，检查连接字符串是否已加密：

**加密前：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mydb;Password=secret"
  }
}
```

**加密后：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "ENC:abc123def456..."
  }
}
```

## 📌 第五步：运行应用

直接运行应用，配置会自动解密：

```powershell
dotnet run
```

应用会在启动时自动解密配置，无需任何额外操作！

## ✅ 完成！

你的配置文件现在已经安全加密了！

---

## 🎯 常用命令速查

```powershell
# 生成新密钥
.\scripts\generate-encryption-key.ps1

# 加密单个文件
.\scripts\encrypt-config.ps1

# 加密所有配置
.\scripts\encrypt-all-configs.ps1

# 手动加密字符串
dotnet run -- encrypt "your-connection-string"

# 手动解密字符串
dotnet run -- decrypt "ENC:encrypted-string"
```

## 📖 更多信息

查看完整文档：[CONFIG_ENCRYPTION_GUIDE.md](CONFIG_ENCRYPTION_GUIDE.md)

## ⚠️ 重要提示

1. **不要**将加密密钥提交到 Git
2. **务必**妥善保管备份文件（.backup.*）
3. **必须**在生产环境设置环境变量
4. **建议**不同环境使用不同的密钥

## 🆘 遇到问题？

### 错误：未配置加密密钥

```
✗ 错误: 未配置加密密钥
```

**解决方法：** 设置 `RILEY_ENCRYPTION_KEY` 环境变量

### 错误：解密失败

```
✗ 解密字符串时发生错误
```

**可能原因：**
- 密钥错误
- 环境变量未设置
- 配置文件损坏

**解决方法：** 
1. 检查环境变量
2. 使用备份文件恢复
3. 查看详细日志

## 📞 获取帮助

查看所有可用命令：
```powershell
dotnet run -- help
```

查看脚本帮助：
```powershell
.\scripts\encrypt-config.ps1 -Help
```

---

**准备好了？** 现在就开始保护你的配置文件吧！ 🔐

