#!/bin/bash

# 林龍香大米商城 - 权限设置脚本
# 为所有脚本文件设置正确的执行权限

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

# 设置脚本权限
set_script_permissions() {
    print_color "cyan" "设置脚本执行权限..."
    
    local scripts_dir="LLX.Server/scripts"
    
    if [ ! -d "$scripts_dir" ]; then
        print_color "red" "✗ 脚本目录不存在: $scripts_dir"
        return 1
    fi
    
    # 查找所有.sh文件并设置权限
    local sh_files=$(find "$scripts_dir" -name "*.sh" -type f)
    
    if [ -z "$sh_files" ]; then
        print_color "yellow" "⚠ 没有找到.sh脚本文件"
        return 0
    fi
    
    print_color "gray" "找到以下脚本文件:"
    echo "$sh_files" | while read -r file; do
        print_color "white" "  $file"
    done
    
    # 设置执行权限
    echo "$sh_files" | while read -r file; do
        chmod +x "$file"
        print_color "green" "✓ 已设置权限: $file"
    done
    
    print_color "green" "✓ 所有脚本权限设置完成"
}

# 验证权限设置
verify_permissions() {
    print_color "cyan" "验证权限设置..."
    
    local scripts_dir="LLX.Server/scripts"
    local sh_files=$(find "$scripts_dir" -name "*.sh" -type f)
    
    echo "$sh_files" | while read -r file; do
        if [ -x "$file" ]; then
            print_color "green" "✓ $file (可执行)"
        else
            print_color "red" "✗ $file (不可执行)"
        fi
    done
}

# 显示使用说明
show_usage() {
    print_color "cyan" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "cyan" "📋 脚本使用说明"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    print_color "white" "现在您可以运行以下脚本:"
    print_color "white" ""
    print_color "white" "1. 修复容器问题:"
    print_color "cyan" "   ./LLX.Server/scripts/fix-container-issues.sh -f"
    print_color "white" ""
    print_color "white" "2. 部署API服务:"
    print_color "cyan" "   ./LLX.Server/scripts/deploy-api-only.sh"
    print_color "white" ""
    print_color "white" "3. 更新API服务:"
    print_color "cyan" "   ./LLX.Server/scripts/update-api.sh -b"
    print_color "white" ""
    print_color "white" "4. 查看帮助信息:"
    print_color "cyan" "   ./LLX.Server/scripts/fix-container-issues.sh -h"
    print_color "cyan" "   ./LLX.Server/scripts/deploy-api-only.sh -h"
    print_color "cyan" "   ./LLX.Server/scripts/update-api.sh -h"
}

# 主函数
main() {
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "cyan" "🔧 林龍香大米商城 - 权限设置"
    print_color "cyan" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
    
    # 检查环境
    check_project_directory || exit 1
    
    # 设置权限
    set_script_permissions || exit 1
    
    # 验证权限
    verify_permissions
    
    # 显示使用说明
    show_usage
    
    print_color "green" "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    print_color "green" "🎉 权限设置完成！"
    print_color "green" "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
}

# 执行主函数
main "$@"
