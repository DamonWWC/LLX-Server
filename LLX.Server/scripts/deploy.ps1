# 林龍香大米商城后端服务一键部署脚本
# PowerShell 版本

param(
    [string]$Environment = "production",
    [switch]$Build = $false,
    [switch]$Pull = $false,
    [switch]$Force = $false,
    [switch]$Help = $false
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
林龍香大米商城后端服务部署脚本

用法: .\deploy.ps1 [参数]

参数:
  -Environment <string>  部署环境 (development/production) [默认: production]
  -Build                 强制重新构建镜像
  -Pull                  拉取最新镜像
  -Force                 强制重新部署（停止并删除现有容器）
  -Help                  显示此帮助信息

示例:
  .\deploy.ps1                           # 生产环境部署
  .\deploy.ps1 -Environment development  # 开发环境部署
  .\deploy.ps1 -Build -Force             # 强制重新构建并部署
  .\deploy.ps1 -Pull                     # 拉取最新镜像并部署

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
    }
    catch {
        Write-ColorOutput "✗ Docker 未安装或未运行" "Red"
        exit 1
    }
}

# 检查Docker Compose是否安装
function Test-DockerCompose {
    try {
        docker-compose --version | Out-Null
        Write-ColorOutput "✓ Docker Compose 已安装" "Green"
    }
    catch {
        Write-ColorOutput "✗ Docker Compose 未安装" "Red"
        exit 1
    }
}

# 检查环境变量文件
function Test-EnvironmentFile {
    $envFile = ".env"
    if (-not (Test-Path $envFile)) {
        Write-ColorOutput "⚠ 环境变量文件 .env 不存在" "Yellow"
        Write-ColorOutput "请复制 env.example 为 .env 并配置相应的值" "Yellow"
        
        $copy = Read-Host "是否现在复制示例文件? (y/n)"
        if ($copy -eq "y" -or $copy -eq "Y") {
            if (Test-Path "env.example") {
                Copy-Item "env.example" ".env"
                Write-ColorOutput "✓ 已复制环境变量示例文件" "Green"
            } elseif (Test-Path "doc/env.example") {
                Copy-Item "doc/env.example" ".env"
                Write-ColorOutput "✓ 已复制环境变量示例文件" "Green"
            } else {
                Write-ColorOutput "✗ 找不到环境变量示例文件" "Red"
                exit 1
            }
            Write-ColorOutput "请编辑 .env 文件配置相应的值" "Yellow"
            exit 0
        } else {
            exit 1
        }
    } else {
        Write-ColorOutput "✓ 环境变量文件存在" "Green"
    }
}

# 停止并删除现有容器
function Stop-ExistingContainers {
    Write-ColorOutput "🔄 停止现有容器..." "Yellow"
    
    try {
        docker-compose down --remove-orphans
        Write-ColorOutput "✓ 现有容器已停止" "Green"
    }
    catch {
        Write-ColorOutput "⚠ 停止容器时出现警告，继续执行..." "Yellow"
    }
}

# 拉取最新镜像
function Pull-LatestImages {
    Write-ColorOutput "🔄 拉取最新镜像..." "Yellow"
    
    try {
        docker-compose pull
        Write-ColorOutput "✓ 镜像拉取完成" "Green"
    }
    catch {
        Write-ColorOutput "⚠ 镜像拉取失败，使用本地镜像" "Yellow"
    }
}

# 构建镜像
function Build-Images {
    Write-ColorOutput "🔄 构建应用镜像..." "Yellow"
    
    try {
        docker-compose build --no-cache
        Write-ColorOutput "✓ 镜像构建完成" "Green"
    }
    catch {
        Write-ColorOutput "✗ 镜像构建失败" "Red"
        exit 1
    }
}

# 启动服务
function Start-Services {
    Write-ColorOutput "🔄 启动服务..." "Yellow"
    
    try {
        if ($Environment -eq "development") {
            docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
        } else {
            docker-compose up -d
        }
        Write-ColorOutput "✓ 服务启动完成" "Green"
    }
    catch {
        Write-ColorOutput "✗ 服务启动失败" "Red"
        exit 1
    }
}

# 等待服务就绪
function Wait-ForServices {
    Write-ColorOutput "🔄 等待服务就绪..." "Yellow"
    
    $maxAttempts = 30
    $attempt = 0
    
    do {
        $attempt++
        Start-Sleep -Seconds 2
        
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -TimeoutSec 5 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-ColorOutput "✓ 服务已就绪" "Green"
                return
            }
        }
        catch {
            # 继续等待
        }
        
        Write-Host "." -NoNewline
    } while ($attempt -lt $maxAttempts)
    
    Write-ColorOutput "⚠ 服务启动超时，请检查日志" "Yellow"
}

# 显示服务状态
function Show-ServiceStatus {
    Write-ColorOutput "`n📊 服务状态:" "Cyan"
    docker-compose ps
    
    Write-ColorOutput "`n📋 服务信息:" "Cyan"
    Write-ColorOutput "API 服务: http://localhost:8080" "White"
    Write-ColorOutput "健康检查: http://localhost:8080/health" "White"
    Write-ColorOutput "Swagger 文档: http://localhost:8080/swagger" "White"
    Write-ColorOutput "数据库: localhost:5432" "White"
    Write-ColorOutput "Redis: localhost:6379" "White"
}

# 显示日志
function Show-Logs {
    $showLogs = Read-Host "`n是否查看服务日志? (y/n)"
    if ($showLogs -eq "y" -or $showLogs -eq "Y") {
        Write-ColorOutput "`n📋 服务日志 (按 Ctrl+C 退出):" "Cyan"
        docker-compose logs -f
    }
}

# 主函数
function Main {
    Write-ColorOutput "🚀 林龍香大米商城后端服务部署脚本" "Cyan"
    Write-ColorOutput "部署环境: $Environment" "White"
    Write-ColorOutput "===========================================" "Cyan"
    
    # 预检查
    Test-Docker
    Test-DockerCompose
    Test-EnvironmentFile
    
    # 停止现有容器
    if ($Force) {
        Stop-ExistingContainers
    }
    
    # 拉取最新镜像
    if ($Pull) {
        Pull-LatestImages
    }
    
    # 构建镜像
    if ($Build) {
        Build-Images
    }
    
    # 启动服务
    Start-Services
    
    # 等待服务就绪
    Wait-ForServices
    
    # 显示状态
    Show-ServiceStatus
    
    # 显示日志
    Show-Logs
    
    Write-ColorOutput "`n🎉 部署完成!" "Green"
}

# 执行主函数
Main
