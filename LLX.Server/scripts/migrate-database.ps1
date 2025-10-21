# 数据库迁移脚本
# 使用方法: .\migrate-database.ps1 -DatabaseType PostgreSQL -ConnectionString "your_connection_string"

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("PostgreSQL", "SqlServer", "MySql", "Sqlite")]
    [string]$DatabaseType,
    
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [string]$MigrationName = "InitialCreate"
)

Write-Host "正在为 $DatabaseType 数据库创建迁移..." -ForegroundColor Green

# 设置环境变量
$env:ASPNETCORE_ENVIRONMENT = "Development"

# 创建迁移
Write-Host "创建迁移: $MigrationName" -ForegroundColor Yellow
dotnet ef migrations add $MigrationName --context AppDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "迁移创建成功!" -ForegroundColor Green
    
    # 应用迁移
    Write-Host "应用迁移到数据库..." -ForegroundColor Yellow
    dotnet ef database update --context AppDbContext
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "数据库迁移完成!" -ForegroundColor Green
    } else {
        Write-Host "数据库迁移失败!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "迁移创建失败!" -ForegroundColor Red
    exit 1
}
