# 生成加密密钥脚本
# 用于生成安全的随机加密密钥

Write-Host "===========================================
  加密密钥生成器
===========================================" -ForegroundColor Cyan

# 切换到项目目录
Set-Location (Split-Path $PSScriptRoot)

Write-Host "`n正在生成新的加密密钥...`n" -ForegroundColor Green

# 执行密钥生成命令
dotnet run -- generate-key

Write-Host "`n===========================================
  安全提示
===========================================" -ForegroundColor Yellow
Write-Host "
1. 请妥善保管此密钥，丢失后无法恢复加密的配置
2. 不要将密钥提交到版本控制系统
3. 生产环境使用环境变量存储密钥
4. 定期更换密钥以提高安全性
5. 不同环境使用不同的密钥
" -ForegroundColor Gray

# 询问是否立即设置环境变量
Write-Host "是否现在设置此密钥为环境变量? (Y/N): " -NoNewline -ForegroundColor Cyan
$response = Read-Host

if ($response -eq 'Y' -or $response -eq 'y') {
    Write-Host "`n请输入生成的密钥: " -NoNewline -ForegroundColor Cyan
    $key = Read-Host
    
    if ($key) {
        # 设置当前会话的环境变量
        $env:RILEY_ENCRYPTION_KEY = $key
        Write-Host "✓ 已设置当前会话的环境变量" -ForegroundColor Green
        
        # 询问是否永久设置
        Write-Host "`n是否永久设置此环境变量（用户级别）? (Y/N): " -NoNewline -ForegroundColor Cyan
        $permanentResponse = Read-Host
        
        if ($permanentResponse -eq 'Y' -or $permanentResponse -eq 'y') {
            try {
                [System.Environment]::SetEnvironmentVariable("RILEY_ENCRYPTION_KEY", $key, "User")
                Write-Host "✓ 已永久设置环境变量（用户级别）" -ForegroundColor Green
                Write-Host "  注意: 需要重新打开终端才能生效" -ForegroundColor Yellow
            } catch {
                Write-Host "✗ 设置永久环境变量失败: $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host "`n===========================================
  下一步操作
===========================================" -ForegroundColor Cyan
Write-Host "
1. 运行加密命令:
   .\scripts\encrypt-config.ps1

2. 或手动加密配置文件:
   dotnet run -- encrypt-config appsettings.json

3. 查看完整文档:
   cat CONFIG_ENCRYPTION_GUIDE.md
" -ForegroundColor Gray

