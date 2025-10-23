# 数据库连接测试脚本
# 使用方法: .\test-connection.ps1 -DatabaseType PostgreSQL -ConnectionString "your_connection_string"

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("PostgreSQL", "SqlServer", "MySql", "Sqlite")]
    [string]$DatabaseType,
    
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString
)

Write-Host "正在测试 $DatabaseType 数据库连接..." -ForegroundColor Green

# 设置环境变量
$env:ASPNETCORE_ENVIRONMENT = "Development"

# 构建测试命令
$testCommand = @"
using Microsoft.EntityFrameworkCore;
using LLX.Server.Data;
using LLX.Server.Models.Entities;

var provider = DatabaseProvider.$DatabaseType;
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseDatabase(`"$ConnectionString`", provider)
    .Options;

try {
    using var context = new AppDbContext(options);
    var canConnect = await context.Database.CanConnectAsync();
    if (canConnect) {
        Console.WriteLine("✅ 数据库连接成功!");
        
        // 测试基本查询
        var productCount = await context.Products.CountAsync();
        Console.WriteLine($"📊 商品数量: {productCount}");
        
        // 测试地址查询
        var addressCount = await context.Addresses.CountAsync();
        Console.WriteLine($"📍 地址数量: {addressCount}");
        
        // 测试订单查询
        var orderCount = await context.Orders.CountAsync();
        Console.WriteLine($"📦 订单数量: {orderCount}");
        
        Console.WriteLine("🎉 所有测试通过!");
    } else {
        Console.WriteLine("❌ 数据库连接失败!");
        Environment.Exit(1);
    }
} catch (Exception ex) {
    Console.WriteLine($"❌ 连接测试失败: {ex.Message}");
    Environment.Exit(1);
}
"@

# 创建临时测试文件
$tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
$testCommand | Out-File -FilePath $tempFile -Encoding UTF8

try {
    # 编译并运行测试
    dotnet run --project . -- $tempFile
} finally {
    # 清理临时文件
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
}
