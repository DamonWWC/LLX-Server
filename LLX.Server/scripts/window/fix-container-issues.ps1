# 林龍香大米商城 - 容器运行问题修复脚本
# Windows PowerShell 版本

param(
    [switch]$Force,
    [switch]$NoRebuild,
    [switch]$RestartOnly,
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
    Write-ColorOutput "林龍香大米商城 - 容器运行问题修复脚本" "Cyan"
    Write-ColorOutput ""
    Write-ColorOutput "用法: .\fix-container-issues.ps1 [参数]"
    Write-ColorOutput ""
    Write-ColorOutput "参数:"
    Write-ColorOutput "  -Force             强制修复（不询问确认）"
    Write-ColorOutput "  -NoRebuild         不重新构建镜像"
    Write-ColorOutput "  -RestartOnly       仅重启服务"
    Write-ColorOutput "  -Help              显示此帮助信息"
    Write-ColorOutput ""
    Write-ColorOutput "修复的问题:"
    Write-ColorOutput "  1. 日志文件权限问题"
    Write-ColorOutput "  2. 数据库连接问题（localhost -> host.docker.internal）"
    Write-ColorOutput "  3. Redis连接问题"
    Write-ColorOutput "  4. 容器网络配置"
    Write-ColorOutput ""
    Write-ColorOutput "示例:"
    Write-ColorOutput "  .\fix-container-issues.ps1                    # 标准修复"
    Write-ColorOutput "  .\fix-container-issues.ps1 -Force            # 强制修复"
    Write-ColorOutput "  .\fix-container-issues.ps1 -NoRebuild        # 不重新构建镜像"
    Write-ColorOutput "  .\fix-container-issues.ps1 -RestartOnly      # 仅重启服务"
}

# 检查是否在项目根目录
function Test-ProjectDirectory {
    if (-not (Test-Path "LLX.Server.sln")) {
        Write-ColorOutput "✗ 不在项目根目录" "Red"
        Write-ColorOutput "请切换到项目根目录运行此脚本" "Yellow"
        return $false
    }
    Write-ColorOutput "✓ 在项目根目录" "Green"
    return $true
}

# 检查Docker服务
function Test-DockerService {
    try {
        $dockerVersion = docker --version 2>$null
        if (-not $dockerVersion) {
            Write-ColorOutput "✗ Docker 未安装" "Red"
            return $false
        }
        
        $dockerPs = docker ps 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "✗ Docker 服务未运行" "Red"
            return $false
        }
        
        Write-ColorOutput "✓ Docker 服务正常" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "✗ Docker 检查失败: $($_.Exception.Message)" "Red"
        return $false
    }
}

# 创建日志目录并设置权限
function New-LogsDirectory {
    Write-ColorOutput "修复日志目录权限..." "Cyan"
    
    # 创建日志目录
    if (-not (Test-Path "logs")) {
        New-Item -ItemType Directory -Path "logs" -Force | Out-Null
        Write-ColorOutput "  创建日志目录: logs/" "Gray"
    }
    else {
        Write-ColorOutput "  日志目录已存在: logs/" "Gray"
    }
    
    # 设置权限（Windows下权限管理不同）
    try {
        $acl = Get-Acl "logs"
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
        Set-Acl "logs" $acl
        Write-ColorOutput "  设置目录权限" "Gray"
    }
    catch {
        Write-ColorOutput "  ⚠ 权限设置失败，但目录已创建" "Yellow"
    }
    
    Write-ColorOutput "✓ 日志目录权限修复完成" "Green"
}

# 检查并修复环境变量配置
function Test-EnvironmentConfig {
    Write-ColorOutput "检查环境变量配置..." "Cyan"
    
    # 检查.env文件是否存在
    if (-not (Test-Path ".env")) {
        Write-ColorOutput "⚠ .env 文件不存在" "Yellow"
        if (Test-Path "env.api-only.example") {
            Write-ColorOutput "  复制示例文件..." "Gray"
            Copy-Item "env.api-only.example" ".env"
            Write-ColorOutput "✓ 已创建 .env 文件" "Green"
        }
        else {
            Write-ColorOutput "✗ 找不到环境变量示例文件" "Red"
            return $false
        }
    }
    else {
        Write-ColorOutput "✓ .env 文件存在" "Green"
    }
    
    # 检查localhost配置
    $envContent = Get-Content ".env" -Raw
    if ($envContent -match "localhost") {
        Write-ColorOutput "⚠ 发现localhost配置，需要修改为容器网络地址" "Yellow"
        
        if (-not $Force) {
            $response = Read-Host "是否自动修复localhost配置？(y/n)"
            if ($response -match "^[Yy]$") {
                # 备份原文件
                Copy-Item ".env" ".env.backup"
                Write-ColorOutput "  备份原配置: .env.backup" "Gray"
                
                # 替换localhost为host.docker.internal
                $newContent = $envContent -replace "Host=localhost", "Host=host.docker.internal"
                $newContent = $newContent -replace "localhost:6379", "host.docker.internal:6379"
                
                $newContent | Out-File -FilePath ".env" -Encoding UTF8
                
                Write-ColorOutput "✓ 已修复localhost配置" "Green"
                Write-ColorOutput "⚠ 请检查并确认数据库和Redis连接信息" "Yellow"
            }
            else {
                Write-ColorOutput "⚠ 请手动修改 .env 文件中的localhost配置" "Yellow"
                Write-ColorOutput "  建议修改为:" "White"
                Write-ColorOutput "    DB_CONNECTION_STRING=Host=host.docker.internal;Port=5432;..." "White"
                Write-ColorOutput "    REDIS_CONNECTION_STRING=host.docker.internal:6379,..." "White"
            }
        }
        else {
            # 强制修复
            Copy-Item ".env" ".env.backup"
            $newContent = $envContent -replace "Host=localhost", "Host=host.docker.internal"
            $newContent = $newContent -replace "localhost:6379", "host.docker.internal:6379"
            $newContent | Out-File -FilePath ".env" -Encoding UTF8
            Write-ColorOutput "✓ 已强制修复localhost配置" "Green"
        }
    }
    else {
        Write-ColorOutput "✓ 环境变量配置正常" "Green"
    }
    
    return $true
}

# 重新构建Docker镜像
function Build-DockerImage {
    if ($NoRebuild) {
        Write-ColorOutput "跳过Docker镜像构建" "Yellow"
        return $true
    }
    
    Write-ColorOutput "重新构建Docker镜像..." "Cyan"
    
    try {
        # 编译.NET项目
        Write-ColorOutput "  编译.NET项目..." "Gray"
        Set-Location "LLX.Server"
        
        dotnet clean --configuration Release | Out-Null
        dotnet build --configuration Release
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "  ✓ .NET项目编译成功" "Green"
        }
        else {
            Write-ColorOutput "  ✗ .NET项目编译失败" "Red"
            Set-Location ".."
            return $false
        }
        Set-Location ".."
        
        # 构建Docker镜像
        Write-ColorOutput "  构建Docker镜像..." "Gray"
        docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "  ✓ Docker镜像构建成功" "Green"
        }
        else {
            Write-ColorOutput "  ✗ Docker镜像构建失败" "Red"
            return $false
        }
    }
    catch {
        Write-ColorOutput "✗ 构建失败: $($_.Exception.Message)" "Red"
        return $false
    }
    
    return $true
}

# 停止现有容器
function Stop-ExistingContainer {
    Write-ColorOutput "停止现有容器..." "Cyan"
    
    try {
        $container = docker ps -a --filter "name=llxrice_api" --format "{{.Names}}" 2>$null
        if ($container -eq "llxrice_api") {
            docker stop llxrice_api
            docker rm llxrice_api
            Write-ColorOutput "✓ 已停止并删除现有容器" "Green"
        }
        else {
            Write-ColorOutput "  没有找到现有容器" "Gray"
        }
    }
    catch {
        Write-ColorOutput "⚠ 停止容器时出现错误: $($_.Exception.Message)" "Yellow"
    }
}

# 启动服务
function Start-Service {
    Write-ColorOutput "启动服务..." "Cyan"
    
    try {
        docker-compose -f docker-compose.api-only.yml up -d
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "✓ 服务启动成功" "Green"
        }
        else {
            Write-ColorOutput "✗ 服务启动失败" "Red"
            return $false
        }
    }
    catch {
        Write-ColorOutput "✗ 服务启动失败: $($_.Exception.Message)" "Red"
        return $false
    }
    
    return $true
}

# 等待服务就绪
function Wait-ForService {
    Write-ColorOutput "等待服务就绪..." "Yellow"
    
    $maxAttempts = 30
    $attempt = 0
    $port = 8080
    
    # 从环境变量读取端口
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        $portLine = $envContent | Where-Object { $_ -match "^API_PORT=" }
        if ($portLine) {
            $port = $portLine.Split('=')[1]
        }
    }
    
    while ($attempt -lt $maxAttempts) {
        $attempt++
        Start-Sleep -Seconds 2
        
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$port/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-ColorOutput "✓ 服务已就绪！" "Green"
                return $true
            }
        }
        catch {
            # 忽略错误，继续等待
        }
        
        Write-Host "." -NoNewline
    }
    
    Write-ColorOutput "`n⚠ 服务启动超时" "Yellow"
    Write-ColorOutput "可以运行以下命令查看日志:" "White"
    Write-ColorOutput "  docker logs llxrice_api" "Cyan"
    return $false
}

# 验证修复
function Test-Fix {
    Write-ColorOutput "验证修复结果..." "Cyan"
    
    # 检查容器状态
    $container = docker ps --filter "name=llxrice_api" --format "{{.Names}}" 2>$null
    if ($container -eq "llxrice_api") {
        Write-ColorOutput "✓ 容器运行正常" "Green"
    }
    else {
        Write-ColorOutput "✗ 容器未运行" "Red"
        return $false
    }
    
    # 检查日志权限
    try {
        docker exec llxrice_api ls -la /app/logs > $null 2>&1
        Write-ColorOutput "✓ 日志目录权限正常" "Green"
    }
    catch {
        Write-ColorOutput "⚠ 日志目录权限可能仍有问题" "Yellow"
    }
    
    # 检查健康状态
    $port = 8080
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        $portLine = $envContent | Where-Object { $_ -match "^API_PORT=" }
        if ($portLine) {
            $port = $portLine.Split('=')[1]
        }
    }
    
    try {
        $healthResponse = Invoke-WebRequest -Uri "http://localhost:$port/health" -UseBasicParsing -TimeoutSec 10 -ErrorAction SilentlyContinue
        if ($healthResponse.StatusCode -eq 200) {
            Write-ColorOutput "✓ 健康检查通过" "Green"
        }
        else {
            Write-ColorOutput "⚠ 健康检查失败，请检查数据库和Redis连接" "Yellow"
        }
    }
    catch {
        Write-ColorOutput "⚠ 健康检查失败，请检查数据库和Redis连接" "Yellow"
    }
    
    return $true
}

# 显示服务状态
function Show-Status {
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "📊 服务状态" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    
    # 显示容器状态
    docker ps --filter "name=llxrice_api" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    # 获取端口
    $port = 8080
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        $portLine = $envContent | Where-Object { $_ -match "^API_PORT=" }
        if ($portLine) {
            $port = $portLine.Split('=')[1]
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
    Write-ColorOutput "  重启服务:     docker restart llxrice_api" "White"
    Write-ColorOutput "  进入容器:     docker exec -it llxrice_api bash" "White"
}

# 主函数
function Main {
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "🔧 林龍香大米商城 - 容器问题修复" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput ""
    
    # 显示帮助信息
    if ($Help) {
        Show-Help
        return
    }
    
    # 检查环境
    if (-not (Test-ProjectDirectory)) { exit 1 }
    if (-not (Test-DockerService)) { exit 1 }
    
    # 确认修复
    if (-not $Force -and -not $RestartOnly) {
        Write-ColorOutput "即将修复以下问题:" "Yellow"
        Write-ColorOutput "  1. 日志文件权限问题" "White"
        Write-ColorOutput "  2. 数据库连接问题（localhost -> host.docker.internal）" "White"
        Write-ColorOutput "  3. Redis连接问题" "White"
        Write-ColorOutput "  4. 容器网络配置" "White"
        Write-ColorOutput ""
        $response = Read-Host "是否继续？(y/n)"
        if ($response -notmatch "^[Yy]$") {
            Write-ColorOutput "✗ 用户取消操作" "Red"
            return
        }
    }
    
    # 修复日志权限
    New-LogsDirectory
    
    # 修复环境配置
    if (-not (Test-EnvironmentConfig)) { exit 1 }
    
    # 重新构建镜像（如果需要）
    if (-not $RestartOnly) {
        if (-not (Build-DockerImage)) { exit 1 }
    }
    
    # 停止现有容器
    Stop-ExistingContainer
    
    # 启动服务
    if (-not (Start-Service)) { exit 1 }
    
    # 等待服务就绪
    Wait-ForService
    
    # 验证修复
    Test-Fix
    
    # 显示状态
    Show-Status
    
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
    Write-ColorOutput "🎉 容器问题修复完成！" "Green"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
    
    Write-ColorOutput "`n⚠ 如果仍有问题，请检查:" "Yellow"
    Write-ColorOutput "  1. 数据库和Redis服务是否运行" "White"
    Write-ColorOutput "  2. 网络连接是否正常" "White"
    Write-ColorOutput "  3. 环境变量配置是否正确" "White"
    Write-ColorOutput "  4. 查看详细日志: docker logs llxrice_api" "White"
}

# 执行主函数
Main
