#!/bin/bash

# 林龍香大米商城 - API服务快速更新脚本
# 适用于已部署的API服务更新

set -e

# 颜色输出函数
print_color() {
    local color=$1
    local message=$2
    case $color in
        "red") echo -e "\033[31m$message\033[0m" ;;
        "green") echo -e "\033[32m$message\033[0m" ;;
        "yellow") echo -e "\033[33m$message\033[0m" ;;
        "blue") echo -e "\033[34m$message\033[0m" ;;
        "cyan") echo -e "\033[36m$message\033[0m" ;;
        "white") echo -e "\033[37m$message\033[0m" ;;
        *) echo "$message" ;;
    esac
}

# 显示帮助信息
show_help() {
    cat << EOF
林龍香大米商城 - API服务快速更新脚本

用法: ./update-api.sh [选项]

选项:
  -b, --backup           更新前创建备份
  -f, --force            强制更新（不询问确认）
  -r, --rollback         回滚到上一个版本
  -v, --version <tag>    更新到指定版本
  -h, --help             显示此帮助信息

示例:
  ./update-api.sh                    # 标准更新
  ./update-api.sh -b                 # 带备份的更新
  ./update-api.sh -f                 # 强制更新
  ./update-api.sh -r                 # 回滚
  ./update-api.sh -v v1.2.3         # 更新到指定版本

EOF
}

# 默认参数
BACKUP=false
FORCE=false
ROLLBACK=false
VERSION=""
HELP=false

# 解析命令行参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -b|--backup)
            BACKUP=true
            shift
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        -r|--rollback)
            ROLLBACK=true
            shift
            ;;
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        -h|--help)
            HELP=true
            shift
            ;;
        *)
            print_color "red" "未知参数: $1"
            show_help
            exit 1
            ;;
    esac
done

# 显示帮助信息
if [ "$HELP" = true ]; then
    show_help
    exit 0
fi

# 检查是否在项目根目录
check_project_directory() {
    if [ ! -f "LLX.Server.sln" ]; then
        print_color "red" "✗ 不在项目根目录"
        print_color "yellow" "请切换到项目根目录运行此脚本"
        return 1
    fi
    print_color "green" "✓ 在项目根目录"
    return 0
}

# 检查Docker服务
check_docker() {
    if ! command -v docker &> /dev/null; then
        print_color "red" "✗ Docker 未安装"
        return 1
    fi
    
    if ! docker ps &> /dev/null; then
        print_color "red" "✗ Docker 服务未运行"
        return 1
    fi
    
    print_color "green" "✓ Docker 服务正常"
    return 0
}

# 检查当前服务状态
check_service_status() {
    if docker ps --filter "name=llxrice_api" --format "{{.Names}}" | grep -q "llxrice_api"; then
        print_color "green" "✓ 发现运行中的API服务"
        return 0
    else
        print_color "yellow" "⚠ 未发现运行中的API服务"
        return 1
    fi
}

# 创建备份
create_backup() {
    local backup_dir="backups/$(date +%Y%m%d_%H%M%S)"
    
    print_color "cyan" "创建备份到: $backup_dir"
    mkdir -p "$backup_dir"
    
    # 备份配置文件
    cp .env "$backup_dir/" 2>/dev/null || true
    cp docker-compose.api-only.yml "$backup_dir/" 2>/dev/null || true
    
    # 备份当前镜像
    if docker images llxrice/api:latest --format "{{.Repository}}:{{.Tag}}" | grep -q "llxrice/api:latest"; then
        print_color "gray" "  备份Docker镜像..."
        docker save llxrice/api:latest > "$backup_dir/api-image-backup.tar"
        print_color "green" "  ✓ 镜像备份完成"
    fi
    
    # 备份日志
    if [ -d "logs" ]; then
        print_color "gray" "  备份日志文件..."
        cp -r logs "$backup_dir/"
        print_color "green" "  ✓ 日志备份完成"
    fi
    
    print_color "green" "✓ 备份完成: $backup_dir"
    echo "$backup_dir" > .last_backup
}

# 拉取最新代码
pull_latest_code() {
    print_color "cyan" "拉取最新代码..."
    
    # 检查是否有未提交的更改
    if ! git diff --quiet || ! git diff --cached --quiet; then
        print_color "yellow" "⚠ 检测到未提交的更改"
        if [ "$FORCE" = false ]; then
            read -p "是否继续？(y/n): " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                print_color "red" "✗ 用户取消操作"
                exit 1
            fi
        fi
    fi
    
    # 拉取代码
    if [ -n "$VERSION" ]; then
        print_color "gray" "  切换到版本: $VERSION"
        git fetch origin
        git checkout "$VERSION" || {
            print_color "red" "✗ 版本 $VERSION 不存在"
            exit 1
        }
    else
        print_color "gray" "  拉取最新代码..."
        git pull origin main || {
            print_color "red" "✗ 代码拉取失败"
            exit 1
        }
    fi
    
    print_color "green" "✓ 代码更新完成"
}

# 构建新镜像
build_new_image() {
    print_color "cyan" "构建新Docker镜像..."
    
    # 编译.NET项目
    print_color "gray" "  编译.NET项目..."
    cd LLX.Server
    dotnet clean --configuration Release > /dev/null 2>&1
    if dotnet build --configuration Release; then
        print_color "green" "  ✓ .NET项目编译成功"
    else
        print_color "red" "  ✗ .NET项目编译失败"
        cd ..
        exit 1
    fi
    cd ..
    
    # 构建Docker镜像
    print_color "gray" "  构建Docker镜像..."
    if docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .; then
        print_color "green" "  ✓ Docker镜像构建成功"
    else
        print_color "red" "  ✗ Docker镜像构建失败"
        exit 1
    fi
}

# 停止现有服务
stop_existing_service() {
    print_color "cyan" "停止现有服务..."
    
    if docker ps --filter "name=llxrice_api" --format "{{.Names}}" | grep -q "llxrice_api"; then
        docker stop llxrice_api
        print_color "green" "✓ 服务已停止"
    else
        print_color "gray" "  没有运行中的服务"
    fi
}

# 启动新服务
start_new_service() {
    print_color "cyan" "启动新服务..."
    
    if docker-compose -f docker-compose.api-only.yml up -d; then
        print_color "green" "✓ 服务启动成功"
    else
        print_color "red" "✗ 服务启动失败"
        return 1
    fi
}

# 等待服务就绪
wait_for_service() {
    print_color "yellow" "等待服务就绪..."
    
    local max_attempts=30
    local attempt=0
    local port=8080
    
    # 从环境变量读取端口
    if [ -f ".env" ]; then
        local env_port=$(grep "^API_PORT=" .env | cut -d '=' -f2)
        if [ -n "$env_port" ]; then
            port=$env_port
        fi
    fi
    
    while [ $attempt -lt $max_attempts ]; do
        attempt=$((attempt + 1))
        sleep 2
        
        if curl -f -s "http://localhost:$port/health" > /dev/null 2>&1; then
            print_color "green" "✓ 服务已就绪！"
            return 0
        fi
        
        printf "."
    done
    
    print_color "yellow" "\n⚠ 服务启动超时"
    print_color "white" "可以运行以下命令查看日志:"
    print_color "cyan" "  docker logs llxrice_api"
    return 1
}

# 验证更新
verify_update() {
    print_color "cyan" "验证更新..."
    
    local port=8080
    if [ -f ".env" ]; then
        local env_port=$(grep "^API_PORT=" .env | cut -d '=' -f2)
        if [ -n "$env_port" ]; then
            port=$env_port
        fi
    fi
    
    # 健康检查
    if curl -f -s "http://localhost:$port/health" > /dev/null 2>&1; then
        print_color "green" "✓ 健康检查通过"
    else
        print_color "red" "✗ 健康检查失败"
        return 1
    fi
    
    # API测试
    if curl -s "http://localhost:$port/api/products" > /dev/null 2>&1; then
        print_color "green" "✓ API测试通过"
    else
        print_color "yellow" "⚠ API测试失败（可能是正常的，如果API需要认证）"
    fi
    
    print_color "green" "✓ 更新验证完成"
}

# 回滚操作
rollback() {
    print_color "cyan" "执行回滚操作..."
    
    # 检查是否有备份
    if [ ! -f ".last_backup" ]; then
        print_color "red" "✗ 没有找到备份信息"
        exit 1
    fi
    
    local backup_dir=$(cat .last_backup)
    if [ ! -d "$backup_dir" ]; then
        print_color "red" "✗ 备份目录不存在: $backup_dir"
        exit 1
    fi
    
    # 停止当前服务
    print_color "gray" "  停止当前服务..."
    docker stop llxrice_api 2>/dev/null || true
    
    # 恢复镜像
    if [ -f "$backup_dir/api-image-backup.tar" ]; then
        print_color "gray" "  恢复Docker镜像..."
        docker load < "$backup_dir/api-image-backup.tar"
    fi
    
    # 恢复配置
    if [ -f "$backup_dir/.env" ]; then
        print_color "gray" "  恢复配置文件..."
        cp "$backup_dir/.env" .
    fi
    
    # 启动服务
    print_color "gray" "  启动服务..."
    docker-compose -f docker-compose.api-only.yml up -d
    
    # 等待服务就绪
    sleep 10
    
    # 验证回滚
    if curl -f -s "http://localhost:8080/health" > /dev/null 2>&1; then
        print_color "green" "✓ 回滚成功！"
    else
        print_color "red" "✗ 回滚失败"
        exit 1
    fi
}

# 显示服务状态
show_status() {
    print_color "cyan" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "cyan" "📊 服务状态"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # 显示容器状态
    docker ps --filter "name=llxrice_api" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    # 获取端口
    local port=8080
    if [ -f ".env" ]; then
        local env_port=$(grep "^API_PORT=" .env | cut -d '=' -f2)
        if [ -n "$env_port" ]; then
            port=$env_port
        fi
    fi
    
    print_color "cyan" "\n📋 访问信息"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "white" "  API服务:      http://localhost:$port"
    print_color "white" "  健康检查:     http://localhost:$port/health"
    print_color "white" "  Swagger文档:  http://localhost:$port/swagger"
    
    print_color "cyan" "\n📝 常用命令"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "white" "  查看日志:     docker logs -f llxrice_api"
    print_color "white" "  停止服务:     docker stop llxrice_api"
    print_color "white" "  重启服务:     docker restart llxrice_api"
}

# 主函数
main() {
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "cyan" "🔄 林龍香大米商城 - API服务更新"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
    
    # 检查环境
    check_project_directory || exit 1
    check_docker || exit 1
    
    # 回滚操作
    if [ "$ROLLBACK" = true ]; then
        rollback
        show_status
        exit 0
    fi
    
    # 检查服务状态
    check_service_status || {
        print_color "yellow" "⚠ 未发现运行中的服务，将进行全新部署"
    }
    
    # 确认更新
    if [ "$FORCE" = false ]; then
        print_color "yellow" "即将更新API服务"
        if [ -n "$VERSION" ]; then
            print_color "white" "目标版本: $VERSION"
        else
            print_color "white" "目标版本: 最新版本"
        fi
        read -p "是否继续？(y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_color "red" "✗ 用户取消操作"
            exit 0
        fi
    fi
    
    # 创建备份
    if [ "$BACKUP" = true ]; then
        create_backup
    fi
    
    # 拉取代码
    pull_latest_code
    
    # 构建镜像
    build_new_image
    
    # 停止服务
    stop_existing_service
    
    # 启动服务
    start_new_service || exit 1
    
    # 等待就绪
    wait_for_service || {
        print_color "red" "✗ 服务启动失败"
        exit 1
    }
    
    # 验证更新
    verify_update
    
    # 显示状态
    show_status
    
    print_color "green" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "green" "🎉 API服务更新完成！"
    print_color "green" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
}

# 执行主函数
main "$@"
