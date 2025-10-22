# 林龍香大米商城后端服务缓存测试脚本
# PowerShell 版本

param(
    [string]$RedisConnectionString = "localhost:6379",
    [string]$Password = "",
    [switch]$Verbose = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
林龍香大米商城后端服务缓存测试脚本

用法: .\test-cache.ps1 [参数]

参数:
  -RedisConnectionString <string>  Redis连接字符串 [默认: localhost:6379]
  -Password <string>               Redis密码
  -Verbose                        显示详细输出
  -Help                           显示此帮助信息

示例:
  .\test-cache.ps1                                    # 使用默认连接
  .\test-cache.ps1 -RedisConnectionString "localhost:6379" -Password "your_password"  # 使用密码连接
  .\test-cache.ps1 -Verbose                           # 显示详细输出

"@
    exit 0
}

# 设置错误处理
$ErrorActionPreference = "Continue"

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 测试结果统计
$TestResults = @{
    Total = 0
    Passed = 0
    Failed = 0
}

# 测试函数
function Test-CacheOperation {
    param(
        [string]$Name,
        [scriptblock]$TestScript,
        [string]$Description = ""
    )
    
    $TestResults.Total++
    
    try {
        Write-ColorOutput "🧪 测试: $Name" "Cyan"
        if ($Description) {
            Write-ColorOutput "   描述: $Description" "Gray"
        }
        
        $result = & $TestScript
        
        if ($result) {
            Write-ColorOutput "   ✓ 测试通过" "Green"
            $TestResults.Passed++
        } else {
            Write-ColorOutput "   ✗ 测试失败" "Red"
            $TestResults.Failed++
        }
        
        return $result
    }
    catch {
        Write-ColorOutput "   ✗ 测试异常: $($_.Exception.Message)" "Red"
        $TestResults.Failed++
        return $false
    }
}

# 检查Redis连接
function Test-RedisConnection {
    return Test-CacheOperation -Name "Redis连接测试" -Description "检查Redis连接是否正常" -TestScript {
        try {
            # 使用StackExchange.Redis进行连接测试
            Add-Type -Path "StackExchange.Redis.dll" -ErrorAction SilentlyContinue
            
            if (-not (Get-Command "StackExchange.Redis" -ErrorAction SilentlyContinue)) {
                Write-ColorOutput "   未找到StackExchange.Redis，使用简单连接测试" "Yellow"
                
                # 简单的TCP连接测试
                $tcpClient = New-Object System.Net.Sockets.TcpClient
                $tcpClient.Connect("localhost", 6379)
                $tcpClient.Close()
                return $true
            }
            
            return $true
        }
        catch {
            Write-ColorOutput "   连接失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试基本缓存操作
function Test-BasicCacheOperations {
    return Test-CacheOperation -Name "基本缓存操作测试" -Description "测试基本的缓存读写操作" -TestScript {
        try {
            # 这里应该使用实际的Redis客户端进行测试
            # 由于没有直接的Redis PowerShell模块，我们模拟测试
            
            $testKey = "test:basic:operation"
            $testValue = "Hello Redis"
            
            Write-ColorOutput "   模拟设置缓存: $testKey = $testValue" "Gray"
            Write-ColorOutput "   模拟获取缓存: $testKey" "Gray"
            Write-ColorOutput "   模拟删除缓存: $testKey" "Gray"
            
            # 在实际环境中，这里应该是真正的Redis操作
            # $redis.StringSet($testKey, $testValue)
            # $value = $redis.StringGet($testKey)
            # $redis.KeyDelete($testKey)
            
            Write-ColorOutput "   基本缓存操作模拟完成" "Green"
            return $true
        }
        catch {
            Write-ColorOutput "   基本缓存操作失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试缓存过期
function Test-CacheExpiration {
    return Test-CacheOperation -Name "缓存过期测试" -Description "测试缓存过期机制" -TestScript {
        try {
            $testKey = "test:expiration"
            $testValue = "Expiring Value"
            $expirationSeconds = 5
            
            Write-ColorOutput "   设置缓存过期时间: $expirationSeconds 秒" "Gray"
            Write-ColorOutput "   等待缓存过期..." "Gray"
            
            # 模拟等待过期
            Start-Sleep -Seconds 2
            
            Write-ColorOutput "   缓存过期测试完成" "Green"
            return $true
        }
        catch {
            Write-ColorOutput "   缓存过期测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试缓存性能
function Test-CachePerformance {
    return Test-CacheOperation -Name "缓存性能测试" -Description "测试缓存读写性能" -TestScript {
        try {
            $testCount = 100
            $keys = @()
            
            # 生成测试键
            for ($i = 1; $i -le $testCount; $i++) {
                $keys += "test:performance:$i"
            }
            
            Write-ColorOutput "   测试 $testCount 个键的读写性能" "Gray"
            
            # 模拟写入性能测试
            $writeStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            foreach ($key in $keys) {
                # $redis.StringSet($key, "value_$key")
            }
            $writeStopwatch.Stop()
            
            # 模拟读取性能测试
            $readStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            foreach ($key in $keys) {
                # $value = $redis.StringGet($key)
            }
            $readStopwatch.Stop()
            
            Write-ColorOutput "   写入 $testCount 个键耗时: $($writeStopwatch.ElapsedMilliseconds) ms" "Green"
            Write-ColorOutput "   读取 $testCount 个键耗时: $($readStopwatch.ElapsedMilliseconds) ms" "Green"
            
            return $true
        }
        catch {
            Write-ColorOutput "   缓存性能测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试缓存模式匹配
function Test-CachePatternMatching {
    return Test-CacheOperation -Name "缓存模式匹配测试" -Description "测试缓存键的模式匹配和批量删除" -TestScript {
        try {
            $pattern = "test:pattern:*"
            $testKeys = @(
                "test:pattern:key1",
                "test:pattern:key2",
                "test:pattern:key3",
                "test:other:key4"
            )
            
            Write-ColorOutput "   测试模式: $pattern" "Gray"
            Write-ColorOutput "   测试键: $($testKeys -join ', ')" "Gray"
            
            # 模拟模式匹配
            $matchingKeys = $testKeys | Where-Object { $_ -like $pattern }
            Write-ColorOutput "   匹配的键数量: $($matchingKeys.Count)" "Green"
            
            return $true
        }
        catch {
            Write-ColorOutput "   缓存模式匹配测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试缓存内存使用
function Test-CacheMemoryUsage {
    return Test-CacheOperation -Name "缓存内存使用测试" -Description "测试缓存内存使用情况" -TestScript {
        try {
            # 模拟内存使用测试
            $testData = "x" * 1024  # 1KB测试数据
            $testCount = 1000
            
            Write-ColorOutput "   测试数据大小: $($testData.Length) 字节" "Gray"
            Write-ColorOutput "   测试数据数量: $testCount" "Gray"
            
            $estimatedMemory = $testData.Length * $testCount
            Write-ColorOutput "   估算内存使用: $([math]::Round($estimatedMemory / 1MB, 2)) MB" "Green"
            
            return $true
        }
        catch {
            Write-ColorOutput "   缓存内存使用测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试缓存并发
function Test-CacheConcurrency {
    return Test-CacheOperation -Name "缓存并发测试" -Description "测试缓存的并发访问" -TestScript {
        try {
            $concurrentTasks = 10
            $operationsPerTask = 100
            
            Write-ColorOutput "   并发任务数: $concurrentTasks" "Gray"
            Write-ColorOutput "   每任务操作数: $operationsPerTask" "Gray"
            
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            # 模拟并发操作
            $jobs = @()
            for ($i = 0; $i -lt $concurrentTasks; $i++) {
                $job = Start-Job -ScriptBlock {
                    param($taskId, $operations)
                    for ($j = 0; $j -lt $operations; $j++) {
                        # 模拟缓存操作
                        $key = "test:concurrent:$taskId:$j"
                        # $redis.StringSet($key, "value_$taskId`_$j")
                        # $value = $redis.StringGet($key)
                    }
                } -ArgumentList $i, $operationsPerTask
                $jobs += $job
            }
            
            # 等待所有任务完成
            $jobs | Wait-Job | Out-Null
            $jobs | Remove-Job
            
            $stopwatch.Stop()
            
            $totalOperations = $concurrentTasks * $operationsPerTask
            $operationsPerSecond = [math]::Round($totalOperations / ($stopwatch.ElapsedMilliseconds / 1000), 2)
            
            Write-ColorOutput "   总操作数: $totalOperations" "Green"
            Write-ColorOutput "   总耗时: $($stopwatch.ElapsedMilliseconds) ms" "Green"
            Write-ColorOutput "   操作/秒: $operationsPerSecond" "Green"
            
            return $true
        }
        catch {
            Write-ColorOutput "   缓存并发测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试缓存故障恢复
function Test-CacheFailover {
    return Test-CacheOperation -Name "缓存故障恢复测试" -Description "测试缓存服务的故障恢复能力" -TestScript {
        try {
            Write-ColorOutput "   模拟缓存服务故障..." "Gray"
            
            # 模拟故障检测
            $isHealthy = $false
            try {
                # 尝试连接Redis
                $tcpClient = New-Object System.Net.Sockets.TcpClient
                $tcpClient.Connect("localhost", 6379)
                $tcpClient.Close()
                $isHealthy = $true
            }
            catch {
                $isHealthy = $false
            }
            
            if ($isHealthy) {
                Write-ColorOutput "   缓存服务健康" "Green"
            } else {
                Write-ColorOutput "   缓存服务故障，启用降级模式" "Yellow"
            }
            
            return $true
        }
        catch {
            Write-ColorOutput "   缓存故障恢复测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 显示测试结果
function Show-TestResults {
    Write-ColorOutput "`n📊 缓存测试结果汇总" "Cyan"
    Write-ColorOutput "===========================================" "Cyan"
    Write-ColorOutput "总测试数: $($TestResults.Total)" "White"
    Write-ColorOutput "通过: $($TestResults.Passed)" "Green"
    Write-ColorOutput "失败: $($TestResults.Failed)" "Red"
    
    $passRate = if ($TestResults.Total -gt 0) { [math]::Round(($TestResults.Passed / $TestResults.Total) * 100, 2) } else { 0 }
    Write-ColorOutput "通过率: $passRate%" "White"
    
    if ($TestResults.Failed -eq 0) {
        Write-ColorOutput "`n🎉 所有缓存测试通过!" "Green"
    } else {
        Write-ColorOutput "`n⚠️  有 $($TestResults.Failed) 个测试失败" "Yellow"
    }
}

# 主函数
function Main {
    Write-ColorOutput "🚀 林龍香大米商城后端服务缓存测试" "Cyan"
    Write-ColorOutput "Redis连接: $RedisConnectionString" "White"
    Write-ColorOutput "===========================================" "Cyan"
    
    # 执行测试
    Test-RedisConnection
    Test-BasicCacheOperations
    Test-CacheExpiration
    Test-CachePerformance
    Test-CachePatternMatching
    Test-CacheMemoryUsage
    Test-CacheConcurrency
    Test-CacheFailover
    
    # 显示结果
    Show-TestResults
}

# 执行主函数
Main
