# 林龍香大米商城 - 配置设置脚本
# Windows PowerShell 版本

param(
    [string]$Provider = "PostgreSQL",
    [string]$DbHost = "host.docker.internal",
    [int]$DbPort = 5432,
    [string]$DbName = "llxrice",
    [string]$DbUser = "llxrice_user",
    [string]$DbPassword = "",
    [string]$RedisHost = "host.docker.internal",
    [int]$RedisPort = 6379,
    [string]$RedisPassword = "",
    [switch]$Help
)

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    $colorMap = @{
        "Red" = "Red"
        "Green" = "Green"
        "Yellow" = "Yellow"
        "Blue" = "Blue"
        "Cyan" = "Cyan"
        "White" = "White"
        "Gray" = "DarkGray"
    }
    
    Write-Host $Message -ForegroundColor $colorMap[$Color]
}

# 显示帮助信息
function Show-Help {
    Write-ColorOutput "林龍香大米商城 - 配置设置脚本" "Cyan"
    Write-ColorOutput ""
    Write-ColorOutput "用法: .\setup-config.ps1 [参数]"
    Write-ColorOutput ""
    Write-ColorOutput "参数:"
    Write-ColorOutput "  -Provider <provider>       数据库提供商 (PostgreSQL/SqlServer/MySql/Sqlite) [默认: PostgreSQL]"
    Write-ColorOutput "  -DbHost <host>            数据库主机地址 [默认: host.docker.internal]"
    Write-ColorOutput "  -DbPort <port>            数据库端口 [默认: 5432]"
    Write-ColorOutput "  -DbName <name>            数据库名称 [默认: llxrice]"
    Write-ColorOutput "  -DbUser <user>            数据库用户名 [默认: llxrice_user]"
    Write-ColorOutput "  -DbPassword <password>    数据库密码 [必需]"
    Write-ColorOutput "  -RedisHost <host>         Redis主机地址 [默认: host.docker.internal]"
    Write-ColorOutput "  -RedisPort <port>         Redis端口 [默认: 6379]"
    Write-ColorOutput "  -RedisPassword <password> Redis密码 [必需]"
    Write-ColorOutput "  -Help                     显示此帮助信息"
    Write-ColorOutput ""
    Write-ColorOutput "示例:"
    Write-ColorOutput "  .\setup-config.ps1 -DbPassword 'your_db_password' -RedisPassword 'your_redis_password'"
    Write-ColorOutput "  .\setup-config.ps1 -Provider SqlServer -DbHost '192.168.1.100' -DbPassword 'your_password' -RedisPassword 'your_redis_password'"
}

# 验证参数
function Test-Parameters {
    if ([string]::IsNullOrEmpty($DbPassword)) {
        Write-ColorOutput "✗ 数据库密码不能为空" "Red"
        Write-ColorOutput "请使用 -DbPassword 参数指定数据库密码" "Yellow"
        return $false
    }
    
    if ([string]::IsNullOrEmpty($RedisPassword)) {
        Write-ColorOutput "✗ Redis密码不能为空" "Red"
        Write-ColorOutput "请使用 -RedisPassword 参数指定Redis密码" "Yellow"
        return $false
    }
    
    $validProviders = @("PostgreSQL", "SqlServer", "MySql", "Sqlite")
    if ($validProviders -notcontains $Provider) {
        Write-ColorOutput "✗ 不支持的数据库提供商: $Provider" "Red"
        Write-ColorOutput "支持的提供商: $($validProviders -join ', ')" "Yellow"
        return $false
    }
    
    return $true
}

# 创建.env文件
function New-EnvFile {
    Write-ColorOutput "创建.env文件..." "Cyan"
    
    # 构建数据库连接字符串
    $dbConnectionString = "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30"
    
    # 构建Redis连接字符串
    $redisConnectionString = "$RedisHost`:$RedisPort,password=$RedisPassword,ssl=false,abortConnect=false"
    
    # 创建.env文件内容
    $envContent = @"
# ============================================
# 林龍香大米商城 - API服务独立部署环境变量配置
# ============================================
# 说明：此配置文件用于仅部署API服务的场景
# 前提：PostgreSQL和Redis已经在其他地方运行
# 使用：复制此文件为 .env 并修改相应的值

# ============================================
# 应用基础配置
# ============================================

# 应用环境 (Development/Staging/Production)
ASPNETCORE_ENVIRONMENT=Production

# API服务端口（宿主机端口）
API_PORT=8080

# ============================================
# 数据库连接配置（$Provider）
# ============================================

# 数据库连接字符串
DB_CONNECTION_STRING=$dbConnectionString

# 数据库提供商（PostgreSQL/SqlServer/MySql/Sqlite）
DB_PROVIDER=$Provider

# 数据库连接池配置
DB_MIN_POOL_SIZE=5
DB_MAX_POOL_SIZE=100
DB_CONNECTION_LIFETIME=300
DB_COMMAND_TIMEOUT=30
DB_ENABLE_RETRY=true
DB_MAX_RETRY_COUNT=3

# ============================================
# Redis缓存连接配置
# ============================================

# Redis连接字符串
REDIS_CONNECTION_STRING=$redisConnectionString

# ============================================
# 性能配置
# ============================================

# 慢请求阈值（毫秒）
PERFORMANCE_SLOW_REQUEST_THRESHOLD=1000

# ============================================
# 日志配置
# ============================================

# 日志级别 (Trace/Debug/Information/Warning/Error/Critical)
LOG_LEVEL=Information

# ============================================
# 容器资源配置
# ============================================

# 内存限制（单位：M表示MB，G表示GB）
CONTAINER_MEMORY_LIMIT=512M
CONTAINER_MEMORY_RESERVATION=256M

# CPU限制（0.5表示0.5个CPU核心）
CONTAINER_CPU_LIMIT=0.5
CONTAINER_CPU_RESERVATION=0.25

# ============================================
# 高级配置（可选）
# ============================================

# 是否启用详细错误信息（生产环境建议设为false）
ENABLE_DETAILED_ERRORS=false

# 是否启用敏感数据日志（生产环境建议设为false）
ENABLE_SENSITIVE_DATA_LOGGING=false

# 配置加密密钥（32位随机字符串，如果使用配置加密功能）
# CONFIG_ENCRYPTION_KEY=your_32_character_encryption_key_here
"@
    
    # 写入.env文件
    $envContent | Out-File -FilePath ".env" -Encoding UTF8
    
    Write-ColorOutput "✓ .env 文件创建成功" "Green"
}

# 设置系统环境变量
function Set-SystemEnvironmentVariables {
    Write-ColorOutput "设置系统环境变量..." "Cyan"
    
    try {
        # 设置数据库Provider
        [Environment]::SetEnvironmentVariable("DB_PROVIDER", $Provider, "User")
        Write-ColorOutput "✓ DB_PROVIDER 环境变量设置成功" "Green"
        
        # 设置数据库连接字符串
        $dbConnectionString = "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30"
        [Environment]::SetEnvironmentVariable("DB_CONNECTION_STRING", $dbConnectionString, "User")
        Write-ColorOutput "✓ DB_CONNECTION_STRING 环境变量设置成功" "Green"
        
        # 设置Redis连接字符串
        $redisConnectionString = "$RedisHost`:$RedisPort,password=$RedisPassword,ssl=false,abortConnect=false"
        [Environment]::SetEnvironmentVariable("REDIS_CONNECTION_STRING", $redisConnectionString, "User")
        Write-ColorOutput "✓ REDIS_CONNECTION_STRING 环境变量设置成功" "Green"
        
    } catch {
        Write-ColorOutput "✗ 设置环境变量失败: $($_.Exception.Message)" "Red"
        return $false
    }
    
    return $true
}

# 验证配置
function Test-Configuration {
    Write-ColorOutput "`n验证配置..." "Cyan"
    
    # 验证.env文件
    if (Test-Path ".env") {
        Write-ColorOutput "✓ .env 文件存在" "Green"
        
        $envContent = Get-Content ".env"
        
        # 检查关键配置
        $dbProviderLine = $envContent | Where-Object { $_ -match "^DB_PROVIDER=" }
        if ($dbProviderLine) {
            $dbProvider = $dbProviderLine.Split('=')[1]
            Write-ColorOutput "✓ 数据库提供商: $dbProvider" "Green"
        }
        
        $dbConnectionLine = $envContent | Where-Object { $_ -match "^DB_CONNECTION_STRING=" }
        if ($dbConnectionLine) {
            Write-ColorOutput "✓ 数据库连接字符串: 已设置" "Green"
        }
        
        $redisConnectionLine = $envContent | Where-Object { $_ -match "^REDIS_CONNECTION_STRING=" }
        if ($redisConnectionLine) {
            Write-ColorOutput "✓ Redis连接字符串: 已设置" "Green"
        }
    } else {
        Write-ColorOutput "✗ .env 文件不存在" "Red"
    }
    
    # 验证环境变量
    $envDbProvider = [Environment]::GetEnvironmentVariable("DB_PROVIDER", "User")
    if ($envDbProvider) {
        Write-ColorOutput "✓ 系统环境变量 DB_PROVIDER: $envDbProvider" "Green"
    } else {
        Write-ColorOutput "⚠ 系统环境变量 DB_PROVIDER 未设置" "Yellow"
    }
}

# 显示配置信息
function Show-ConfigurationInfo {
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "📋 配置信息" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    
    Write-ColorOutput "数据库配置:" "White"
    Write-ColorOutput "  提供商: $Provider" "Gray"
    Write-ColorOutput "  主机: $DbHost" "Gray"
    Write-ColorOutput "  端口: $DbPort" "Gray"
    Write-ColorOutput "  数据库: $DbName" "Gray"
    Write-ColorOutput "  用户名: $DbUser" "Gray"
    
    Write-ColorOutput "`nRedis配置:" "White"
    Write-ColorOutput "  主机: $RedisHost" "Gray"
    Write-ColorOutput "  端口: $RedisPort" "Gray"
    
    Write-ColorOutput "`n下一步操作:" "White"
    Write-ColorOutput "  1. 检查 .env 文件配置是否正确" "Gray"
    Write-ColorOutput "  2. 运行配置测试: .\test-config.ps1" "Gray"
    Write-ColorOutput "  3. 部署服务: .\deploy-api-only.ps1" "Gray"
}

# 主函数
function Main {
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "🔧 林龍香大米商城 - 配置设置" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput ""
    
    # 显示帮助信息
    if ($Help) {
        Show-Help
        return
    }
    
    # 检查是否在项目根目录
    if (-not (Test-Path "LLX.Server.sln")) {
        Write-ColorOutput "✗ 不在项目根目录" "Red"
        Write-ColorOutput "请切换到项目根目录运行此脚本" "Yellow"
        return
    }
    
    Write-ColorOutput "✓ 在项目根目录" "Green"
    
    # 验证参数
    if (-not (Test-Parameters)) {
        return
    }
    
    # 创建.env文件
    New-EnvFile
    
    # 设置系统环境变量
    if (Set-SystemEnvironmentVariables) {
        Write-ColorOutput "✓ 系统环境变量设置成功" "Green"
    }
    
    # 验证配置
    Test-Configuration
    
    # 显示配置信息
    Show-ConfigurationInfo
    
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
    Write-ColorOutput "🎉 配置设置完成！" "Green"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
}

# 执行主函数
Main
