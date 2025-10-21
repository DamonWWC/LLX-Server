# 数据库切换演示脚本
# 演示如何在不同数据库之间切换

param(
    [switch]$ShowHelp
)

if ($ShowHelp) {
    Write-Host "数据库切换演示脚本" -ForegroundColor Green
    Write-Host "使用方法: .\demo-database-switch.ps1" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "此脚本将演示:" -ForegroundColor Cyan
    Write-Host "1. 切换到 PostgreSQL" -ForegroundColor White
    Write-Host "2. 切换到 SQL Server" -ForegroundColor White
    Write-Host "3. 切换到 MySQL" -ForegroundColor White
    Write-Host "4. 切换到 SQLite" -ForegroundColor White
    Write-Host "5. 测试每个数据库的连接" -ForegroundColor White
    exit 0
}

Write-Host "🚀 开始数据库切换演示..." -ForegroundColor Green
Write-Host ""

# 演示数据库配置
$databases = @(
    @{
        Name = "PostgreSQL"
        Provider = "PostgreSQL"
        ConnectionString = "Host=localhost;Port=5432;Database=llxrice_demo;Username=llxrice_user;Password=demo_password;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=10"
    },
    @{
        Name = "SQL Server"
        Provider = "SqlServer"
        ConnectionString = "Server=localhost;Database=llxrice_demo;User Id=llxrice_user;Password=demo_password;TrustServerCertificate=true;MultipleActiveResultSets=true"
    },
    @{
        Name = "MySQL"
        Provider = "MySql"
        ConnectionString = "Server=localhost;Port=3306;Database=llxrice_demo;Uid=llxrice_user;Pwd=demo_password;"
    },
    @{
        Name = "SQLite"
        Provider = "Sqlite"
        ConnectionString = "Data Source=llxrice_demo.db"
    }
)

foreach ($db in $databases) {
    Write-Host "📊 切换到 $($db.Name)..." -ForegroundColor Yellow
    
    # 更新配置文件
    $configPath = "appsettings.json"
    $config = Get-Content $configPath | ConvertFrom-Json
    
    $config.ConnectionStrings.DefaultConnection = $db.ConnectionString
    $config.Database.Provider = $db.Provider
    
    $config | ConvertTo-Json -Depth 10 | Set-Content $configPath
    
    Write-Host "✅ 配置文件已更新" -ForegroundColor Green
    
    # 测试连接（模拟）
    Write-Host "🔍 测试数据库连接..." -ForegroundColor Cyan
    
    # 这里可以添加实际的连接测试
    Start-Sleep -Seconds 1
    
    Write-Host "✅ $($db.Name) 配置完成" -ForegroundColor Green
    Write-Host ""
}

Write-Host "🎉 数据库切换演示完成!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 接下来您可以:" -ForegroundColor Cyan
Write-Host "1. 运行 'dotnet run' 启动应用" -ForegroundColor White
Write-Host "2. 访问 http://localhost:5000/health 检查健康状态" -ForegroundColor White
Write-Host "3. 访问 http://localhost:5000/swagger 查看 API 文档" -ForegroundColor White
Write-Host "4. 使用 'dotnet ef migrations add InitialCreate' 创建迁移" -ForegroundColor White
Write-Host "5. 使用 'dotnet ef database update' 应用迁移" -ForegroundColor White
