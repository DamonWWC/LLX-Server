# 批量加密所有配置文件脚本
# 一次性加密项目中的所有配置文件

Write-Host "===========================================
  批量配置文件加密工具
===========================================" -ForegroundColor Cyan

# 检查加密密钥
if (-not $env:RILEY_ENCRYPTION_KEY) {
    Write-Host "`n✗ 错误: 未设置加密密钥环境变量 RILEY_ENCRYPTION_KEY" -ForegroundColor Red
    Write-Host "`n请先执行以下操作之一:"
    Write-Host "  1. 生成新密钥: .\scripts\generate-encryption-key.ps1"
    Write-Host "  2. 设置环境变量: `$env:RILEY_ENCRYPTION_KEY='your-key'`n"
    exit 1
}

Write-Host "`n✓ 已检测到加密密钥环境变量`n" -ForegroundColor Green

# 切换到项目目录
Set-Location (Split-Path $PSScriptRoot)

# 定义要加密的配置文件列表
$configFiles = @(
    "appsettings.json",
    "appsettings.Development.json",
    "appsettings.Production.json",
    "appsettings.Staging.json"
)

$successCount = 0
$failedCount = 0
$skippedCount = 0
$processedFiles = @()

Write-Host "开始处理配置文件...`n" -ForegroundColor Cyan

foreach ($configFile in $configFiles) {
    if (Test-Path $configFile) {
        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
        Write-Host "处理: $configFile" -ForegroundColor Cyan
        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
        
        try {
            $output = dotnet run -- encrypt-config $configFile 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                if ($output -match "新加密字段: 0") {
                    Write-Host "⊘ 跳过: $configFile (已全部加密)`n" -ForegroundColor Yellow
                    $skippedCount++
                } else {
                    Write-Host "✓ 成功: $configFile`n" -ForegroundColor Green
                    $successCount++
                    $processedFiles += $configFile
                }
            } else {
                Write-Host "✗ 失败: $configFile`n" -ForegroundColor Red
                $failedCount++
            }
        } catch {
            Write-Host "✗ 错误: $configFile - $_`n" -ForegroundColor Red
            $failedCount++
        }
    } else {
        Write-Host "⊘ 跳过: $configFile (文件不存在)" -ForegroundColor DarkGray
        $skippedCount++
    }
}

# 显示汇总信息
Write-Host "`n===========================================
  处理完成
===========================================" -ForegroundColor Cyan

Write-Host "`n统计信息:" -ForegroundColor White
Write-Host "  ✓ 成功: $successCount" -ForegroundColor Green
Write-Host "  ✗ 失败: $failedCount" -ForegroundColor Red
Write-Host "  ⊘ 跳过: $skippedCount" -ForegroundColor Yellow
Write-Host "  ━ 总计: $($configFiles.Count)" -ForegroundColor Gray

if ($processedFiles.Count -gt 0) {
    Write-Host "`n已加密的文件:" -ForegroundColor White
    foreach ($file in $processedFiles) {
        Write-Host "  • $file" -ForegroundColor Green
    }
}

# 显示备份文件信息
$backupFiles = Get-ChildItem -Path . -Filter "*.backup.*" | Sort-Object LastWriteTime -Descending | Select-Object -First 10
if ($backupFiles) {
    Write-Host "`n最近的备份文件:" -ForegroundColor White
    foreach ($backup in $backupFiles) {
        Write-Host "  • $($backup.Name) ($(Get-Date $backup.LastWriteTime -Format 'yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray
    }
    Write-Host "`n提示: 如需恢复，可以使用这些备份文件" -ForegroundColor Yellow
}

Write-Host "`n===========================================
  安全提醒
===========================================" -ForegroundColor Yellow
Write-Host "
1. 请勿将加密密钥提交到版本控制系统
2. 在生产环境部署时设置环境变量 RILEY_ENCRYPTION_KEY
3. 妥善保管备份文件（.backup.*）
4. 定期更换加密密钥以提高安全性
" -ForegroundColor Gray

if ($failedCount -eq 0) {
    Write-Host "✓ 所有配置文件处理完成!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠ 部分配置文件处理失败，请检查错误信息" -ForegroundColor Yellow
    exit 1
}

