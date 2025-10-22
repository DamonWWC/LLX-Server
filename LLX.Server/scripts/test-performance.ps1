# 林龍香大米商城后端服务性能测试脚本
# PowerShell 版本

param(
    [string]$BaseUrl = "http://localhost:8080",
    [int]$ConcurrentUsers = 10,
    [int]$TestDuration = 60,
    [int]$RampUpTime = 10,
    [switch]$Verbose = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
林龍香大米商城后端服务性能测试脚本

用法: .\test-performance.ps1 [参数]

参数:
  -BaseUrl <string>        API 基础 URL [默认: http://localhost:8080]
  -ConcurrentUsers <int>   并发用户数 [默认: 10]
  -TestDuration <int>      测试持续时间（秒） [默认: 60]
  -RampUpTime <int>        用户启动时间（秒） [默认: 10]
  -Verbose                 显示详细输出
  -Help                    显示此帮助信息

示例:
  .\test-performance.ps1                                    # 使用默认参数
  .\test-performance.ps1 -ConcurrentUsers 50 -TestDuration 120  # 50个并发用户，测试2分钟
  .\test-performance.ps1 -BaseUrl http://localhost:5000     # 使用自定义URL

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

# 性能测试结果
$PerformanceResults = @{
    TotalRequests = 0
    SuccessfulRequests = 0
    FailedRequests = 0
    ResponseTimes = @()
    StartTime = $null
    EndTime = $null
    Endpoints = @{}
}

# 测试端点配置
$TestEndpoints = @(
    @{
        Name = "健康检查"
        Url = "/health"
        Method = "GET"
        Weight = 10
    },
    @{
        Name = "获取所有商品"
        Url = "/api/products"
        Method = "GET"
        Weight = 30
    },
    @{
        Name = "分页获取商品"
        Url = "/api/products/paged?pageNumber=1&pageSize=20"
        Method = "GET"
        Weight = 25
    },
    @{
        Name = "搜索商品"
        Url = "/api/products/search?name=大米"
        Method = "GET"
        Weight = 15
    },
    @{
        Name = "获取所有地址"
        Url = "/api/addresses"
        Method = "GET"
        Weight = 10
    },
    @{
        Name = "获取运费配置"
        Url = "/api/shipping/rates"
        Method = "GET"
        Weight = 10
    }
)

# 单个请求测试函数
function Test-SingleRequest {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET"
    )
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $success = $false
    $errorMessage = ""
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method $Method -TimeoutSec 30 -ErrorAction Stop
        $success = $true
    }
    catch {
        $errorMessage = $_.Exception.Message
    }
    
    $stopwatch.Stop()
    $responseTime = $stopwatch.ElapsedMilliseconds
    
    $PerformanceResults.TotalRequests++
    if ($success) {
        $PerformanceResults.SuccessfulRequests++
    } else {
        $PerformanceResults.FailedRequests++
    }
    
    $PerformanceResults.ResponseTimes += $responseTime
    
    # 记录端点统计
    if (-not $PerformanceResults.Endpoints.ContainsKey($Name)) {
        $PerformanceResults.Endpoints[$Name] = @{
            TotalRequests = 0
            SuccessfulRequests = 0
            FailedRequests = 0
            ResponseTimes = @()
        }
    }
    
    $PerformanceResults.Endpoints[$Name].TotalRequests++
    if ($success) {
        $PerformanceResults.Endpoints[$Name].SuccessfulRequests++
    } else {
        $PerformanceResults.Endpoints[$Name].FailedRequests++
    }
    $PerformanceResults.Endpoints[$Name].ResponseTimes += $responseTime
    
    return @{
        Success = $success
        ResponseTime = $responseTime
        ErrorMessage = $errorMessage
    }
}

# 用户模拟函数
function Start-UserSimulation {
    param(
        [int]$UserId,
        [int]$Duration
    )
    
    $endTime = (Get-Date).AddSeconds($Duration)
    $requestCount = 0
    
    while ((Get-Date) -lt $endTime) {
        # 根据权重随机选择端点
        $random = Get-Random -Minimum 1 -Maximum 101
        $currentWeight = 0
        $selectedEndpoint = $null
        
        foreach ($endpoint in $TestEndpoints) {
            $currentWeight += $endpoint.Weight
            if ($random -le $currentWeight) {
                $selectedEndpoint = $endpoint
                break
            }
        }
        
        if ($selectedEndpoint) {
            $fullUrl = "$BaseUrl$($selectedEndpoint.Url)"
            $result = Test-SingleRequest -Name $selectedEndpoint.Name -Url $fullUrl -Method $selectedEndpoint.Method
            $requestCount++
            
            if ($Verbose) {
                $status = if ($result.Success) { "✓" } else { "✗" }
                Write-ColorOutput "用户$UserId 请求$requestCount $status $($selectedEndpoint.Name) $($result.ResponseTime)ms" "Gray"
            }
        }
        
        # 随机等待时间（1-3秒）
        $waitTime = Get-Random -Minimum 1000 -Maximum 3000
        Start-Sleep -Milliseconds $waitTime
    }
    
    return $requestCount
}

# 启动性能测试
function Start-PerformanceTest {
    Write-ColorOutput "🚀 开始性能测试" "Cyan"
    Write-ColorOutput "并发用户数: $ConcurrentUsers" "White"
    Write-ColorOutput "测试持续时间: $TestDuration 秒" "White"
    Write-ColorOutput "用户启动时间: $RampUpTime 秒" "White"
    Write-ColorOutput "===========================================" "Cyan"
    
    $PerformanceResults.StartTime = Get-Date
    
    # 创建用户任务
    $userTasks = @()
    $rampUpInterval = $RampUpTime / $ConcurrentUsers
    
    for ($i = 1; $i -le $ConcurrentUsers; $i++) {
        $startDelay = [math]::Round($rampUpInterval * ($i - 1) * 1000)
        
        $task = Start-Job -ScriptBlock {
            param($UserId, $Duration, $BaseUrl, $TestEndpoints, $Verbose)
            
            # 重新定义函数（在Job中）
            function Test-SingleRequest {
                param([string]$Name, [string]$Url, [string]$Method = "GET")
                
                $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                $success = $false
                
                try {
                    $response = Invoke-RestMethod -Uri $Url -Method $Method -TimeoutSec 30 -ErrorAction Stop
                    $success = $true
                }
                catch {
                    # 忽略错误，只记录结果
                }
                
                $stopwatch.Stop()
                return @{
                    Success = $success
                    ResponseTime = $stopwatch.ElapsedMilliseconds
                }
            }
            
            # 用户模拟逻辑
            $endTime = (Get-Date).AddSeconds($Duration)
            $requestCount = 0
            
            while ((Get-Date) -lt $endTime) {
                $random = Get-Random -Minimum 1 -Maximum 101
                $currentWeight = 0
                $selectedEndpoint = $null
                
                foreach ($endpoint in $TestEndpoints) {
                    $currentWeight += $endpoint.Weight
                    if ($random -le $currentWeight) {
                        $selectedEndpoint = $endpoint
                        break
                    }
                }
                
                if ($selectedEndpoint) {
                    $fullUrl = "$BaseUrl$($selectedEndpoint.Url)"
                    $result = Test-SingleRequest -Name $selectedEndpoint.Name -Url $fullUrl -Method $selectedEndpoint.Method
                    $requestCount++
                }
                
                $waitTime = Get-Random -Minimum 1000 -Maximum 3000
                Start-Sleep -Milliseconds $waitTime
            }
            
            return $requestCount
        } -ArgumentList $i, $TestDuration, $BaseUrl, $TestEndpoints, $Verbose
        
        $userTasks += $task
        
        if ($startDelay -gt 0) {
            Start-Sleep -Milliseconds $startDelay
        }
    }
    
    # 等待所有用户完成
    Write-ColorOutput "⏳ 等待测试完成..." "Yellow"
    $userTasks | Wait-Job | Out-Null
    
    # 收集结果
    $totalRequests = 0
    foreach ($task in $userTasks) {
        $result = Receive-Job -Job $task
        $totalRequests += $result
    }
    
    $userTasks | Remove-Job
    
    $PerformanceResults.EndTime = Get-Date
    $PerformanceResults.TotalRequests = $totalRequests
}

# 计算性能指标
function Calculate-PerformanceMetrics {
    $duration = ($PerformanceResults.EndTime - $PerformanceResults.StartTime).TotalSeconds
    $responseTimes = $PerformanceResults.ResponseTimes
    
    if ($responseTimes.Count -eq 0) {
        return @{
            RequestsPerSecond = 0
            AverageResponseTime = 0
            MinResponseTime = 0
            MaxResponseTime = 0
            P95ResponseTime = 0
            P99ResponseTime = 0
            ErrorRate = 0
        }
    }
    
    $sortedResponseTimes = $responseTimes | Sort-Object
    $count = $sortedResponseTimes.Count
    
    return @{
        RequestsPerSecond = [math]::Round($PerformanceResults.TotalRequests / $duration, 2)
        AverageResponseTime = [math]::Round(($responseTimes | Measure-Object -Average).Average, 2)
        MinResponseTime = ($responseTimes | Measure-Object -Minimum).Minimum
        MaxResponseTime = ($responseTimes | Measure-Object -Maximum).Maximum
        P95ResponseTime = $sortedResponseTimes[[math]::Floor($count * 0.95)]
        P99ResponseTime = $sortedResponseTimes[[math]::Floor($count * 0.99)]
        ErrorRate = [math]::Round(($PerformanceResults.FailedRequests / $PerformanceResults.TotalRequests) * 100, 2)
    }
}

# 显示性能测试结果
function Show-PerformanceResults {
    $metrics = Calculate-PerformanceMetrics
    
    Write-ColorOutput "`n📊 性能测试结果" "Cyan"
    Write-ColorOutput "===========================================" "Cyan"
    Write-ColorOutput "测试持续时间: $([math]::Round(($PerformanceResults.EndTime - $PerformanceResults.StartTime).TotalSeconds, 2)) 秒" "White"
    Write-ColorOutput "并发用户数: $ConcurrentUsers" "White"
    Write-ColorOutput "总请求数: $($PerformanceResults.TotalRequests)" "White"
    Write-ColorOutput "成功请求数: $($PerformanceResults.SuccessfulRequests)" "Green"
    Write-ColorOutput "失败请求数: $($PerformanceResults.FailedRequests)" "Red"
    Write-ColorOutput "错误率: $($metrics.ErrorRate)%" "White"
    Write-ColorOutput "`n📈 性能指标:" "Cyan"
    Write-ColorOutput "请求/秒: $($metrics.RequestsPerSecond)" "Green"
    Write-ColorOutput "平均响应时间: $($metrics.AverageResponseTime) ms" "Green"
    Write-ColorOutput "最小响应时间: $($metrics.MinResponseTime) ms" "Green"
    Write-ColorOutput "最大响应时间: $($metrics.MaxResponseTime) ms" "Green"
    Write-ColorOutput "95%响应时间: $($metrics.P95ResponseTime) ms" "Green"
    Write-ColorOutput "99%响应时间: $($metrics.P99ResponseTime) ms" "Green"
    
    # 显示端点统计
    if ($PerformanceResults.Endpoints.Count -gt 0) {
        Write-ColorOutput "`n📋 端点性能统计:" "Cyan"
        foreach ($endpoint in $PerformanceResults.Endpoints.GetEnumerator()) {
            $endpointMetrics = $endpoint.Value
            $avgResponseTime = if ($endpointMetrics.ResponseTimes.Count -gt 0) {
                [math]::Round(($endpointMetrics.ResponseTimes | Measure-Object -Average).Average, 2)
            } else { 0 }
            
            $errorRate = if ($endpointMetrics.TotalRequests -gt 0) {
                [math]::Round(($endpointMetrics.FailedRequests / $endpointMetrics.TotalRequests) * 100, 2)
            } else { 0 }
            
            Write-ColorOutput "  $($endpoint.Key):" "White"
            Write-ColorOutput "    请求数: $($endpointMetrics.TotalRequests)" "Gray"
            Write-ColorOutput "    平均响应时间: $avgResponseTime ms" "Gray"
            Write-ColorOutput "    错误率: $errorRate%" "Gray"
        }
    }
    
    # 性能评估
    Write-ColorOutput "`n🎯 性能评估:" "Cyan"
    if ($metrics.RequestsPerSecond -ge 100) {
        Write-ColorOutput "✓ 吞吐量优秀 (≥100 req/s)" "Green"
    } elseif ($metrics.RequestsPerSecond -ge 50) {
        Write-ColorOutput "⚠ 吞吐量良好 (≥50 req/s)" "Yellow"
    } else {
        Write-ColorOutput "✗ 吞吐量需要优化 (<50 req/s)" "Red"
    }
    
    if ($metrics.AverageResponseTime -le 200) {
        Write-ColorOutput "✓ 响应时间优秀 (≤200ms)" "Green"
    } elseif ($metrics.AverageResponseTime -le 500) {
        Write-ColorOutput "⚠ 响应时间良好 (≤500ms)" "Yellow"
    } else {
        Write-ColorOutput "✗ 响应时间需要优化 (>500ms)" "Red"
    }
    
    if ($metrics.ErrorRate -le 1) {
        Write-ColorOutput "✓ 错误率优秀 (≤1%)" "Green"
    } elseif ($metrics.ErrorRate -le 5) {
        Write-ColorOutput "⚠ 错误率可接受 (≤5%)" "Yellow"
    } else {
        Write-ColorOutput "✗ 错误率过高 (>5%)" "Red"
    }
}

# 主函数
function Main {
    Write-ColorOutput "🚀 林龍香大米商城后端服务性能测试" "Cyan"
    Write-ColorOutput "测试URL: $BaseUrl" "White"
    Write-ColorOutput "===========================================" "Cyan"
    
    # 检查服务是否可用
    try {
        $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 10
        Write-ColorOutput "✓ 服务可用，开始性能测试" "Green"
    }
    catch {
        Write-ColorOutput "✗ 服务不可用: $($_.Exception.Message)" "Red"
        Write-ColorOutput "请确保服务正在运行在 $BaseUrl" "Yellow"
        exit 1
    }
    
    # 执行性能测试
    Start-PerformanceTest
    
    # 显示结果
    Show-PerformanceResults
    
    Write-ColorOutput "`n🎉 性能测试完成!" "Green"
}

# 执行主函数
Main
