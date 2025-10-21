#!/bin/bash

# 数据库切换脚本
# 使用方法: ./switch-database.sh PostgreSQL "your_connection_string"

set -e

# 检查参数
if [ $# -lt 2 ]; then
    echo "使用方法: $0 <数据库类型> <连接字符串> [配置文件]"
    echo "支持的数据库类型: PostgreSQL, SqlServer, MySql, Sqlite"
    echo "示例: $0 PostgreSQL 'Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass'"
    exit 1
fi

DATABASE_TYPE=$1
CONNECTION_STRING=$2
CONFIG_FILE=${3:-"appsettings.json"}

# 验证数据库类型
case $DATABASE_TYPE in
    PostgreSQL|SqlServer|MySql|Sqlite)
        ;;
    *)
        echo "错误: 不支持的数据库类型 '$DATABASE_TYPE'"
        echo "支持的数据库类型: PostgreSQL, SqlServer, MySql, Sqlite"
        exit 1
        ;;
esac

echo "正在切换到 $DATABASE_TYPE 数据库..."

# 检查配置文件是否存在
if [ ! -f "$CONFIG_FILE" ]; then
    echo "错误: 配置文件 '$CONFIG_FILE' 不存在"
    exit 1
fi

# 创建备份
cp "$CONFIG_FILE" "${CONFIG_FILE}.backup.$(date +%Y%m%d_%H%M%S)"
echo "已创建配置文件备份"

# 使用 jq 更新配置文件
if command -v jq &> /dev/null; then
    # 更新连接字符串
    jq --arg conn "$CONNECTION_STRING" '.ConnectionStrings.DefaultConnection = $conn' "$CONFIG_FILE" > temp.json
    mv temp.json "$CONFIG_FILE"
    
    # 更新数据库提供程序
    jq --arg provider "$DATABASE_TYPE" '.Database.Provider = $provider' "$CONFIG_FILE" > temp.json
    mv temp.json "$CONFIG_FILE"
    
    echo "配置文件已更新: $CONFIG_FILE"
    echo "数据库类型: $DATABASE_TYPE"
    echo "连接字符串: $CONNECTION_STRING"
else
    echo "警告: 未找到 jq 命令，请手动更新配置文件"
    echo "需要设置:"
    echo "  ConnectionStrings.DefaultConnection = '$CONNECTION_STRING'"
    echo "  Database.Provider = '$DATABASE_TYPE'"
fi

echo ""
echo "请运行以下命令来应用数据库迁移:"
echo "dotnet ef migrations add InitialCreate"
echo "dotnet ef database update"
echo ""
echo "数据库切换完成!"
