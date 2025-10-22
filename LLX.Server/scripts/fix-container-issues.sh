#!/bin/bash

# 林龍香大米商城 - 容器运行问题修复脚本
# 解决日志权限和网络连接问题

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
        "gray") echo -e "\033[90m$message\033[0m" ;;
        *) echo "$message" ;;
    esac
}

# 显示帮助信息
show_help() {
    cat << EOF
林龍香大米商城 - 容器运行问题修复脚本

用法: ./fix-container-issues.sh [选项]

选项:
  -h, --help              显示此帮助信息
  -f, --force             强制修复（不询问确认）
  -n, --no-rebuild        不重新构建镜像
  -r, --restart-only      仅重启服务

修复的问题:
  1. 日志文件权限问题
  2. 数据库连接问题（localhost -> host.docker.internal）
  3. Redis连接问题
  4. 容器网络配置

示例:
  ./fix-container-issues.sh                    # 标准修复
  ./fix-container-issues.sh -f                 # 强制修复
  ./fix-container-issues.sh -n                 # 不重新构建镜像
  ./fix-container-issues.sh -r                 # 仅重启服务

EOF
}

# 默认参数
FORCE=false
NO_REBUILD=false
RESTART_ONLY=false
HELP=false

# 解析命令行参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            HELP=true
            shift
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        -n|--no-rebuild)
            NO_REBUILD=true
            shift
            ;;
        -r|--restart-only)
            RESTART_ONLY=true
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

# 创建日志目录并设置权限
fix_logs_permissions() {
    print_color "cyan" "修复日志目录权限..."
    
    # 创建日志目录
    mkdir -p logs
    print_color "gray" "  创建日志目录: logs/"
    
    # 设置权限
    chmod 755 logs
    print_color "gray" "  设置目录权限: 755"
    
    # 设置所有权（尝试不同的方法）
    if command -v sudo &> /dev/null; then
        sudo chown -R $USER:$USER logs 2>/dev/null || true
    else
        chown -R $USER:$USER logs 2>/dev/null || true
    fi
    print_color "gray" "  设置目录所有权"
    
    print_color "green" "✓ 日志目录权限修复完成"
}

# 检查并修复环境变量配置
fix_environment_config() {
    print_color "cyan" "检查环境变量配置..."
    
    # 检查.env文件是否存在
    if [ ! -f ".env" ]; then
        print_color "yellow" "⚠ .env 文件不存在"
        if [ -f "env.api-only.example" ]; then
            print_color "gray" "  复制示例文件..."
            cp env.api-only.example .env
            print_color "green" "✓ 已创建 .env 文件"
        else
            print_color "red" "✗ 找不到环境变量示例文件"
            return 1
        fi
    else
        print_color "green" "✓ .env 文件存在"
    fi
    
    # 检查localhost配置
    if grep -q "localhost" .env; then
        print_color "yellow" "⚠ 发现localhost配置，需要修改为容器网络地址"
        
        if [ "$FORCE" = false ]; then
            read -p "是否自动修复localhost配置？(y/n): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                # 备份原文件
                cp .env .env.backup
                print_color "gray" "  备份原配置: .env.backup"
                
                # 替换localhost为host.docker.internal
                sed -i 's/Host=localhost/Host=host.docker.internal/g' .env
                sed -i 's/localhost:6379/host.docker.internal:6379/g' .env
                
                print_color "green" "✓ 已修复localhost配置"
                print_color "yellow" "⚠ 请检查并确认数据库和Redis连接信息"
            else
                print_color "yellow" "⚠ 请手动修改 .env 文件中的localhost配置"
                print_color "white" "  建议修改为:"
                print_color "white" "    DB_CONNECTION_STRING=Host=host.docker.internal;Port=5432;..."
                print_color "white" "    REDIS_CONNECTION_STRING=host.docker.internal:6379,..."
            fi
        else
            # 强制修复
            cp .env .env.backup
            sed -i 's/Host=localhost/Host=host.docker.internal/g' .env
            sed -i 's/localhost:6379/host.docker.internal:6379/g' .env
            print_color "green" "✓ 已强制修复localhost配置"
        fi
    else
        print_color "green" "✓ 环境变量配置正常"
    fi
}

# 重新构建Docker镜像
rebuild_docker_image() {
    if [ "$NO_REBUILD" = true ]; then
        print_color "yellow" "跳过Docker镜像构建"
        return 0
    fi
    
    print_color "cyan" "重新构建Docker镜像..."
    
    # 编译.NET项目
    print_color "gray" "  编译.NET项目..."
    cd LLX.Server
    dotnet clean --configuration Release > /dev/null 2>&1
    if dotnet build --configuration Release; then
        print_color "green" "  ✓ .NET项目编译成功"
    else
        print_color "red" "  ✗ .NET项目编译失败"
        cd ..
        return 1
    fi
    cd ..
    
    # 构建Docker镜像
    print_color "gray" "  构建Docker镜像..."
    if docker build -f LLX.Server/Dockerfile -t llxrice/api:latest .; then
        print_color "green" "  ✓ Docker镜像构建成功"
    else
        print_color "red" "  ✗ Docker镜像构建失败"
        return 1
    fi
}

# 停止现有容器
stop_existing_container() {
    print_color "cyan" "停止现有容器..."
    
    if docker ps -a --filter "name=llxrice_api" --format "{{.Names}}" | grep -q "llxrice_api"; then
        docker stop llxrice_api
        docker rm llxrice_api
        print_color "green" "✓ 已停止并删除现有容器"
    else
        print_color "gray" "  没有找到现有容器"
    fi
}

# 启动服务
start_service() {
    print_color "cyan" "启动服务..."
    
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

# 验证修复
verify_fix() {
    print_color "cyan" "验证修复结果..."
    
    # 检查容器状态
    if docker ps --filter "name=llxrice_api" --format "{{.Names}}" | grep -q "llxrice_api"; then
        print_color "green" "✓ 容器运行正常"
    else
        print_color "red" "✗ 容器未运行"
        return 1
    fi
    
    # 检查日志权限
    if docker exec llxrice_api ls -la /app/logs > /dev/null 2>&1; then
        print_color "green" "✓ 日志目录权限正常"
    else
        print_color "yellow" "⚠ 日志目录权限可能仍有问题"
    fi
    
    # 检查健康状态
    local port=8080
    if [ -f ".env" ]; then
        local env_port=$(grep "^API_PORT=" .env | cut -d '=' -f2)
        if [ -n "$env_port" ]; then
            port=$env_port
        fi
    fi
    
    if curl -f -s "http://localhost:$port/health" > /dev/null 2>&1; then
        print_color "green" "✓ 健康检查通过"
    else
        print_color "yellow" "⚠ 健康检查失败，请检查数据库和Redis连接"
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
    print_color "white" "  进入容器:     docker exec -it llxrice_api bash"
}

# 主函数
main() {
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "cyan" "🔧 林龍香大米商城 - 容器问题修复"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
    
    # 检查环境
    check_project_directory || exit 1
    check_docker || exit 1
    
    # 确认修复
    if [ "$FORCE" = false ] && [ "$RESTART_ONLY" = false ]; then
        print_color "yellow" "即将修复以下问题:"
        print_color "white" "  1. 日志文件权限问题"
        print_color "white" "  2. 数据库连接问题（localhost -> host.docker.internal）"
        print_color "white" "  3. Redis连接问题"
        print_color "white" "  4. 容器网络配置"
        echo
        read -p "是否继续？(y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_color "red" "✗ 用户取消操作"
            exit 0
        fi
    fi
    
    # 修复日志权限
    fix_logs_permissions
    
    # 修复环境配置
    fix_environment_config
    
    # 重新构建镜像（如果需要）
    if [ "$RESTART_ONLY" = false ]; then
        rebuild_docker_image || exit 1
    fi
    
    # 停止现有容器
    stop_existing_container
    
    # 启动服务
    start_service || exit 1
    
    # 等待服务就绪
    wait_for_service
    
    # 验证修复
    verify_fix
    
    # 显示状态
    show_status
    
    print_color "green" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "green" "🎉 容器问题修复完成！"
    print_color "green" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    print_color "yellow" "\n⚠ 如果仍有问题，请检查:"
    print_color "white" "  1. 数据库和Redis服务是否运行"
    print_color "white" "  2. 网络连接是否正常"
    print_color "white" "  3. 环境变量配置是否正确"
    print_color "white" "  4. 查看详细日志: docker logs llxrice_api"
}

# 执行主函数
main "$@"
