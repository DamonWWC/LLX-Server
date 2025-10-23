# 数据库迁移管理工具
# 提供完整的迁移管理功能

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("add", "remove", "update", "list", "reset", "script")]
    [string]$Action,
    
    [string]$MigrationName = "",
    [string]$DatabaseType = "PostgreSQL",
    [string]$ConnectionString = "",
    [switch]$Force,
    [switch]$ShowHelp
)

if ($ShowHelp) {
    Write-Host "数据库迁移管理工具" -ForegroundColor Green
    Write-Host ""
    Write-Host "使用方法:" -ForegroundColor Yellow
    Write-Host "  .\manage-migrations.ps1 -Action <操作> [-MigrationName <名称>] [-DatabaseType <类型>] [-ConnectionString <连接字符串>] [-Force]" -ForegroundColor White
    Write-Host ""
    Write-Host "操作说明:" -ForegroundColor Cyan
    Write-Host "  add      - 添加新迁移" -ForegroundColor White
    Write-Host "  remove   - 删除最后一个迁移" -ForegroundColor White
    Write-Host "  update   - 应用迁移到数据库" -ForegroundColor White
    Write-Host "  list     - 列出所有迁移" -ForegroundColor White
    Write-Host "  reset    - 重置数据库（删除所有数据）" -ForegroundColor White
    Write-Host "  script   - 生成迁移脚本" -ForegroundColor White
    Write-Host ""
    Write-Host "参数说明:" -ForegroundColor Cyan
    Write-Host "  -MigrationName   迁移名称（add 操作必需）" -ForegroundColor White
    Write-Host "  -DatabaseType    数据库类型 (PostgreSQL, SqlServer, MySql, Sqlite)" -ForegroundColor White
    Write-Host "  -ConnectionString 数据库连接字符串" -ForegroundColor White
    Write-Host "  -Force           强制执行操作" -ForegroundColor White
    Write-Host ""
    Write-Host "示例:" -ForegroundColor Cyan
    Write-Host "  .\manage-migrations.ps1 -Action add -MigrationName InitialCreate" -ForegroundColor White
    Write-Host "  .\manage-migrations.ps1 -Action update -DatabaseType PostgreSQL" -ForegroundColor White
    Write-Host "  .\manage-migrations.ps1 -Action list" -ForegroundColor White
    exit 0
}

# 设置默认连接字符串
if ([string]::IsNullOrEmpty($ConnectionString)) {
    $ConnectionString = switch ($DatabaseType) {
        "PostgreSQL" { "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password" }
        "SqlServer" { "Server=localhost;Database=llxrice;User Id=llxrice_user;Password=your_password;TrustServerCertificate=true" }
        "MySql" { "Server=localhost;Port=3306;Database=llxrice;Uid=llxrice_user;Pwd=your_password;" }
        "Sqlite" { "Data Source=llxrice.db" }
        default { "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password" }
    }
}

Write-Host "🔧 数据库迁移管理工具" -ForegroundColor Green
Write-Host "操作: $Action" -ForegroundColor Yellow
Write-Host "数据库类型: $DatabaseType" -ForegroundColor Yellow
Write-Host ""

# 设置环境变量
$env:ASPNETCORE_ENVIRONMENT = "Development"

try {
    switch ($Action) {
        "add" {
            if ([string]::IsNullOrEmpty($MigrationName)) {
                Write-Host "❌ 错误: 添加迁移时必须指定迁移名称" -ForegroundColor Red
                exit 1
            }
            
            Write-Host "📝 添加迁移: $MigrationName" -ForegroundColor Cyan
            dotnet ef migrations add $MigrationName --context AppDbContext
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ 迁移 '$MigrationName' 添加成功" -ForegroundColor Green
            } else {
                Write-Host "❌ 迁移添加失败" -ForegroundColor Red
                exit 1
            }
        }
        
        "remove" {
            Write-Host "🗑️ 删除最后一个迁移" -ForegroundColor Cyan
            dotnet ef migrations remove --context AppDbContext
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ 迁移删除成功" -ForegroundColor Green
            } else {
                Write-Host "❌ 迁移删除失败" -ForegroundColor Red
                exit 1
            }
        }
        
        "update" {
            Write-Host "🔄 应用迁移到数据库" -ForegroundColor Cyan
            dotnet ef database update --context AppDbContext
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ 数据库迁移成功" -ForegroundColor Green
            } else {
                Write-Host "❌ 数据库迁移失败" -ForegroundColor Red
                exit 1
            }
        }
        
        "list" {
            Write-Host "📋 列出所有迁移" -ForegroundColor Cyan
            dotnet ef migrations list --context AppDbContext
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ 迁移列表获取成功" -ForegroundColor Green
            } else {
                Write-Host "❌ 获取迁移列表失败" -ForegroundColor Red
                exit 1
            }
        }
        
        "reset" {
            if (-not $Force) {
                $confirmation = Read-Host "⚠️ 警告: 这将删除所有数据！是否继续？(y/N)"
                if ($confirmation -ne "y" -and $confirmation -ne "Y") {
                    Write-Host "❌ 操作已取消" -ForegroundColor Yellow
                    exit 0
                }
            }
            
            Write-Host "🔄 重置数据库" -ForegroundColor Cyan
            dotnet ef database drop --context AppDbContext --force
            dotnet ef database update --context AppDbContext
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ 数据库重置成功" -ForegroundColor Green
            } else {
                Write-Host "❌ 数据库重置失败" -ForegroundColor Red
                exit 1
            }
        }
        
        "script" {
            Write-Host "📄 生成迁移脚本" -ForegroundColor Cyan
            $outputFile = "migration_script_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
            dotnet ef migrations script --context AppDbContext --output $outputFile
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ 迁移脚本已生成: $outputFile" -ForegroundColor Green
            } else {
                Write-Host "❌ 迁移脚本生成失败" -ForegroundColor Red
                exit 1
            }
        }
    }
} catch {
    Write-Host "❌ 操作失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 操作完成!" -ForegroundColor Green
