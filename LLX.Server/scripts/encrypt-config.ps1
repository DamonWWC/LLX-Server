# 配置文件加密脚本
# 用于快速加密配置文件中的敏感信息

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile = "appsettings.json",
    
    [Parameter(Mandatory=$false)]
    [string]$EncryptionKey = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateKey = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Help = $false
)

function Show-Help {
    Write-Host "===========================================
配置文件加密脚本
===========================================

用法:
  .\encrypt-config.ps1 [-ConfigFile <文件路径>] [-EncryptionKey <密钥>] [-GenerateKey]

参数:
  -ConfigFile      要加密的配置文件路径 (默认: appsettings.json)
  -EncryptionKey   加密密钥 (可选，不提供则使用环境变量)
  -GenerateKey     生成新的加密密钥
  -Help            显示此帮助信息

示例:
  # 生成新密钥
  .\encrypt-config.ps1 -GenerateKey

  # 使用环境变量中的密钥加密配置文件
  .\encrypt-config.ps1

  # 加密指定配置文件
  .\encrypt-config.ps1 -ConfigFile appsettings.Production.json

  # 使用指定密钥加密
  .\encrypt-config.ps1 -ConfigFile appsettings.json -EncryptionKey 'your-key-here'

环境变量:
  RILEY_ENCRYPTION_KEY  - 加密密钥
" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Message {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Warning-Message {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

# 显示帮助
if ($Help) {
    Show-Help
    exit 0
}

# 生成密钥
if ($GenerateKey) {
    Write-Host "`n生成加密密钥..." -ForegroundColor Cyan
    Set-Location (Split-Path $PSScriptRoot)
    dotnet run -- generate-key
    exit 0
}

# 检查配置文件是否存在
$configPath = Join-Path (Split-Path $PSScriptRoot) $ConfigFile
if (-not (Test-Path $configPath)) {
    Write-Error-Message "配置文件不存在: $configPath"
    exit 1
}

# 设置加密密钥环境变量
if ($EncryptionKey) {
    $env:RILEY_ENCRYPTION_KEY = $EncryptionKey
    Write-Info "使用提供的加密密钥"
} elseif ($env:RILEY_ENCRYPTION_KEY) {
    Write-Info "使用环境变量中的加密密钥"
} else {
    Write-Error-Message "未设置加密密钥!"
    Write-Host "`n请使用以下方式之一设置密钥:"
    Write-Host "  1. 设置环境变量: `$env:RILEY_ENCRYPTION_KEY='your-key'"
    Write-Host "  2. 使用参数: -EncryptionKey 'your-key'"
    Write-Host "  3. 生成新密钥: .\encrypt-config.ps1 -GenerateKey`n"
    exit 1
}

# 执行加密
Write-Host "`n开始加密配置文件..." -ForegroundColor Cyan
Write-Host "配置文件: $configPath`n"

try {
    Set-Location (Split-Path $PSScriptRoot)
    $result = dotnet run -- encrypt-config $ConfigFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "`n配置文件加密完成!"
    } else {
        Write-Error-Message "`n配置文件加密失败!"
        exit 1
    }
} catch {
    Write-Error-Message "执行加密命令时发生错误: $_"
    exit 1
}

Write-Host "`n提示: 请确保在生产环境中设置 RILEY_ENCRYPTION_KEY 环境变量。" -ForegroundColor Yellow

