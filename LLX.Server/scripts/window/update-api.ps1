# 林龍香大米商城 - API服务快速更新脚本
# Windows PowerShell 版本

param(
    [switch]$Backup,
    [switch]$Force,
    [switch]$Rollback,
    [string]$Version = "",
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
    Write-ColorOutput "林龍香大米商城 - API服务快速更新脚本" "Cyan"
    Write-ColorOutput ""
    Write-ColorOutput "用法: .\update-api.ps1 [参数]"
    Write-ColorOutput ""
    Write-ColorOutput "参数:"
    Write-ColorOutput "  -Backup           更新前创建备份"
    Write-ColorOutput "  -Force            强制更新（不询问确认）"
    Write-ColorOutput "  -Rollback         回滚到上一个版本"
    Write-ColorOutput "  -Version <tag>    更新到指定版本"
    Write-ColorOutput "  -Help             显示此帮助信息"
    Write-ColorOutput ""
    Write-ColorOutput "示例:"
    Write-ColorOutput "  .\update-api.ps1                    # 标准更新"
    Write-ColorOutput "  .\update-api.ps1 -Backup            # 带备份的更新"
    Write-ColorOutput "  .\update-api.ps1 -Force             # 强制更新"
    Write-ColorOutput "  .\update-api.ps1 -Rollback          # 回滚"
    Write-ColorOutput "  .\update-api.ps1 -Version v1.2.3    # 更新到指定版本"
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

# 检查当前服务状态
function Test-ServiceStatus {
    try {
        $container = docker ps --filter "name=llxrice_api" --format "{{.Names}}" 2>$null
        if ($container -eq "llxrice_api") {
            Write-ColorOutput "✓ 发现运行中的API服务" "Green"
            return $true
        }
        else {
            Write-ColorOutput "⚠ 未发现运行中的API服务" "Yellow"
            return $false
        }
    }
    catch {
        Write-ColorOutput "⚠ 无法检查服务状态" "Yellow"
        return $false
    }
}

# 创建备份
function New-Backup {
    $backupDir = "backups\$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    
    Write-ColorOutput "创建备份到: $backupDir" "Cyan"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    
    # 备份配置文件
    if (Test-Path ".env") {
        Copy-Item ".env" "$backupDir\" -Force
        Write-ColorOutput "  ✓ 配置文件备份完成" "Green"
    }
    
    if (Test-Path "docker-compose.api-only.yml") {
        Copy-Item "docker-compose.api-only.yml" "$backupDir\" -Force
    }
    
    # 备份当前镜像
    try {
        $imageExists = docker images llxrice/api:latest --format "{{.Repository}}:{{.Tag}}" 2>$null
        if ($imageExists -eq "llxrice/api:latest") {
            Write-ColorOutput "  备份Docker镜像..." "Gray"
            docker save llxrice/api:latest -o "$backupDir\api-image-backup.tar" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-ColorOutput "  ✓ 镜像备份完成" "Green"
            }
        }
    }
    catch {
        Write-ColorOutput "  ⚠ 镜像备份失败: $($_.Exception.Message)" "Yellow"
    }
    
    # 备份日志
    if (Test-Path "logs") {
        Write-ColorOutput "  备份日志文件..." "Gray"
        Copy-Item "logs" "$backupDir\" -Recurse -Force
        Write-ColorOutput "  ✓ 日志备份完成" "Green"
    }
    
    Write-ColorOutput "✓ 备份完成: $backupDir" "Green"
    $backupDir | Out-File -FilePath ".last_backup" -Encoding UTF8
}

# 拉取最新代码
function Update-Code {
    Write-ColorOutput "拉取最新代码..." "Cyan"
    
    try {
        # 检查是否有未提交的更改
        $gitStatus = git status --porcelain 2>$null
        if ($gitStatus) {
            Write-ColorOutput "⚠ 检测到未提交的更改" "Yellow"
            if (-not $Force) {
                $response = Read-Host "是否继续？(y/n)"
                if ($response -notmatch "^[Yy]$") {
                    Write-ColorOutput "✗ 用户取消操作" "Red"
                    exit 1
                }
            }
        }
        
        # 拉取代码
        if ($Version) {
            Write-ColorOutput "  切换到版本: $Version" "Gray"
            git fetch origin
            git checkout $Version
            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "✗ 版本 $Version 不存在" "Red"
                exit 1
            }
        }
        else {
            Write-ColorOutput "  拉取最新代码..." "Gray"
            git pull origin main
            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "✗ 代码拉取失败" "Red"
                exit 1
            }
        }
        
        Write-ColorOutput "✓ 代码更新完成" "Green"
    }
    catch {
        Write-ColorOutput "✗ 代码更新失败: $($_.Exception.Message)" "Red"
        exit 1
    }
}

# 构建新镜像
function Build-NewImage {
    Write-ColorOutput "构建新Docker镜像..." "Cyan"
    
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
            exit 1
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
            exit 1
        }
    }
    catch {
        Write-ColorOutput "✗ 构建失败: $($_.Exception.Message)" "Red"
        exit 1
    }
}

# 停止现有服务
function Stop-ExistingService {
    Write-ColorOutput "停止现有服务..." "Cyan"
    
    try {
        $container = docker ps --filter "name=llxrice_api" --format "{{.Names}}" 2>$null
        if ($container -eq "llxrice_api") {
            docker stop llxrice_api
            Write-ColorOutput "✓ 服务已停止" "Green"
        }
        else {
            Write-ColorOutput "  没有运行中的服务" "Gray"
        }
    }
    catch {
        Write-ColorOutput "⚠ 停止服务时出现错误: $($_.Exception.Message)" "Yellow"
    }
}

# 启动新服务
function Start-NewService {
    Write-ColorOutput "启动新服务..." "Cyan"
    
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

# 验证更新
function Test-Update {
    Write-ColorOutput "验证更新..." "Cyan"
    
    $port = 8080
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        $portLine = $envContent | Where-Object { $_ -match "^API_PORT=" }
        if ($portLine) {
            $port = $portLine.Split('=')[1]
        }
    }
    
    try {
        # 健康检查
        $healthResponse = Invoke-WebRequest -Uri "http://localhost:$port/health" -UseBasicParsing -TimeoutSec 10 -ErrorAction SilentlyContinue
        if ($healthResponse.StatusCode -eq 200) {
            Write-ColorOutput "✓ 健康检查通过" "Green"
        }
        else {
            Write-ColorOutput "✗ 健康检查失败" "Red"
            return $false
        }
        
        # API测试
        $apiResponse = Invoke-WebRequest -Uri "http://localhost:$port/api/products" -UseBasicParsing -TimeoutSec 10 -ErrorAction SilentlyContinue
        if ($apiResponse) {
            Write-ColorOutput "✓ API测试通过" "Green"
        }
        else {
            Write-ColorOutput "⚠ API测试失败（可能是正常的，如果API需要认证）" "Yellow"
        }
        
        Write-ColorOutput "✓ 更新验证完成" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "✗ 验证失败: $($_.Exception.Message)" "Red"
        return $false
    }
}

# 回滚操作
function Invoke-Rollback {
    Write-ColorOutput "执行回滚操作..." "Cyan"
    
    # 检查是否有备份
    if (-not (Test-Path ".last_backup")) {
        Write-ColorOutput "✗ 没有找到备份信息" "Red"
        exit 1
    }
    
    $backupDir = Get-Content ".last_backup" -Raw
    if (-not (Test-Path $backupDir)) {
        Write-ColorOutput "✗ 备份目录不存在: $backupDir" "Red"
        exit 1
    }
    
    try {
        # 停止当前服务
        Write-ColorOutput "  停止当前服务..." "Gray"
        docker stop llxrice_api 2>$null
        
        # 恢复镜像
        if (Test-Path "$backupDir\api-image-backup.tar") {
            Write-ColorOutput "  恢复Docker镜像..." "Gray"
            docker load -i "$backupDir\api-image-backup.tar"
        }
        
        # 恢复配置
        if (Test-Path "$backupDir\.env") {
            Write-ColorOutput "  恢复配置文件..." "Gray"
            Copy-Item "$backupDir\.env" "." -Force
        }
        
        # 启动服务
        Write-ColorOutput "  启动服务..." "Gray"
        docker-compose -f docker-compose.api-only.yml up -d
        
        # 等待服务就绪
        Start-Sleep -Seconds 10
        
        # 验证回滚
        $healthResponse = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing -TimeoutSec 10 -ErrorAction SilentlyContinue
        if ($healthResponse.StatusCode -eq 200) {
            Write-ColorOutput "✓ 回滚成功！" "Green"
        }
        else {
            Write-ColorOutput "✗ 回滚失败" "Red"
            exit 1
        }
    }
    catch {
        Write-ColorOutput "✗ 回滚失败: $($_.Exception.Message)" "Red"
        exit 1
    }
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
}

# 主函数
function Main {
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "🔄 林龍香大米商城 - API服务更新" "Cyan"
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
    
    # 回滚操作
    if ($Rollback) {
        Invoke-Rollback
        Show-Status
        return
    }
    
    # 检查服务状态
    $serviceRunning = Test-ServiceStatus
    if (-not $serviceRunning) {
        Write-ColorOutput "⚠ 未发现运行中的服务，将进行全新部署" "Yellow"
    }
    
    # 确认更新
    if (-not $Force) {
        Write-ColorOutput "即将更新API服务" "Yellow"
        if ($Version) {
            Write-ColorOutput "目标版本: $Version" "White"
        }
        else {
            Write-ColorOutput "目标版本: 最新版本" "White"
        }
        $response = Read-Host "是否继续？(y/n)"
        if ($response -notmatch "^[Yy]$") {
            Write-ColorOutput "✗ 用户取消操作" "Red"
            return
        }
    }
    
    # 创建备份
    if ($Backup) {
        New-Backup
    }
    
    # 拉取代码
    Update-Code
    
    # 构建镜像
    Build-NewImage
    
    # 停止服务
    Stop-ExistingService
    
    # 启动服务
    if (-not (Start-NewService)) { exit 1 }
    
    # 等待就绪
    if (-not (Wait-ForService)) {
        Write-ColorOutput "✗ 服务启动失败" "Red"
        exit 1
    }
    
    # 验证更新
    if (-not (Test-Update)) {
        Write-ColorOutput "✗ 更新验证失败" "Red"
        exit 1
    }
    
    # 显示状态
    Show-Status
    
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
    Write-ColorOutput "🎉 API服务更新完成！" "Green"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
}

# 执行主函数
Main
