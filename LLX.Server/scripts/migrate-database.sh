#!/bin/bash

# 数据库迁移脚本
# 使用方法: ./migrate-database.sh PostgreSQL "your_connection_string" [migration_name]

set -e

# 检查参数
if [ $# -lt 2 ]; then
    echo "使用方法: $0 <数据库类型> <连接字符串> [迁移名称]"
    echo "支持的数据库类型: PostgreSQL, SqlServer, MySql, Sqlite"
    echo "示例: $0 PostgreSQL 'Host=localhost;Port=5432;Database=llxrice;Username=user;Password=pass' InitialCreate"
    exit 1
fi

DATABASE_TYPE=$1
CONNECTION_STRING=$2
MIGRATION_NAME=${3:-"InitialCreate"}

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

echo "正在为 $DATABASE_TYPE 数据库创建迁移..."

# 设置环境变量
export ASPNETCORE_ENVIRONMENT=Development

# 创建迁移
echo "创建迁移: $MIGRATION_NAME"
dotnet ef migrations add $MIGRATION_NAME --context AppDbContext

if [ $? -eq 0 ]; then
    echo "迁移创建成功!"
    
    # 应用迁移
    echo "应用迁移到数据库..."
    dotnet ef database update --context AppDbContext
    
    if [ $? -eq 0 ]; then
        echo "数据库迁移完成!"
    else
        echo "数据库迁移失败!"
        exit 1
    fi
else
    echo "迁移创建失败!"
    exit 1
fi
