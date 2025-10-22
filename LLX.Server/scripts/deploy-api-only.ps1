# 林龍香大米商城 - API服务独立部署脚本
# PowerShell 版本
# 说明：此脚本仅部署API服务，不部署数据库和Redis

param(
    [string]$Environment = "production",
    [switch]$Build = $false,
    [switch]$NoBuild = $false,
    [switch]$Force = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
林龍香大米商城 - API服务独立部署脚本

用法: .\deploy-api-only.ps1 [参数]

参数:
  -Environment <string>  部署环境 (development/production) [默认: production]
  -Build                 强制重新构建Docker镜像
  -NoBuild               使用已有的构建文件，不重新编译.NET项目
  -Force                 强制重新部署（停止并删除现有容器）
  -Help                  显示此帮助信息

示例:
  .\deploy-api-only.ps1                      # 标准部署
  .\deploy-api-only.ps1 -Build               # 重新构建镜像并部署
  .\deploy-api-only.ps1 -NoBuild             # 使用已构建的文件部署
  .\deploy-api-only.ps1 -Force               # 强制重新部署

前提条件:
  1. PostgreSQL数据库已运行并可访问
  2. Redis缓存已运行并可访问
  3. .env文件已正确配置数据库和Redis连接信息

"@
    exit 0
}

# 设置错误处理
$ErrorActionPreference = "Stop"

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 检查Docker是否安装
function Test-Docker {
    try {
        docker --version | Out-Null
        Write-ColorOutput "✓ Docker 已安装" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "✗ Docker 未安装或未运行" "Red"
        Write-ColorOutput "请先安装 Docker Desktop: https://www.docker.com/products/docker-desktop" "Yellow"
        return $false
    }
}

# 检查Docker服务是否运行
function Test-DockerService {
    try {
        docker ps | Out-Null
        Write-ColorOutput "✓ Docker 服务正在运行" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "✗ Docker 服务未运行" "Red"
        Write-ColorOutput "请启动 Docker Desktop" "Yellow"
        return $false
    }
}

# 检查是否在正确的目录
function Test-ProjectDirectory {
    if (Test-Path "LLX.Server.sln") {
        Write-ColorOutput "✓ 在项目根目录" "Green"
        return $true
    } else {
        Write-ColorOutput "✗ 不在项目根目录" "Red"
        Write-ColorOutput "请切换到项目根目录运行此脚本" "Yellow"
        return $false
    }
}

# 检查环境变量文件
function Test-EnvironmentFile {
    if (-not (Test-Path ".env")) {
        Write-ColorOutput "⚠ 环境变量文件 .env 不存在" "Yellow"
        
        if (Test-Path "env.api-only.example") {
            Write-ColorOutput "发现 env.api-only.example 示例文件" "White"
            $copy = Read-Host "是否复制示例文件为 .env？(y/n)"
            if ($copy -eq "y" -or $copy -eq "Y") {
                Copy-Item "env.api-only.example" ".env"
                Write-ColorOutput "✓ 已创建 .env 文件" "Green"
                Write-ColorOutput "⚠ 请立即编辑 .env 文件，配置数据库和Redis连接信息" "Yellow"
                Write-ColorOutput "配置完成后，请重新运行此脚本" "Yellow"
                
                # 显示需要配置的关键项
                Write-ColorOutput "`n需要配置的关键项:" "Cyan"
                Write-ColorOutput "  1. DB_CONNECTION_STRING - PostgreSQL数据库连接字符串" "White"
                Write-ColorOutput "  2. REDIS_CONNECTION_STRING - Redis缓存连接字符串" "White"
                
                return $false
            } else {
                Write-ColorOutput "✗ 需要 .env 文件才能继续部署" "Red"
                return $false
            }
        } else {
            Write-ColorOutput "✗ 找不到环境变量示例文件" "Red"
            return $false
        }
    }
    
    Write-ColorOutput "✓ 环境变量文件存在" "Green"
    return $true
}

# 验证环境变量配置
function Test-EnvironmentVariables {
    Write-ColorOutput "`n验证环境变量配置..." "Cyan"
    
    # 读取.env文件
    $envContent = Get-Content ".env" | Where-Object { $_ -notmatch "^#" -and $_ -notmatch "^\s*$" }
    
    $hasDbConnection = $false
    $hasRedisConnection = $false
    
    foreach ($line in $envContent) {
        if ($line -match "^DB_CONNECTION_STRING=(.+)") {
            $value = $matches[1]
            if ($value -and $value -ne "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_strong_password_here;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30") {
                $hasDbConnection = $true
                Write-ColorOutput "  ✓ 数据库连接配置已设置" "Green"
            }
        }
        if ($line -match "^REDIS_CONNECTION_STRING=(.+)") {
            $value = $matches[1]
            if ($value -and $value -ne "localhost:6379,password=your_redis_password_here,ssl=false,abortConnect=false") {
                $hasRedisConnection = $true
                Write-ColorOutput "  ✓ Redis连接配置已设置" "Green"
            }
        }
    }
    
    if (-not $hasDbConnection) {
        Write-ColorOutput "  ⚠ 数据库连接未配置或使用默认值" "Yellow"
        Write-ColorOutput "  请在 .env 文件中配置 DB_CONNECTION_STRING" "Yellow"
    }
    
    if (-not $hasRedisConnection) {
        Write-ColorOutput "  ⚠ Redis连接未配置或使用默认值" "Yellow"
        Write-ColorOutput "  请在 .env 文件中配置 REDIS_CONNECTION_STRING" "Yellow"
    }
    
    if (-not $hasDbConnection -or -not $hasRedisConnection) {
        $continue = Read-Host "`n是否继续部署？(y/n)"
        if ($continue -ne "y" -and $continue -ne "Y") {
            return $false
        }
    }
    
    return $true
}

# 测试数据库连接
function Test-DatabaseConnection {
    Write-ColorOutput "`n测试数据库连接..." "Cyan"
    
    # 这里可以添加实际的数据库连接测试
    # 暂时跳过，因为需要额外的工具
    
    Write-ColorOutput "  ⚠ 跳过数据库连接测试" "Yellow"
    Write-ColorOutput "  请确保PostgreSQL数据库可访问" "Yellow"
}

# 测试Redis连接
function Test-RedisConnection {
    Write-ColorOutput "`n测试Redis连接..." "Cyan"
    
    # 这里可以添加实际的Redis连接测试
    # 暂时跳过，因为需要额外的工具
    
    Write-ColorOutput "  ⚠ 跳过Redis连接测试" "Yellow"
    Write-ColorOutput "  请确保Redis服务可访问" "Yellow"
}

# 停止并删除现有容器
function Stop-ExistingContainer {
    Write-ColorOutput "`n停止现有API容器..." "Yellow"
    
    try {
        # 停止容器
        $container = docker ps -a --filter "name=llxrice_api" --format "{{.Names}}"
        if ($container) {
            docker stop llxrice_api 2>$null
            docker rm llxrice_api 2>$null
            Write-ColorOutput "✓ 已停止并删除现有容器" "Green"
        } else {
            Write-ColorOutput "  没有找到现有容器" "Gray"
        }
    }
    catch {
        Write-ColorOutput "  ⚠ 停止容器时出现警告" "Yellow"
    }
}

# 构建.NET项目
function Build-DotNetProject {
    if ($NoBuild) {
        Write-ColorOutput "`n跳过.NET项目编译（使用已有构建）..." "Yellow"
        return
    }
    
    Write-ColorOutput "`n编译.NET项目..." "Cyan"
    
    try {
        Set-Location "LLX.Server"
        
        # 清理
        Write-ColorOutput "  清理旧文件..." "Gray"
        dotnet clean --configuration Release | Out-Null
        
        # 编译
        Write-ColorOutput "  编译项目..." "Gray"
        $buildOutput = dotnet build --configuration Release 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "✓ 项目编译成功" "Green"
        } else {
            Write-ColorOutput "✗ 项目编译失败" "Red"
            Write-ColorOutput $buildOutput "Red"
            Set-Location ".."
            return $false
        }
        
        Set-Location ".."
        return $true
    }
    catch {
        Write-ColorOutput "✗ 编译过程出错: $($_.Exception.Message)" "Red"
        Set-Location ".."
        return $false
    }
}

# 构建Docker镜像
function Build-DockerImage {
    Write-ColorOutput "`n构建Docker镜像..." "Cyan"
    
    try {
        if ($Build) {
            Write-ColorOutput "  强制重新构建镜像..." "Yellow"
            docker build -f LLX.Server/Dockerfile -t llxrice/api:latest --no-cache .
        } else {
            docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "✓ Docker镜像构建成功" "Green"
            return $true
        } else {
            Write-ColorOutput "✗ Docker镜像构建失败" "Red"
            return $false
        }
    }
    catch {
        Write-ColorOutput "✗ 构建镜像时出错: $($_.Exception.Message)" "Red"
        return $false
    }
}

# 启动API服务
function Start-ApiService {
    Write-ColorOutput "`n启动API服务..." "Cyan"
    
    try {
        # 使用docker-compose启动
        if (Test-Path "docker-compose.api-only.yml") {
            docker-compose -f docker-compose.api-only.yml up -d
        } else {
            Write-ColorOutput "✗ 找不到 docker-compose.api-only.yml 文件" "Red"
            return $false
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "✓ API服务启动成功" "Green"
            return $true
        } else {
            Write-ColorOutput "✗ API服务启动失败" "Red"
            return $false
        }
    }
    catch {
        Write-ColorOutput "✗ 启动服务时出错: $($_.Exception.Message)" "Red"
        return $false
    }
}

# 等待服务就绪
function Wait-ForService {
    Write-ColorOutput "`n等待服务就绪..." "Yellow"
    
    $maxAttempts = 30
    $attempt = 0
    $port = 8080
    
    # 从环境变量读取端口
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        foreach ($line in $envContent) {
            if ($line -match "^API_PORT=(\d+)") {
                $port = [int]$matches[1]
                break
            }
        }
    }
    
    do {
        $attempt++
        Start-Sleep -Seconds 2
        
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$port/health" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-ColorOutput "`n✓ 服务已就绪！" "Green"
                return $true
            }
        }
        catch {
            Write-Host "." -NoNewline
        }
    } while ($attempt -lt $maxAttempts)
    
    Write-ColorOutput "`n⚠ 服务启动超时" "Yellow"
    Write-ColorOutput "可以运行以下命令查看日志:" "White"
    Write-ColorOutput "  docker logs llxrice_api" "Cyan"
    return $false
}

# 显示服务状态
function Show-ServiceStatus {
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "📊 服务状态" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    
    # 显示容器状态
    docker ps --filter "name=llxrice_api" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    # 获取端口
    $port = 8080
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        foreach ($line in $envContent) {
            if ($line -match "^API_PORT=(\d+)") {
                $port = [int]$matches[1]
                break
            }
        }
    }
    
    Write-ColorOutput "`n📋 访问信息" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "  API服务:      http://localhost:$port" "White"
    Write-ColorOutput "  健康检查:     http://localhost:$port/health" "White"
    Write-ColorOutput "  Swagger文档:  http://localhost:$port/swagger" "White"
    
    Write-ColorOutput "`n📝 常用命令" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "  查看日志:     docker logs -f llxrice_api" "White"
    Write-ColorOutput "  停止服务:     docker stop llxrice_api" "White"
    Write-ColorOutput "  启动服务:     docker start llxrice_api" "White"
    Write-ColorOutput "  重启服务:     docker restart llxrice_api" "White"
    Write-ColorOutput "  删除容器:     docker rm -f llxrice_api" "White"
}

# 显示日志选项
function Show-LogsOption {
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    $showLogs = Read-Host "是否查看实时日志？(y/n)"
    if ($showLogs -eq "y" -or $showLogs -eq "Y") {
        Write-ColorOutput "`n📋 实时日志 (按 Ctrl+C 退出)" "Cyan"
        Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
        docker logs -f llxrice_api
    }
}

# 主函数
function Main {
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "🚀 林龍香大米商城 - API服务独立部署" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "部署环境: $Environment" "White"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" "Cyan"
    
    # 1. 环境检查
    Write-ColorOutput "【步骤 1/8】环境检查" "Cyan"
    if (-not (Test-Docker)) { exit 1 }
    if (-not (Test-DockerService)) { exit 1 }
    if (-not (Test-ProjectDirectory)) { exit 1 }
    
    # 2. 配置检查
    Write-ColorOutput "`n【步骤 2/8】配置检查" "Cyan"
    if (-not (Test-EnvironmentFile)) { exit 1 }
    if (-not (Test-EnvironmentVariables)) { exit 1 }
    
    # 3. 连接测试（可选）
    Write-ColorOutput "`n【步骤 3/8】连接测试" "Cyan"
    Test-DatabaseConnection
    Test-RedisConnection
    
    # 4. 停止现有容器
    Write-ColorOutput "`n【步骤 4/8】停止现有容器" "Cyan"
    if ($Force) {
        Stop-ExistingContainer
    } else {
        $container = docker ps -a --filter "name=llxrice_api" --format "{{.Names}}"
        if ($container) {
            Write-ColorOutput "  发现现有容器: $container" "Yellow"
            $stop = Read-Host "是否停止并删除？(y/n)"
            if ($stop -eq "y" -or $stop -eq "Y") {
                Stop-ExistingContainer
            }
        }
    }
    
    # 5. 编译.NET项目
    Write-ColorOutput "`n【步骤 5/8】编译项目" "Cyan"
    if (-not (Build-DotNetProject)) { exit 1 }
    
    # 6. 构建Docker镜像
    Write-ColorOutput "`n【步骤 6/8】构建Docker镜像" "Cyan"
    if (-not (Build-DockerImage)) { exit 1 }
    
    # 7. 启动服务
    Write-ColorOutput "`n【步骤 7/8】启动API服务" "Cyan"
    if (-not (Start-ApiService)) { exit 1 }
    
    # 8. 等待服务就绪
    Write-ColorOutput "`n【步骤 8/8】验证服务" "Cyan"
    Wait-ForService
    
    # 显示状态
    Show-ServiceStatus
    
    # 显示日志选项
    Show-LogsOption
    
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
    Write-ColorOutput "🎉 API服务部署完成！" "Green"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
}

# 执行主函数
try {
    Main
}
catch {
    Write-ColorOutput "`n✗ 部署过程出现错误: $($_.Exception.Message)" "Red"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Red"
    exit 1
}
