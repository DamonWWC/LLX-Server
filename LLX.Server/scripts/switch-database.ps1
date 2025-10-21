# 数据库切换脚本
# 使用方法: .\switch-database.ps1 -DatabaseType PostgreSQL -ConnectionString "your_connection_string"

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("PostgreSQL", "SqlServer", "MySql", "Sqlite")]
    [string]$DatabaseType,
    
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [string]$ConfigFile = "appsettings.json"
)

Write-Host "正在切换到 $DatabaseType 数据库..." -ForegroundColor Green

# 读取配置文件
$configPath = Join-Path $PSScriptRoot "..\$ConfigFile"
$config = Get-Content $configPath | ConvertFrom-Json

# 更新连接字符串
$config.ConnectionStrings.DefaultConnection = $ConnectionString

# 更新数据库提供程序
if (-not $config.Database) {
    $config.Database = @{}
}
$config.Database.Provider = $DatabaseType

# 保存配置文件
$config | ConvertTo-Json -Depth 10 | Set-Content $configPath

Write-Host "配置文件已更新: $ConfigFile" -ForegroundColor Yellow
Write-Host "数据库类型: $DatabaseType" -ForegroundColor Cyan
Write-Host "连接字符串: $ConnectionString" -ForegroundColor Cyan

# 提示运行迁移
Write-Host "`n请运行以下命令来应用数据库迁移:" -ForegroundColor Yellow
Write-Host "dotnet ef migrations add InitialCreate" -ForegroundColor White
Write-Host "dotnet ef database update" -ForegroundColor White

Write-Host "`n数据库切换完成!" -ForegroundColor Green
