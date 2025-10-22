# 部署问题修复脚本
# 解决Docker Compose配置文件路径问题

Write-Host "🔧 修复部署配置问题..." -ForegroundColor Cyan

# 检查当前目录
$currentDir = Get-Location
Write-Host "当前目录: $currentDir" -ForegroundColor White

# 检查是否在正确的目录
if (Test-Path "LLX.Server.sln") {
    Write-Host "✓ 在项目根目录" -ForegroundColor Green
} else {
    Write-Host "✗ 不在项目根目录，请切换到项目根目录" -ForegroundColor Red
    Write-Host "请运行: cd .." -ForegroundColor Yellow
    exit 1
}

# 检查docker-compose.yml是否存在
if (Test-Path "docker-compose.yml") {
    Write-Host "✓ docker-compose.yml 已存在" -ForegroundColor Green
} else {
    Write-Host "⚠ docker-compose.yml 不存在，正在复制..." -ForegroundColor Yellow
    if (Test-Path "doc/docker-compose.yml") {
        Copy-Item "doc/docker-compose.yml" "."
        Write-Host "✓ 已复制 docker-compose.yml" -ForegroundColor Green
    } else {
        Write-Host "✗ 找不到 docker-compose.yml 文件" -ForegroundColor Red
        exit 1
    }
}

# 检查env.example是否存在
if (Test-Path "env.example") {
    Write-Host "✓ env.example 已存在" -ForegroundColor Green
} else {
    Write-Host "⚠ env.example 不存在，正在复制..." -ForegroundColor Yellow
    if (Test-Path "doc/env.example") {
        Copy-Item "doc/env.example" "."
        Write-Host "✓ 已复制 env.example" -ForegroundColor Green
    } else {
        Write-Host "✗ 找不到 env.example 文件" -ForegroundColor Red
        exit 1
    }
}

# 检查.env文件
if (Test-Path ".env") {
    Write-Host "✓ .env 文件已存在" -ForegroundColor Green
} else {
    Write-Host "⚠ .env 文件不存在，正在创建..." -ForegroundColor Yellow
    if (Test-Path "env.example") {
        Copy-Item "env.example" ".env"
        Write-Host "✓ 已创建 .env 文件" -ForegroundColor Green
        Write-Host "请编辑 .env 文件配置相应的值" -ForegroundColor Yellow
    }
}

# 检查Dockerfile路径
if (Test-Path "LLX.Server/Dockerfile") {
    Write-Host "✓ Dockerfile 存在" -ForegroundColor Green
} else {
    Write-Host "✗ 找不到 Dockerfile" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 部署配置修复完成!" -ForegroundColor Green
Write-Host "现在可以运行部署脚本了:" -ForegroundColor White
Write-Host "  .\LLX.Server\scripts\deploy.ps1" -ForegroundColor Cyan
