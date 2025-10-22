# 林龍香大米商城后端服务 API 测试脚本
# PowerShell 版本

param(
    [string]$BaseUrl = "http://localhost:8080",
    [switch]$Verbose = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
林龍香大米商城后端服务 API 测试脚本

用法: .\test-api.ps1 [参数]

参数:
  -BaseUrl <string>   API 基础 URL [默认: http://localhost:8080]
  -Verbose           显示详细输出
  -Help              显示此帮助信息

示例:
  .\test-api.ps1                           # 使用默认URL测试
  .\test-api.ps1 -BaseUrl http://localhost:5000  # 使用自定义URL测试
  .\test-api.ps1 -Verbose                  # 显示详细输出

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
    Skipped = 0
}

# 测试函数
function Test-ApiEndpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [hashtable]$Headers = @{},
        [int]$ExpectedStatusCode = 200,
        [string]$Description = ""
    )
    
    $TestResults.Total++
    
    try {
        Write-ColorOutput "🧪 测试: $Name" "Cyan"
        if ($Description) {
            Write-ColorOutput "   描述: $Description" "Gray"
        }
        
        $requestParams = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            TimeoutSec = 30
        }
        
        if ($Body) {
            $requestParams.Body = $Body
            $requestParams.ContentType = "application/json"
        }
        
        $response = Invoke-RestMethod @requestParams -ErrorAction Stop
        
        Write-ColorOutput "   ✓ 状态码: $($response.StatusCode)" "Green"
        Write-ColorOutput "   ✓ 响应时间: $($response.ResponseTime)ms" "Green"
        
        if ($Verbose) {
            Write-ColorOutput "   响应内容: $($response | ConvertTo-Json -Depth 2)" "Gray"
        }
        
        $TestResults.Passed++
        return $true
    }
    catch {
        Write-ColorOutput "   ✗ 测试失败: $($_.Exception.Message)" "Red"
        $TestResults.Failed++
        return $false
    }
}

# 测试健康检查
function Test-HealthCheck {
    Write-ColorOutput "`n🏥 健康检查测试" "Yellow"
    
    Test-ApiEndpoint -Name "健康检查" -Method "GET" -Url "$BaseUrl/health" -Description "检查服务健康状态"
}

# 测试商品API
function Test-ProductApi {
    Write-ColorOutput "`n📦 商品API测试" "Yellow"
    
    # 获取所有商品
    Test-ApiEndpoint -Name "获取所有商品" -Method "GET" -Url "$BaseUrl/api/products" -Description "获取商品列表"
    
    # 分页获取商品
    Test-ApiEndpoint -Name "分页获取商品" -Method "GET" -Url "$BaseUrl/api/products/paged?pageNumber=1&pageSize=10" -Description "分页获取商品列表"
    
    # 搜索商品
    Test-ApiEndpoint -Name "搜索商品" -Method "GET" -Url "$BaseUrl/api/products/search?name=大米" -Description "搜索商品"
    
    # 创建商品
    $createProductBody = @{
        name = "测试商品"
        price = 29.99
        unit = "袋"
        weight = 5.0
        image = "https://example.com/image.jpg"
        quantity = 100
    } | ConvertTo-Json
    
    Test-ApiEndpoint -Name "创建商品" -Method "POST" -Url "$BaseUrl/api/products" -Body $createProductBody -Description "创建新商品"
    
    # 获取单个商品（假设ID为1）
    Test-ApiEndpoint -Name "获取单个商品" -Method "GET" -Url "$BaseUrl/api/products/1" -Description "根据ID获取商品"
}

# 测试地址API
function Test-AddressApi {
    Write-ColorOutput "`n📍 地址API测试" "Yellow"
    
    # 获取所有地址
    Test-ApiEndpoint -Name "获取所有地址" -Method "GET" -Url "$BaseUrl/api/addresses" -Description "获取地址列表"
    
    # 获取默认地址
    Test-ApiEndpoint -Name "获取默认地址" -Method "GET" -Url "$BaseUrl/api/addresses/default" -Description "获取默认地址"
    
    # 创建地址
    $createAddressBody = @{
        name = "张三"
        phone = "13800138000"
        province = "广东省"
        city = "深圳市"
        district = "南山区"
        detail = "科技园南区"
        isDefault = $true
    } | ConvertTo-Json
    
    Test-ApiEndpoint -Name "创建地址" -Method "POST" -Url "$BaseUrl/api/addresses" -Body $createAddressBody -Description "创建新地址"
    
    # 智能解析地址
    $parseAddressBody = @{
        fullAddress = "张三 13800138000 广东省深圳市南山区科技园南区"
    } | ConvertTo-Json
    
    Test-ApiEndpoint -Name "智能解析地址" -Method "POST" -Url "$BaseUrl/api/addresses/parse" -Body $parseAddressBody -Description "智能解析地址信息"
}

# 测试订单API
function Test-OrderApi {
    Write-ColorOutput "`n📋 订单API测试" "Yellow"
    
    # 获取所有订单
    Test-ApiEndpoint -Name "获取所有订单" -Method "GET" -Url "$BaseUrl/api/orders" -Description "获取订单列表"
    
    # 分页获取订单
    Test-ApiEndpoint -Name "分页获取订单" -Method "GET" -Url "$BaseUrl/api/orders?pageNumber=1&pageSize=10" -Description "分页获取订单列表"
    
    # 创建订单
    $createOrderBody = @{
        addressId = 1
        orderItems = @(
            @{
                productId = 1
                quantity = 2
                price = 29.99
            }
        )
        shippingFee = 10.00
        totalAmount = 69.98
    } | ConvertTo-Json
    
    Test-ApiEndpoint -Name "创建订单" -Method "POST" -Url "$BaseUrl/api/orders" -Body $createOrderBody -Description "创建新订单"
    
    # 计算订单金额
    $calculateOrderBody = @{
        addressId = 1
        orderItems = @(
            @{
                productId = 1
                quantity = 2
            }
        )
    } | ConvertTo-Json
    
    Test-ApiEndpoint -Name "计算订单金额" -Method "POST" -Url "$BaseUrl/api/orders/calculate" -Body $calculateOrderBody -Description "计算订单总金额"
}

# 测试运费API
function Test-ShippingApi {
    Write-ColorOutput "`n🚚 运费API测试" "Yellow"
    
    # 获取所有运费配置
    Test-ApiEndpoint -Name "获取运费配置" -Method "GET" -Url "$BaseUrl/api/shipping/rates" -Description "获取运费配置列表"
    
    # 根据省份获取运费
    Test-ApiEndpoint -Name "根据省份获取运费" -Method "GET" -Url "$BaseUrl/api/shipping/rates/province/广东省" -Description "根据省份获取运费配置"
    
    # 计算运费
    $calculateShippingBody = @{
        province = "广东省"
        weight = 5.0
    } | ConvertTo-Json
    
    Test-ApiEndpoint -Name "计算运费" -Method "POST" -Url "$BaseUrl/api/shipping/calculate" -Body $calculateShippingBody -Description "计算运费"
}

# 测试Swagger文档
function Test-SwaggerApi {
    Write-ColorOutput "`n📚 Swagger文档测试" "Yellow"
    
    Test-ApiEndpoint -Name "Swagger JSON" -Method "GET" -Url "$BaseUrl/swagger/v1/swagger.json" -Description "获取Swagger API文档"
}

# 性能测试
function Test-Performance {
    Write-ColorOutput "`n⚡ 性能测试" "Yellow"
    
    $endpoints = @(
        "$BaseUrl/health",
        "$BaseUrl/api/products",
        "$BaseUrl/api/addresses",
        "$BaseUrl/api/orders",
        "$BaseUrl/api/shipping/rates"
    )
    
    foreach ($endpoint in $endpoints) {
        Write-ColorOutput "🧪 性能测试: $endpoint" "Cyan"
        
        $times = @()
        for ($i = 1; $i -le 10; $i++) {
            try {
                $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                $response = Invoke-RestMethod -Uri $endpoint -Method GET -TimeoutSec 10
                $stopwatch.Stop()
                $times += $stopwatch.ElapsedMilliseconds
            }
            catch {
                Write-ColorOutput "   ✗ 请求失败: $($_.Exception.Message)" "Red"
            }
        }
        
        if ($times.Count -gt 0) {
            $avgTime = ($times | Measure-Object -Average).Average
            $minTime = ($times | Measure-Object -Minimum).Minimum
            $maxTime = ($times | Measure-Object -Maximum).Maximum
            
            Write-ColorOutput "   平均响应时间: $([math]::Round($avgTime, 2))ms" "Green"
            Write-ColorOutput "   最小响应时间: $minTime ms" "Green"
            Write-ColorOutput "   最大响应时间: $maxTime ms" "Green"
        }
    }
}

# 显示测试结果
function Show-TestResults {
    Write-ColorOutput "`n📊 测试结果汇总" "Cyan"
    Write-ColorOutput "===========================================" "Cyan"
    Write-ColorOutput "总测试数: $($TestResults.Total)" "White"
    Write-ColorOutput "通过: $($TestResults.Passed)" "Green"
    Write-ColorOutput "失败: $($TestResults.Failed)" "Red"
    Write-ColorOutput "跳过: $($TestResults.Skipped)" "Yellow"
    
    $passRate = if ($TestResults.Total -gt 0) { [math]::Round(($TestResults.Passed / $TestResults.Total) * 100, 2) } else { 0 }
    Write-ColorOutput "通过率: $passRate%" "White"
    
    if ($TestResults.Failed -eq 0) {
        Write-ColorOutput "`n🎉 所有测试通过!" "Green"
    } else {
        Write-ColorOutput "`n⚠️  有 $($TestResults.Failed) 个测试失败" "Yellow"
    }
}

# 主函数
function Main {
    Write-ColorOutput "🚀 林龍香大米商城后端服务 API 测试" "Cyan"
    Write-ColorOutput "测试URL: $BaseUrl" "White"
    Write-ColorOutput "===========================================" "Cyan"
    
    # 检查服务是否可用
    try {
        $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 10
        Write-ColorOutput "✓ 服务可用" "Green"
    }
    catch {
        Write-ColorOutput "✗ 服务不可用: $($_.Exception.Message)" "Red"
        Write-ColorOutput "请确保服务正在运行在 $BaseUrl" "Yellow"
        exit 1
    }
    
    # 执行测试
    Test-HealthCheck
    Test-ProductApi
    Test-AddressApi
    Test-OrderApi
    Test-ShippingApi
    Test-SwaggerApi
    Test-Performance
    
    # 显示结果
    Show-TestResults
}

# 执行主函数
Main
