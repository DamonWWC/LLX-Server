# 日志系统测试脚本
# 用于验证日志系统是否正常工作

Write-Host "=== 日志系统测试脚本 ===" -ForegroundColor Green

# 检查日志目录
$logDir = "logs"
if (-not (Test-Path $logDir)) {
    Write-Host "创建日志目录: $logDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

# 启动应用程序（后台运行）
Write-Host "启动应用程序..." -ForegroundColor Yellow
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -NoNewWindow

# 等待应用程序启动
Write-Host "等待应用程序启动..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

try {
    # 测试日志端点
    $baseUrl = "http://localhost:5000"
    
    Write-Host "测试日志级别..." -ForegroundColor Cyan
    Invoke-RestMethod -Uri "$baseUrl/api/logging-test/test-levels" -Method Get
    
    Write-Host "测试结构化日志..." -ForegroundColor Cyan
    Invoke-RestMethod -Uri "$baseUrl/api/logging-test/test-structured" -Method Get
    
    Write-Host "测试异常日志..." -ForegroundColor Cyan
    Invoke-RestMethod -Uri "$baseUrl/api/logging-test/test-exceptions" -Method Get
    
    Write-Host "测试不同类别日志..." -ForegroundColor Cyan
    Invoke-RestMethod -Uri "$baseUrl/api/logging-test/test-categories" -Method Get
    
    # 获取日志文件信息
    Write-Host "获取日志文件信息..." -ForegroundColor Cyan
    $logFiles = Invoke-RestMethod -Uri "$baseUrl/api/logging-test/log-files" -Method Get
    
    Write-Host "`n=== 日志文件信息 ===" -ForegroundColor Green
    Write-Host "日志目录: $($logFiles.logDirectory)"
    Write-Host "文件数量: $($logFiles.files.Count)"
    
    foreach ($file in $logFiles.files) {
        Write-Host "文件: $($file.name) | 大小: $($file.size) bytes | 修改时间: $($file.lastWriteTime)" -ForegroundColor White
    }
    
    # 显示最新的日志内容
    Write-Host "`n=== 最新日志内容预览 ===" -ForegroundColor Green
    $latestFile = $logFiles.files | Sort-Object lastWriteTime -Descending | Select-Object -First 1
    if ($latestFile) {
        Write-Host "显示文件: $($latestFile.name)" -ForegroundColor Yellow
        $content = Get-Content -Path $latestFile.fullPath -Tail 10
        foreach ($line in $content) {
            Write-Host $line -ForegroundColor Gray
        }
    }
    
} catch {
    Write-Host "测试过程中发生错误: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # 停止应用程序
    Write-Host "`n停止应用程序..." -ForegroundColor Yellow
    if ($process -and !$process.HasExited) {
        $process.Kill()
        $process.WaitForExit()
    }
}

Write-Host "`n=== 测试完成 ===" -ForegroundColor Green
Write-Host "请检查 logs 目录中的日志文件" -ForegroundColor Yellow
