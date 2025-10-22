#!/bin/bash

# 林龍香大米商城 - API服务独立部署脚本
# Linux/Mac 版本
# 说明：此脚本仅部署API服务，不部署数据库和Redis

set -e

# 默认参数
ENVIRONMENT="production"
BUILD=false
NO_BUILD=false
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
        "white") echo -e "\033[37m$message\033[0m" ;;
        "gray") echo -e "\033[90m$message\033[0m" ;;
        *) echo "$message" ;;
    esac
}

# 显示帮助信息
show_help() {
    cat << EOF
林龍香大米商城 - API服务独立部署脚本

用法: ./deploy-api-only.sh [选项]

选项:
  -e, --environment <env>  部署环境 (development/production) [默认: production]
  -b, --build             强制重新构建Docker镜像
  -n, --no-build          使用已有的构建文件，不重新编译.NET项目
  -f, --force             强制重新部署（停止并删除现有容器）
  -h, --help              显示此帮助信息

示例:
  ./deploy-api-only.sh                      # 标准部署
  ./deploy-api-only.sh -b                   # 重新构建镜像并部署
  ./deploy-api-only.sh -n                   # 使用已构建的文件部署
  ./deploy-api-only.sh -f                   # 强制重新部署

前提条件:
  1. PostgreSQL数据库已运行并可访问
  2. Redis缓存已运行并可访问
  3. .env文件已正确配置数据库和Redis连接信息

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
        -n|--no-build)
            NO_BUILD=true
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
        print_color "red" "✗ Docker 未安装"
        print_color "yellow" "请先安装 Docker: https://docs.docker.com/engine/install/"
        return 1
    fi
    print_color "green" "✓ Docker 已安装"
    return 0
}

# 检查Docker服务是否运行
check_docker_service() {
    if ! docker ps &> /dev/null; then
        print_color "red" "✗ Docker 服务未运行"
        print_color "yellow" "请启动 Docker 服务: sudo systemctl start docker"
        return 1
    fi
    print_color "green" "✓ Docker 服务正在运行"
    return 0
}

# 检查Docker Compose是否安装
check_docker_compose() {
    if ! command -v docker-compose &> /dev/null; then
        print_color "red" "✗ Docker Compose 未安装"
        print_color "yellow" "请先安装 Docker Compose"
        return 1
    fi
    print_color "green" "✓ Docker Compose 已安装"
    return 0
}

# 检查是否在正确的目录
check_project_directory() {
    if [ ! -f "LLX.Server.sln" ]; then
        print_color "red" "✗ 不在项目根目录"
        print_color "yellow" "请切换到项目根目录运行此脚本"
        return 1
    fi
    print_color "green" "✓ 在项目根目录"
    return 0
}

# 检查环境变量文件
check_environment_file() {
    if [ ! -f ".env" ]; then
        print_color "yellow" "⚠ 环境变量文件 .env 不存在"
        
        if [ -f "env.api-only.example" ]; then
            print_color "white" "发现 env.api-only.example 示例文件"
            read -p "是否复制示例文件为 .env？(y/n): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                cp env.api-only.example .env
                print_color "green" "✓ 已创建 .env 文件"
                print_color "yellow" "⚠ 请立即编辑 .env 文件，配置数据库和Redis连接信息"
                print_color "yellow" "配置完成后，请重新运行此脚本"
                
                # 显示需要配置的关键项
                print_color "cyan" "\n需要配置的关键项:"
                print_color "white" "  1. DB_CONNECTION_STRING - PostgreSQL数据库连接字符串"
                print_color "white" "  2. REDIS_CONNECTION_STRING - Redis缓存连接字符串"
                
                return 1
            else
                print_color "red" "✗ 需要 .env 文件才能继续部署"
                return 1
            fi
        else
            print_color "red" "✗ 找不到环境变量示例文件"
            return 1
        fi
    fi
    
    print_color "green" "✓ 环境变量文件存在"
    return 0
}

# 验证环境变量配置
validate_environment_variables() {
    print_color "cyan" "\n验证环境变量配置..."
    
    local has_db_connection=false
    local has_redis_connection=false
    
    # 读取.env文件
    while IFS= read -r line; do
        # 跳过注释和空行
        [[ "$line" =~ ^#.*$ ]] && continue
        [[ -z "$line" ]] && continue
        
        if [[ "$line" =~ ^DB_CONNECTION_STRING=(.+)$ ]]; then
            local value="${BASH_REMATCH[1]}"
            if [[ -n "$value" && "$value" != "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_strong_password_here;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Command Timeout=30" ]]; then
                has_db_connection=true
                print_color "green" "  ✓ 数据库连接配置已设置"
            fi
        fi
        
        if [[ "$line" =~ ^REDIS_CONNECTION_STRING=(.+)$ ]]; then
            local value="${BASH_REMATCH[1]}"
            if [[ -n "$value" && "$value" != "localhost:6379,password=your_redis_password_here,ssl=false,abortConnect=false" ]]; then
                has_redis_connection=true
                print_color "green" "  ✓ Redis连接配置已设置"
            fi
        fi
    done < ".env"
    
    if [ "$has_db_connection" = false ]; then
        print_color "yellow" "  ⚠ 数据库连接未配置或使用默认值"
        print_color "yellow" "  请在 .env 文件中配置 DB_CONNECTION_STRING"
    fi
    
    if [ "$has_redis_connection" = false ]; then
        print_color "yellow" "  ⚠ Redis连接未配置或使用默认值"
        print_color "yellow" "  请在 .env 文件中配置 REDIS_CONNECTION_STRING"
    fi
    
    if [ "$has_db_connection" = false ] || [ "$has_redis_connection" = false ]; then
        read -p "\n是否继续部署？(y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            return 1
        fi
    fi
    
    return 0
}

# 测试数据库连接
test_database_connection() {
    print_color "cyan" "\n测试数据库连接..."
    print_color "yellow" "  ⚠ 跳过数据库连接测试"
    print_color "yellow" "  请确保PostgreSQL数据库可访问"
}

# 测试Redis连接
test_redis_connection() {
    print_color "cyan" "\n测试Redis连接..."
    print_color "yellow" "  ⚠ 跳过Redis连接测试"
    print_color "yellow" "  请确保Redis服务可访问"
}

# 停止并删除现有容器
stop_existing_container() {
    print_color "yellow" "\n停止现有API容器..."
    
    if docker ps -a --filter "name=llxrice_api" --format "{{.Names}}" | grep -q "llxrice_api"; then
        docker stop llxrice_api 2>/dev/null || true
        docker rm llxrice_api 2>/dev/null || true
        print_color "green" "✓ 已停止并删除现有容器"
    else
        print_color "gray" "  没有找到现有容器"
    fi
}

# 构建.NET项目
build_dotnet_project() {
    if [ "$NO_BUILD" = true ]; then
        print_color "yellow" "\n跳过.NET项目编译（使用已有构建）..."
        return 0
    fi
    
    print_color "cyan" "\n编译.NET项目..."
    
    cd LLX.Server
    
    # 清理
    print_color "gray" "  清理旧文件..."
    dotnet clean --configuration Release > /dev/null 2>&1
    
    # 编译
    print_color "gray" "  编译项目..."
    if dotnet build --configuration Release; then
        print_color "green" "✓ 项目编译成功"
        cd ..
        return 0
    else
        print_color "red" "✗ 项目编译失败"
        cd ..
        return 1
    fi
}

# 构建Docker镜像
build_docker_image() {
    print_color "cyan" "\n构建Docker镜像..."
    
    if [ "$BUILD" = true ]; then
        print_color "yellow" "  强制重新构建镜像..."
        if docker build -f LLX.Server/Dockerfile -t llxrice/api:latest --no-cache .; then
            print_color "green" "✓ Docker镜像构建成功"
            return 0
        else
            print_color "red" "✗ Docker镜像构建失败"
            return 1
        fi
    else
        if docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .; then
            print_color "green" "✓ Docker镜像构建成功"
            return 0
        else
            print_color "red" "✗ Docker镜像构建失败"
            return 1
        fi
    fi
}

# 启动API服务
start_api_service() {
    print_color "cyan" "\n启动API服务..."
    
    if [ ! -f "docker-compose.api-only.yml" ]; then
        print_color "red" "✗ 找不到 docker-compose.api-only.yml 文件"
        return 1
    fi
    
    if docker-compose -f docker-compose.api-only.yml up -d; then
        print_color "green" "✓ API服务启动成功"
        return 0
    else
        print_color "red" "✗ API服务启动失败"
        return 1
    fi
}

# 等待服务就绪
wait_for_service() {
    print_color "yellow" "\n等待服务就绪..."
    
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
            print_color "green" "\n✓ 服务已就绪！"
            return 0
        fi
        
        printf "."
    done
    
    print_color "yellow" "\n⚠ 服务启动超时"
    print_color "white" "可以运行以下命令查看日志:"
    print_color "cyan" "  docker logs llxrice_api"
    return 1
}

# 显示服务状态
show_service_status() {
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
    print_color "white" "  启动服务:     docker start llxrice_api"
    print_color "white" "  重启服务:     docker restart llxrice_api"
    print_color "white" "  删除容器:     docker rm -f llxrice_api"
}

# 显示日志选项
show_logs_option() {
    print_color "cyan" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    read -p "是否查看实时日志？(y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_color "cyan" "\n📋 实时日志 (按 Ctrl+C 退出)"
        print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        docker logs -f llxrice_api
    fi
}

# 主函数
main() {
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "cyan" "🚀 林龍香大米商城 - API服务独立部署"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "white" "部署环境: $ENVIRONMENT"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
    
    # 1. 环境检查
    print_color "cyan" "【步骤 1/8】环境检查"
    check_docker || exit 1
    check_docker_service || exit 1
    check_docker_compose || exit 1
    check_project_directory || exit 1
    
    # 2. 配置检查
    print_color "cyan" "\n【步骤 2/8】配置检查"
    check_environment_file || exit 1
    validate_environment_variables || exit 1
    
    # 3. 连接测试（可选）
    print_color "cyan" "\n【步骤 3/8】连接测试"
    test_database_connection
    test_redis_connection
    
    # 4. 停止现有容器
    print_color "cyan" "\n【步骤 4/8】停止现有容器"
    if [ "$FORCE" = true ]; then
        stop_existing_container
    else
        if docker ps -a --filter "name=llxrice_api" --format "{{.Names}}" | grep -q "llxrice_api"; then
            print_color "yellow" "  发现现有容器: llxrice_api"
            read -p "是否停止并删除？(y/n): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                stop_existing_container
            fi
        fi
    fi
    
    # 5. 编译.NET项目
    print_color "cyan" "\n【步骤 5/8】编译项目"
    build_dotnet_project || exit 1
    
    # 6. 构建Docker镜像
    print_color "cyan" "\n【步骤 6/8】构建Docker镜像"
    build_docker_image || exit 1
    
    # 7. 启动服务
    print_color "cyan" "\n【步骤 7/8】启动API服务"
    start_api_service || exit 1
    
    # 8. 等待服务就绪
    print_color "cyan" "\n【步骤 8/8】验证服务"
    wait_for_service
    
    # 显示状态
    show_service_status
    
    # 显示日志选项
    show_logs_option
    
    print_color "green" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "green" "🎉 API服务部署完成！"
    print_color "green" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
}

# 执行主函数
main
