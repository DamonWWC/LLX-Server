# 配置加密速查表 🚀

快速参考卡片 - 打印此页面放在桌面上！

## 🔑 环境变量

```powershell
# Windows PowerShell
$env:RILEY_ENCRYPTION_KEY="your-64-character-key-here"

# Linux/Mac
export RILEY_ENCRYPTION_KEY="your-64-character-key-here"
```

## ⚡ 常用命令

```powershell
# 生成密钥
dotnet run -- generate-key
.\scripts\generate-encryption-key.ps1

# 加密字符串
dotnet run -- encrypt "your-string"

# 解密字符串  
dotnet run -- decrypt "ENC:..."

# 加密配置文件
dotnet run -- encrypt-config appsettings.json
.\scripts\encrypt-config.ps1

# 批量加密
.\scripts\encrypt-all-configs.ps1

# 帮助
dotnet run -- help
```

## 📝 快速开始

```powershell
# 1️⃣ 生成密钥
.\scripts\generate-encryption-key.ps1

# 2️⃣ 设置环境变量
$env:RILEY_ENCRYPTION_KEY="生成的密钥"

# 3️⃣ 加密配置
.\scripts\encrypt-config.ps1

# ✅ 完成！运行应用
dotnet run
```

## 🎯 典型场景

### 开发环境

```powershell
# 可选加密，密钥可放配置文件
{
  "EncryptionSettings": {
    "Key": "dev-key-32-chars-minimum"
  }
}
```

### 生产环境

```powershell
# 必须加密，密钥用环境变量
$env:RILEY_ENCRYPTION_KEY="prod-key"
dotnet run -- encrypt-config appsettings.Production.json
```

### Docker 部署

```yaml
services:
  app:
    environment:
      - RILEY_ENCRYPTION_KEY=your-key
```

## 🔧 配置格式

### 加密前
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Password=secret"
  }
}
```

### 加密后
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "ENC:YWJjZGVm..."
  }
}
```

## 🚨 故障排查

| 错误 | 原因 | 解决 |
|------|------|------|
| 未配置加密密钥 | 环境变量未设置 | 设置 `RILEY_ENCRYPTION_KEY` |
| 解密失败 | 密钥错误 | 检查环境变量或使用备份 |
| 连接失败 | 配置错误 | 查看 `*.backup.*` 文件 |

## 📁 重要文件

```
LLX.Server/
├── appsettings.json              # 当前配置
├── appsettings.Production.json   # 生产配置
├── appsettings.json.backup.*     # 自动备份
└── scripts/
    ├── generate-encryption-key.ps1
    ├── encrypt-config.ps1
    └── encrypt-all-configs.ps1
```

## ⚠️ 安全检查清单

- [ ] 密钥至少32字符
- [ ] 生产环境使用环境变量
- [ ] 密钥未提交到Git
- [ ] 配置文件已加密
- [ ] 备份文件已保存
- [ ] 不同环境不同密钥
- [ ] 应用启动测试通过

## 🆘 获取帮助

```powershell
# 查看完整文档
cat CONFIG_ENCRYPTION_README.md

# 查看快速开始
cat CONFIG_ENCRYPTION_QUICKSTART.md

# 查看详细指南
cat CONFIG_ENCRYPTION_GUIDE.md
```

## 📞 常见问题

**Q: 密钥长度要求？**  
A: 至少32字符，推荐64字符

**Q: 开发环境必须加密吗？**  
A: 不强制，但推荐加密以保持一致性

**Q: 忘记密钥怎么办？**  
A: 使用 `*.backup.*` 备份文件恢复

**Q: 如何更换密钥？**  
A: 恢复备份 → 设置新密钥 → 重新加密

**Q: 支持哪些数据库？**  
A: 支持所有数据库连接字符串

**Q: 性能影响？**  
A: 仅启动时解密，运行时无影响

## 🎓 最佳实践

✅ **DO**
- 使用 `generate-key` 生成密钥
- 生产环境用环境变量
- 定期轮换密钥
- 保留备份文件

❌ **DON'T**
- 硬编码密钥
- 提交密钥到Git
- 多环境共用密钥
- 忽略安全警告

---

**提示:** 将此页面打印或保存书签，随时查阅！

**文档版本:** 1.0.0  
**更新日期:** 2024-10-21

