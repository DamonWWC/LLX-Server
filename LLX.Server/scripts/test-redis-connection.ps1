# Redis 连接测试脚本
# 用于验证新的 Redis 配置是否正常工作

Write-Host "=== Redis 连接测试脚本 ===" -ForegroundColor Green

# 启动应用程序（后台运行）
Write-Host "启动应用程序..." -ForegroundColor Yellow
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -NoNewWindow

# 等待应用程序启动
Write-Host "等待应用程序启动..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

try {
    # 测试健康检查端点
    $baseUrl = "http://localhost:5000"
    
    Write-Host "测试健康检查端点..." -ForegroundColor Cyan
    $healthResponse = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    
    Write-Host "健康检查结果:" -ForegroundColor Green
    $healthResponse | ConvertTo-Json -Depth 3
    
    # 检查 Redis 状态
    if ($healthResponse.status -eq "Healthy") {
        Write-Host "✅ 应用程序健康检查通过" -ForegroundColor Green
        
        # 检查 Redis 连接状态
        $redisStatus = $healthResponse.checks | Where-Object { $_.name -like "*Redis*" }
        if ($redisStatus) {
            if ($redisStatus.status -eq "Healthy") {
                Write-Host "✅ Redis 连接正常" -ForegroundColor Green
                Write-Host "Redis 服务器: 118.126.105.146:6379" -ForegroundColor White
            } else {
                Write-Host "❌ Redis 连接失败: $($redisStatus.status)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "❌ 应用程序健康检查失败" -ForegroundColor Red
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
Write-Host "如果看到 ✅ Redis 连接正常，说明配置成功" -ForegroundColor Yellow
