# 林龍香大米商城后端服务数据库测试脚本
# PowerShell 版本

param(
    [string]$ConnectionString = "",
    [switch]$Verbose = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
林龍香大米商城后端服务数据库测试脚本

用法: .\test-database.ps1 [参数]

参数:
  -ConnectionString <string>  数据库连接字符串
  -Verbose                   显示详细输出
  -Help                      显示此帮助信息

示例:
  .\test-database.ps1                                    # 使用默认连接字符串
  .\test-database.ps1 -ConnectionString "Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass"  # 使用自定义连接字符串
  .\test-database.ps1 -Verbose                           # 显示详细输出

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
function Test-DatabaseOperation {
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

# 检查数据库连接
function Test-DatabaseConnection {
    return Test-DatabaseOperation -Name "数据库连接测试" -Description "检查数据库连接是否正常" -TestScript {
        try {
            # 这里应该使用实际的数据库连接测试
            # 由于没有直接的PostgreSQL PowerShell模块，我们使用.NET的方式
            Add-Type -AssemblyName System.Data
            
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            $connection.Close()
            return $true
        }
        catch {
            Write-ColorOutput "   连接失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试表结构
function Test-TableStructure {
    return Test-DatabaseOperation -Name "表结构测试" -Description "检查数据库表结构是否正确" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            
            $tables = @("Products", "Addresses", "Orders", "OrderItems", "ShippingRates")
            $allTablesExist = $true
            
            foreach ($table in $tables) {
                $command = $connection.CreateCommand()
                $command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '$table'"
                $count = $command.ExecuteScalar()
                
                if ($count -eq 0) {
                    Write-ColorOutput "   表 $table 不存在" "Red"
                    $allTablesExist = $false
                } else {
                    Write-ColorOutput "   表 $table 存在" "Green"
                }
            }
            
            $connection.Close()
            return $allTablesExist
        }
        catch {
            Write-ColorOutput "   表结构检查失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试数据完整性
function Test-DataIntegrity {
    return Test-DatabaseOperation -Name "数据完整性测试" -Description "检查数据完整性和约束" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            
            # 测试外键约束
            $command = $connection.CreateCommand()
            $command.CommandText = @"
                SELECT COUNT(*) 
                FROM Orders o 
                LEFT JOIN Addresses a ON o.AddressId = a.Id 
                WHERE a.Id IS NULL AND o.AddressId IS NOT NULL
"@
            $orphanedOrders = $command.ExecuteScalar()
            
            if ($orphanedOrders -gt 0) {
                Write-ColorOutput "   发现 $orphanedOrders 个孤立订单" "Red"
                return $false
            }
            
            # 测试订单项约束
            $command.CommandText = @"
                SELECT COUNT(*) 
                FROM OrderItems oi 
                LEFT JOIN Orders o ON oi.OrderId = o.Id 
                WHERE o.Id IS NULL
"@
            $orphanedOrderItems = $command.ExecuteScalar()
            
            if ($orphanedOrderItems -gt 0) {
                Write-ColorOutput "   发现 $orphanedOrderItems 个孤立订单项" "Red"
                return $false
            }
            
            $connection.Close()
            Write-ColorOutput "   数据完整性检查通过" "Green"
            return $true
        }
        catch {
            Write-ColorOutput "   数据完整性检查失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试索引性能
function Test-IndexPerformance {
    return Test-DatabaseOperation -Name "索引性能测试" -Description "检查索引是否存在并测试查询性能" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            
            # 检查关键索引
            $indexes = @(
                "IX_Products_Name",
                "IX_Addresses_Phone",
                "IX_Orders_Status",
                "IX_Orders_AddressId",
                "IX_OrderItems_OrderId"
            )
            
            $allIndexesExist = $true
            foreach ($index in $indexes) {
                $command = $connection.CreateCommand()
                $command.CommandText = "SELECT COUNT(*) FROM pg_indexes WHERE indexname = '$index'"
                $count = $command.ExecuteScalar()
                
                if ($count -eq 0) {
                    Write-ColorOutput "   索引 $index 不存在" "Red"
                    $allIndexesExist = $false
                } else {
                    Write-ColorOutput "   索引 $index 存在" "Green"
                }
            }
            
            $connection.Close()
            return $allIndexesExist
        }
        catch {
            Write-ColorOutput "   索引检查失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试连接池
function Test-ConnectionPool {
    return Test-DatabaseOperation -Name "连接池测试" -Description "测试数据库连接池性能" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            
            $connections = @()
            $maxConnections = 10
            
            # 创建多个连接
            for ($i = 0; $i -lt $maxConnections; $i++) {
                $conn = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
                $conn.Open()
                $connections += $conn
            }
            
            Write-ColorOutput "   成功创建 $maxConnections 个连接" "Green"
            
            # 关闭所有连接
            foreach ($conn in $connections) {
                $conn.Close()
            }
            
            Write-ColorOutput "   连接池测试通过" "Green"
            return $true
        }
        catch {
            Write-ColorOutput "   连接池测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试事务
function Test-Transactions {
    return Test-DatabaseOperation -Name "事务测试" -Description "测试数据库事务功能" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            
            $transaction = $connection.BeginTransaction()
            
            try {
                # 执行一些操作
                $command = $connection.CreateCommand()
                $command.Transaction = $transaction
                $command.CommandText = "SELECT COUNT(*) FROM Products"
                $count = $command.ExecuteScalar()
                
                Write-ColorOutput "   事务中查询到 $count 个商品" "Green"
                
                # 提交事务
                $transaction.Commit()
                Write-ColorOutput "   事务提交成功" "Green"
                
                $connection.Close()
                return $true
            }
            catch {
                $transaction.Rollback()
                Write-ColorOutput "   事务回滚" "Yellow"
                throw
            }
        }
        catch {
            Write-ColorOutput "   事务测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 测试数据迁移
function Test-Migrations {
    return Test-DatabaseOperation -Name "数据迁移测试" -Description "检查数据库迁移状态" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            
            # 检查迁移表
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory'"
            $migrationTableExists = $command.ExecuteScalar() -gt 0
            
            if ($migrationTableExists) {
                $command.CommandText = "SELECT COUNT(*) FROM __EFMigrationsHistory"
                $migrationCount = $command.ExecuteScalar()
                Write-ColorOutput "   发现 $migrationCount 个迁移记录" "Green"
            } else {
                Write-ColorOutput "   迁移表不存在" "Yellow"
            }
            
            $connection.Close()
            return $true
        }
        catch {
            Write-ColorOutput "   迁移检查失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 性能基准测试
function Test-PerformanceBenchmark {
    return Test-DatabaseOperation -Name "性能基准测试" -Description "测试数据库查询性能" -TestScript {
        try {
            $connection = New-Object System.Data.Odbc.OdbcConnection($ConnectionString)
            $connection.Open()
            
            $queries = @(
                "SELECT COUNT(*) FROM Products",
                "SELECT COUNT(*) FROM Addresses",
                "SELECT COUNT(*) FROM Orders",
                "SELECT COUNT(*) FROM OrderItems",
                "SELECT COUNT(*) FROM ShippingRates"
            )
            
            foreach ($query in $queries) {
                $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                
                $command = $connection.CreateCommand()
                $command.CommandText = $query
                $result = $command.ExecuteScalar()
                
                $stopwatch.Stop()
                $elapsed = $stopwatch.ElapsedMilliseconds
                
                Write-ColorOutput "   查询 '$query' 耗时: $elapsed ms, 结果: $result" "Green"
            }
            
            $connection.Close()
            return $true
        }
        catch {
            Write-ColorOutput "   性能测试失败: $($_.Exception.Message)" "Red"
            return $false
        }
    }
}

# 显示测试结果
function Show-TestResults {
    Write-ColorOutput "`n📊 数据库测试结果汇总" "Cyan"
    Write-ColorOutput "===========================================" "Cyan"
    Write-ColorOutput "总测试数: $($TestResults.Total)" "White"
    Write-ColorOutput "通过: $($TestResults.Passed)" "Green"
    Write-ColorOutput "失败: $($TestResults.Failed)" "Red"
    
    $passRate = if ($TestResults.Total -gt 0) { [math]::Round(($TestResults.Passed / $TestResults.Total) * 100, 2) } else { 0 }
    Write-ColorOutput "通过率: $passRate%" "White"
    
    if ($TestResults.Failed -eq 0) {
        Write-ColorOutput "`n🎉 所有数据库测试通过!" "Green"
    } else {
        Write-ColorOutput "`n⚠️  有 $($TestResults.Failed) 个测试失败" "Yellow"
    }
}

# 主函数
function Main {
    Write-ColorOutput "🚀 林龍香大米商城后端服务数据库测试" "Cyan"
    Write-ColorOutput "===========================================" "Cyan"
    
    # 如果没有提供连接字符串，尝试从环境变量获取
    if (-not $ConnectionString) {
        $ConnectionString = $env:ConnectionStrings__DefaultConnection
        if (-not $ConnectionString) {
            Write-ColorOutput "✗ 未提供数据库连接字符串" "Red"
            Write-ColorOutput "请使用 -ConnectionString 参数或设置环境变量 ConnectionStrings__DefaultConnection" "Yellow"
            exit 1
        }
    }
    
    Write-ColorOutput "使用连接字符串: $($ConnectionString.Substring(0, [Math]::Min(50, $ConnectionString.Length)))..." "White"
    
    # 执行测试
    Test-DatabaseConnection
    Test-TableStructure
    Test-DataIntegrity
    Test-IndexPerformance
    Test-ConnectionPool
    Test-Transactions
    Test-Migrations
    Test-PerformanceBenchmark
    
    # 显示结果
    Show-TestResults
}

# 执行主函数
Main
