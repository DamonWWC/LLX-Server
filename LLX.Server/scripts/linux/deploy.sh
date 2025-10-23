#!/bin/bash

# 林龍香大米商城后端服务一键部署脚本
# Linux/Mac 版本

set -e

# 默认参数
ENVIRONMENT="production"
BUILD=false
PULL=false
FORCE=false
HELP=false

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
        *) echo "$message" ;;
    esac
}

# 显示帮助信息
show_help() {
    cat << EOF
林龍香大米商城后端服务部署脚本

用法: ./deploy.sh [选项]

选项:
  -e, --environment <env>  部署环境 (development/production) [默认: production]
  -b, --build             强制重新构建镜像
  -p, --pull              拉取最新镜像
  -f, --force             强制重新部署（停止并删除现有容器）
  -h, --help              显示此帮助信息

示例:
  ./deploy.sh                           # 生产环境部署
  ./deploy.sh -e development            # 开发环境部署
  ./deploy.sh -b -f                     # 强制重新构建并部署
  ./deploy.sh -p                        # 拉取最新镜像并部署

EOF
}

# 解析命令行参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -b|--build)
            BUILD=true
            shift
            ;;
        -p|--pull)
            PULL=true
            shift
            ;;
        -f|--force)
            FORCE=true
            shift
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

# 检查Docker是否安装
check_docker() {
    if ! command -v docker &> /dev/null; then
        print_color "red" "✗ Docker 未安装或未运行"
        exit 1
    fi
    print_color "green" "✓ Docker 已安装"
}

# 检查Docker Compose是否安装
check_docker_compose() {
    if ! command -v docker-compose &> /dev/null; then
        print_color "red" "✗ Docker Compose 未安装"
        exit 1
    fi
    print_color "green" "✓ Docker Compose 已安装"
}

# 检查环境变量文件
check_environment_file() {
    if [ ! -f ".env" ]; then
        print_color "yellow" "⚠ 环境变量文件 .env 不存在"
        print_color "yellow" "请复制 env.example 为 .env 并配置相应的值"
        
        read -p "是否现在复制示例文件? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            if [ -f "env.example" ]; then
                cp env.example .env
                print_color "green" "✓ 已复制环境变量示例文件"
            elif [ -f "doc/env.example" ]; then
                cp doc/env.example .env
                print_color "green" "✓ 已复制环境变量示例文件"
            else
                print_color "red" "✗ 找不到环境变量示例文件"
                exit 1
            fi
            print_color "yellow" "请编辑 .env 文件配置相应的值"
            exit 0
        else
            exit 1
        fi
    else
        print_color "green" "✓ 环境变量文件存在"
    fi
}

# 停止并删除现有容器
stop_existing_containers() {
    print_color "yellow" "🔄 停止现有容器..."
    
    if docker-compose down --remove-orphans; then
        print_color "green" "✓ 现有容器已停止"
    else
        print_color "yellow" "⚠ 停止容器时出现警告，继续执行..."
    fi
}

# 拉取最新镜像
pull_latest_images() {
    print_color "yellow" "🔄 拉取最新镜像..."
    
    if docker-compose pull; then
        print_color "green" "✓ 镜像拉取完成"
    else
        print_color "yellow" "⚠ 镜像拉取失败，使用本地镜像"
    fi
}

# 构建镜像
build_images() {
    print_color "yellow" "🔄 构建应用镜像..."
    
    if docker-compose build --no-cache; then
        print_color "green" "✓ 镜像构建完成"
    else
        print_color "red" "✗ 镜像构建失败"
        exit 1
    fi
}

# 启动服务
start_services() {
    print_color "yellow" "🔄 启动服务..."
    
    if [ "$ENVIRONMENT" = "development" ]; then
        if [ -f "docker-compose.dev.yml" ]; then
            docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
        else
            docker-compose up -d
        fi
    else
        docker-compose up -d
    fi
    
    if [ $? -eq 0 ]; then
        print_color "green" "✓ 服务启动完成"
    else
        print_color "red" "✗ 服务启动失败"
        exit 1
    fi
}

# 等待服务就绪
wait_for_services() {
    print_color "yellow" "🔄 等待服务就绪..."
    
    local max_attempts=30
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        attempt=$((attempt + 1))
        sleep 2
        
        if curl -f -s http://localhost:8080/health > /dev/null 2>&1; then
            print_color "green" "✓ 服务已就绪"
            return
        fi
        
        printf "."
    done
    
    print_color "yellow" "⚠ 服务启动超时，请检查日志"
}

# 显示服务状态
show_service_status() {
    print_color "cyan" "\n📊 服务状态:"
    docker-compose ps
    
    print_color "cyan" "\n📋 服务信息:"
    print_color "white" "API 服务: http://localhost:8080"
    print_color "white" "健康检查: http://localhost:8080/health"
    print_color "white" "Swagger 文档: http://localhost:8080/swagger"
    print_color "white" "数据库: localhost:5432"
    print_color "white" "Redis: localhost:6379"
}

# 显示日志
show_logs() {
    echo
    read -p "是否查看服务日志? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_color "cyan" "\n📋 服务日志 (按 Ctrl+C 退出):"
        docker-compose logs -f
    fi
}

# 主函数
main() {
    print_color "cyan" "🚀 林龍香大米商城后端服务部署脚本"
    print_color "white" "部署环境: $ENVIRONMENT"
    print_color "cyan" "==========================================="
    
    # 预检查
    check_docker
    check_docker_compose
    check_environment_file
    
    # 停止现有容器
    if [ "$FORCE" = true ]; then
        stop_existing_containers
    fi
    
    # 拉取最新镜像
    if [ "$PULL" = true ]; then
        pull_latest_images
    fi
    
    # 构建镜像
    if [ "$BUILD" = true ]; then
        build_images
    fi
    
    # 启动服务
    start_services
    
    # 等待服务就绪
    wait_for_services
    
    # 显示状态
    show_service_status
    
    # 显示日志
    show_logs
    
    print_color "green" "\n🎉 部署完成!"
}

# 执行主函数
main
