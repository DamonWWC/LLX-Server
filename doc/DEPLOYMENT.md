# 林龍香大米商城 - 后端服务部署指南

## 📋 目录

1. [环境要求](#环境要求)
2. [Docker 部署（推荐）](#docker-部署推荐)
3. [Systemd 服务部署](#systemd-服务部署)
4. [配置说明](#配置说明)
5. [健康检查](#健康检查)
6. [监控和日志](#监控和日志)
7. [备份和恢复](#备份和恢复)
8. [故障排除](#故障排除)

---

## 环境要求

### 硬件要求

**最低配置**:
- CPU: 1 核
- 内存: 1GB
- 磁盘: 10GB

**推荐配置**:
- CPU: 2 核+
- 内存: 2GB+
- 磁盘: 20GB+ (SSD)

### 软件要求

#### Docker 部署
- Docker 20.10+
- Docker Compose 2.0+
- Linux 操作系统（Ubuntu 20.04/22.04, CentOS 7/8, Debian 11+）

#### Systemd 服务部署
- Linux 操作系统
- .NET 8 Runtime
- PostgreSQL 16
- Redis 7.2
- Nginx (可选，用于反向代理)

---

## Docker 部署（推荐）

### 优势

✅ **环境一致**: 开发、测试、生产环境完全一致  
✅ **快速部署**: 一条命令启动所有服务  
✅ **易于维护**: 容器化管理，升级回滚方便  
✅ **资源隔离**: 服务间互不影响

### 1. 安装 Docker

#### Ubuntu/Debian

```bash
# 更新包索引
sudo apt update

# 安装依赖
sudo apt install -y ca-certificates curl gnupg lsb-release

# 添加 Docker 官方 GPG 密钥
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# 添加 Docker 仓库
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# 安装 Docker Engine
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 启动 Docker
sudo systemctl start docker
sudo systemctl enable docker

# 验证安装
docker --version
docker compose version
```

#### CentOS/RHEL

```bash
# 安装依赖
sudo yum install -y yum-utils

# 添加 Docker 仓库
sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo

# 安装 Docker
sudo yum install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# 启动 Docker
sudo systemctl start docker
sudo systemctl enable docker

# 验证安装
docker --version
docker compose version
```

### 2. 准备项目文件

```bash
# 克隆或上传项目代码
git clone <repository-url> /opt/llxrice-api
cd /opt/llxrice-api

# 或者使用 scp 上传
# scp -r ./LLXRice.Api user@server:/opt/llxrice-api
```

### 3. 配置环境变量

创建 `.env` 文件:

```bash
cd /opt/llxrice-api

# 创建 .env 文件
cat > .env << 'EOF'
# 数据库密码
DB_PASSWORD=your_strong_db_password_change_me

# Redis 密码
REDIS_PASSWORD=your_strong_redis_password_change_me

# 环境配置
ASPNETCORE_ENVIRONMENT=Production
EOF

# 设置文件权限
chmod 600 .env
```

**⚠️ 安全提示**: 
- 请务必修改默认密码
- 使用强密码（至少 16 位，包含大小写字母、数字、特殊字符）
- 不要将 `.env` 文件提交到 Git 仓库

### 4. 创建必要的目录

```bash
# 创建日志目录
mkdir -p ./logs

# 创建数据卷目录（可选，用于数据持久化）
mkdir -p ./data/postgres
mkdir -p ./data/redis
```

### 5. 启动服务

```bash
# 构建并启动所有服务
docker compose up -d

# 查看服务状态
docker compose ps

# 查看日志
docker compose logs -f

# 只查看 API 服务日志
docker compose logs -f api
```

### 6. 验证部署

```bash
# 检查服务健康状态
curl http://localhost:8080/health

# 访问 Swagger 文档
curl http://localhost:8080/swagger/index.html

# 或在浏览器中访问
# http://<server-ip>:8080
```

### 7. 常用管理命令

```bash
# 停止所有服务
docker compose down

# 停止并删除数据卷（危险操作，会删除数据）
docker compose down -v

# 重启所有服务
docker compose restart

# 重启单个服务
docker compose restart api

# 查看服务日志
docker compose logs -f api

# 进入容器
docker exec -it llxrice_api bash

# 查看容器资源使用情况
docker stats

# 更新服务（拉取最新代码后）
docker compose up -d --build

# 备份数据库
docker exec llxrice_db pg_dump -U llxrice_user llxrice > backup_$(date +%Y%m%d).sql

# 恢复数据库
docker exec -i llxrice_db psql -U llxrice_user llxrice < backup_20251017.sql
```

---

## Systemd 服务部署

### 1. 安装运行时环境

#### 安装 .NET 8 Runtime

```bash
# Ubuntu 22.04
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt update
sudo apt install -y aspnetcore-runtime-8.0

# 验证安装
dotnet --version
```

#### 安装 PostgreSQL 16

```bash
# 添加 PostgreSQL 仓库
sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -

# 安装 PostgreSQL 16
sudo apt update
sudo apt install -y postgresql-16 postgresql-contrib-16

# 启动服务
sudo systemctl start postgresql
sudo systemctl enable postgresql

# 验证安装
sudo -u postgres psql --version
```

#### 安装 Redis

```bash
# 安装 Redis
sudo apt install -y redis-server

# 配置 Redis 密码
sudo nano /etc/redis/redis.conf
# 找到 # requirepass foobared
# 修改为 requirepass your_strong_redis_password

# 重启 Redis
sudo systemctl restart redis-server
sudo systemctl enable redis-server

# 验证安装
redis-cli ping
# 输出: PONG
```

### 2. 配置数据库

```bash
# 切换到 postgres 用户
sudo -u postgres psql

# 在 PostgreSQL 中执行
CREATE DATABASE llxrice WITH ENCODING 'UTF8';
CREATE USER llxrice_user WITH PASSWORD 'your_strong_db_password';
GRANT ALL PRIVILEGES ON DATABASE llxrice TO llxrice_user;
\q

# 初始化数据库
sudo -u postgres psql -d llxrice -f /opt/llxrice-api/scripts/数据库初始化脚本.sql
```

### 3. 发布应用

```bash
# 在开发机器上发布
cd /path/to/LLXRice.Api/src/LLXRice.Api
dotnet publish -c Release -o ./publish

# 上传到服务器
scp -r ./publish user@server:/var/www/llxrice-api

# 在服务器上设置权限
sudo chown -R www-data:www-data /var/www/llxrice-api
sudo chmod -R 755 /var/www/llxrice-api

# 创建日志目录
sudo mkdir -p /var/www/llxrice-api/logs
sudo chown -R www-data:www-data /var/www/llxrice-api/logs
```

### 4. 配置应用

创建生产环境配置文件:

```bash
sudo nano /var/www/llxrice-api/appsettings.Production.json
```

内容:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_strong_db_password",
    "Redis": "localhost:6379,password=your_strong_redis_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### 5. 创建 Systemd 服务

```bash
sudo nano /etc/systemd/system/llxrice-api.service
```

内容:

```ini
[Unit]
Description=LLXRice API Service
After=network.target postgresql.service redis.service
Requires=postgresql.service redis.service

[Service]
Type=notify
User=www-data
Group=www-data
WorkingDirectory=/var/www/llxrice-api
ExecStart=/usr/bin/dotnet /var/www/llxrice-api/LLXRice.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=llxrice-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

# 资源限制
LimitNOFILE=65536
LimitNPROC=4096

# 安全配置
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/www/llxrice-api/logs

[Install]
WantedBy=multi-user.target
```

### 6. 启动服务

```bash
# 重新加载 systemd
sudo systemctl daemon-reload

# 启动服务
sudo systemctl start llxrice-api

# 设置开机自启
sudo systemctl enable llxrice-api

# 查看服务状态
sudo systemctl status llxrice-api

# 查看日志
sudo journalctl -u llxrice-api -f

# 查看最近 100 条日志
sudo journalctl -u llxrice-api -n 100
```

### 7. 配置 Nginx 反向代理

```bash
# 安装 Nginx
sudo apt install -y nginx

# 创建配置文件
sudo nano /etc/nginx/sites-available/llxrice-api
```

Nginx 配置:

```nginx
upstream llxrice_api {
    server 127.0.0.1:8080;
    keepalive 32;
}

server {
    listen 80;
    server_name api.llxrice.com;  # 替换为您的域名

    # 请求大小限制
    client_max_body_size 10M;

    # 日志
    access_log /var/log/nginx/llxrice-api.access.log;
    error_log /var/log/nginx/llxrice-api.error.log;

    # 代理配置
    location / {
        proxy_pass http://llxrice_api;
        proxy_http_version 1.1;
        
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        proxy_cache_bypass $http_upgrade;
        proxy_buffering off;
        
        # 超时设置
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # 健康检查（不记录日志）
    location /health {
        proxy_pass http://llxrice_api/health;
        access_log off;
    }

    # Swagger 文档（生产环境可选择关闭）
    location /swagger {
        proxy_pass http://llxrice_api/swagger;
    }
}
```

```bash
# 启用站点
sudo ln -s /etc/nginx/sites-available/llxrice-api /etc/nginx/sites-enabled/

# 测试配置
sudo nginx -t

# 重启 Nginx
sudo systemctl restart nginx
```

### 8. 配置 SSL（使用 Let's Encrypt）

```bash
# 安装 Certbot
sudo apt install -y certbot python3-certbot-nginx

# 获取证书
sudo certbot --nginx -d api.llxrice.com

# 自动续期（Certbot 会自动添加定时任务）
sudo certbot renew --dry-run
```

---

## 配置说明

### 环境变量

| 变量名 | 说明 | 默认值 | 必填 |
|--------|------|--------|------|
| `ASPNETCORE_ENVIRONMENT` | 环境名称 | Production | 否 |
| `ASPNETCORE_URLS` | 监听地址 | http://+:8080 | 否 |
| `ConnectionStrings__DefaultConnection` | PostgreSQL 连接串 | - | 是 |
| `ConnectionStrings__Redis` | Redis 连接串 | - | 是 |

### 连接字符串格式

#### PostgreSQL

```
Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=your_password
```

参数说明:
- `Host`: 数据库主机地址
- `Port`: 端口号（默认 5432）
- `Database`: 数据库名称
- `Username`: 用户名
- `Password`: 密码
- `Pooling`: 启用连接池（可选，默认 true）
- `Minimum Pool Size`: 最小连接数（可选，默认 0）
- `Maximum Pool Size`: 最大连接数（可选，默认 100）

#### Redis

```
localhost:6379,password=your_password,ssl=false,abortConnect=false
```

参数说明:
- `localhost:6379`: 服务器地址和端口
- `password`: 密码（如果没有设置密码则省略）
- `ssl`: 是否使用 SSL（默认 false）
- `abortConnect`: 连接失败时是否抛出异常（默认 true）
- `connectTimeout`: 连接超时时间（毫秒，默认 5000）
- `syncTimeout`: 同步操作超时时间（毫秒，默认 5000）

---

## 健康检查

### 健康检查端点

```bash
# 检查服务健康状态
curl http://localhost:8080/health
```

响应:

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "PostgreSQL": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "Redis": {
      "status": "Healthy",
      "duration": "00:00:00.0098765"
    }
  }
}
```

### 监控脚本

创建监控脚本 `/opt/scripts/health-check.sh`:

```bash
#!/bin/bash

URL="http://localhost:8080/health"
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" $URL)

if [ $RESPONSE -eq 200 ]; then
    echo "$(date): Service is healthy"
    exit 0
else
    echo "$(date): Service is unhealthy (HTTP $RESPONSE)"
    
    # 发送告警（示例：使用邮件）
    # echo "LLXRice API is down!" | mail -s "Service Alert" admin@example.com
    
    # 或者使用企业微信、钉钉等 Webhook
    # curl -X POST https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=xxx \
    #   -d '{"msgtype":"text","text":{"content":"LLXRice API 服务异常！"}}'
    
    exit 1
fi
```

添加到 crontab（每分钟检查一次）:

```bash
# 编辑 crontab
crontab -e

# 添加以下行
* * * * * /opt/scripts/health-check.sh >> /var/log/llxrice-health.log 2>&1
```

---

## 监控和日志

### 日志位置

#### Docker 部署
- **应用日志**: `./logs/log-YYYYMMDD.txt`
- **容器日志**: `docker compose logs api`

#### Systemd 部署
- **应用日志**: `/var/www/llxrice-api/logs/log-YYYYMMDD.txt`
- **系统日志**: `journalctl -u llxrice-api`

### 日志查看

```bash
# 查看最新日志（Docker）
docker compose logs -f api --tail=100

# 查看最新日志（Systemd）
sudo journalctl -u llxrice-api -f -n 100

# 查看应用日志文件
tail -f /var/www/llxrice-api/logs/log-$(date +%Y%m%d).txt

# 查看错误日志
grep "ERROR" /var/www/llxrice-api/logs/log-*.txt

# 查看访问统计
grep "GET" /var/www/llxrice-api/logs/log-*.txt | wc -l
```

### 日志轮转

创建日志轮转配置 `/etc/logrotate.d/llxrice-api`:

```
/var/www/llxrice-api/logs/*.txt {
    daily
    rotate 30
    compress
    delaycompress
    notifempty
    missingok
    create 0644 www-data www-data
}
```

---

## 备份和恢复

### 数据库备份

#### 手动备份

```bash
# Docker 环境
docker exec llxrice_db pg_dump -U llxrice_user llxrice > backup_$(date +%Y%m%d_%H%M%S).sql

# Systemd 环境
sudo -u postgres pg_dump llxrice > backup_$(date +%Y%m%d_%H%M%S).sql
```

#### 自动备份脚本

创建备份脚本 `/opt/scripts/backup-db.sh`:

```bash
#!/bin/bash

BACKUP_DIR="/opt/backups/llxrice"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/llxrice_backup_$DATE.sql"

# 创建备份目录
mkdir -p $BACKUP_DIR

# 执行备份（根据部署方式选择）
# Docker:
docker exec llxrice_db pg_dump -U llxrice_user llxrice > $BACKUP_FILE

# Systemd:
# sudo -u postgres pg_dump llxrice > $BACKUP_FILE

# 压缩备份
gzip $BACKUP_FILE

# 保留最近 7 天的备份
find $BACKUP_DIR -name "*.sql.gz" -mtime +7 -delete

echo "Backup completed: $BACKUP_FILE.gz"
```

添加到 crontab（每天凌晨 2 点备份）:

```bash
0 2 * * * /opt/scripts/backup-db.sh >> /var/log/llxrice-backup.log 2>&1
```

### 数据库恢复

```bash
# Docker 环境
gunzip backup_20251017_020000.sql.gz
docker exec -i llxrice_db psql -U llxrice_user llxrice < backup_20251017_020000.sql

# Systemd 环境
gunzip backup_20251017_020000.sql.gz
sudo -u postgres psql llxrice < backup_20251017_020000.sql
```

---

## 故障排除

### 常见问题

#### 1. 服务无法启动

**症状**: `systemctl start llxrice-api` 失败

**排查步骤**:

```bash
# 查看详细错误信息
sudo journalctl -u llxrice-api -n 50 --no-pager

# 检查配置文件
cat /var/www/llxrice-api/appsettings.Production.json

# 检查文件权限
ls -la /var/www/llxrice-api/

# 测试应用启动
cd /var/www/llxrice-api
sudo -u www-data dotnet LLXRice.Api.dll
```

#### 2. 数据库连接失败

**症状**: 日志中出现 "Could not connect to database"

**排查步骤**:

```bash
# 检查 PostgreSQL 是否运行
sudo systemctl status postgresql

# 测试数据库连接
psql -h localhost -U llxrice_user -d llxrice

# 检查防火墙
sudo ufw status

# 检查 PostgreSQL 监听地址
sudo grep "listen_addresses" /etc/postgresql/16/main/postgresql.conf
```

#### 3. Redis 连接失败

**症状**: 日志中出现 "Redis connection failed"

**排查步骤**:

```bash
# 检查 Redis 是否运行
sudo systemctl status redis-server

# 测试 Redis 连接
redis-cli -a your_password ping

# 检查 Redis 配置
sudo grep "bind" /etc/redis/redis.conf
sudo grep "requirepass" /etc/redis/redis.conf
```

#### 4. 性能问题

**症状**: 响应缓慢，超时

**排查步骤**:

```bash
# 查看系统资源使用
top
htop

# 查看数据库连接数
sudo -u postgres psql -c "SELECT count(*) FROM pg_stat_activity WHERE datname='llxrice';"

# 查看慢查询
sudo -u postgres psql llxrice -c "SELECT * FROM pg_stat_statements ORDER BY mean_exec_time DESC LIMIT 10;"

# 检查 Redis 内存使用
redis-cli -a your_password info memory

# 查看应用日志
tail -f /var/www/llxrice-api/logs/log-$(date +%Y%m%d).txt
```

#### 5. Docker 容器无法启动

**症状**: `docker compose up -d` 失败

**排查步骤**:

```bash
# 查看容器日志
docker compose logs

# 查看容器状态
docker compose ps

# 检查端口占用
sudo netstat -tulpn | grep -E ':(8080|5432|6379)'

# 检查 .env 文件
cat .env

# 删除容器重新创建
docker compose down
docker compose up -d
```

### 获取技术支持

如遇到其他问题，请提供以下信息:

1. 操作系统版本: `cat /etc/os-release`
2. .NET 版本: `dotnet --version`
3. PostgreSQL 版本: `psql --version`
4. Redis 版本: `redis-server --version`
5. 错误日志: 最近的错误日志内容
6. 配置文件: `appsettings.Production.json` (隐藏敏感信息)

---

## 安全建议

### 1. 密码安全

- ✅ 使用强密码（至少 16 位，包含大小写字母、数字、特殊字符）
- ✅ 定期更换密码
- ✅ 不同服务使用不同密码
- ✅ 不要在代码中硬编码密码

### 2. 网络安全

```bash
# 配置防火墙（只开放必要端口）
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp    # HTTP
sudo ufw allow 443/tcp   # HTTPS
sudo ufw enable

# 不要对外暴露数据库端口
# 确保 PostgreSQL 和 Redis 只监听 localhost
```

### 3. 系统安全

```bash
# 定期更新系统
sudo apt update && sudo apt upgrade -y

# 配置 fail2ban 防止暴力破解
sudo apt install -y fail2ban

# 禁用 root 远程登录
sudo nano /etc/ssh/sshd_config
# PermitRootLogin no
sudo systemctl restart sshd
```

### 4. 应用安全

- ✅ 使用 HTTPS（配置 SSL 证书）
- ✅ 启用 CORS 白名单（不要使用 `AllowAnyOrigin`）
- ✅ 输入验证和参数化查询（防止 SQL 注入）
- ✅ 定期备份数据
- ✅ 监控异常登录和操作

---

## 性能优化

### 1. 数据库优化

```sql
-- 定期分析统计信息
ANALYZE;

-- 定期清理
VACUUM;

-- 重建索引
REINDEX DATABASE llxrice;
```

### 2. 连接池配置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llxrice;Username=llxrice_user;Password=xxx;Minimum Pool Size=5;Maximum Pool Size=100"
  }
}
```

### 3. Redis 缓存优化

```bash
# 设置最大内存限制
sudo nano /etc/redis/redis.conf
# maxmemory 256mb
# maxmemory-policy allkeys-lru

sudo systemctl restart redis-server
```

---

## 总结

本文档提供了两种部署方式:

1. **Docker 部署**: 适合快速部署、开发测试、容器化环境
2. **Systemd 部署**: 适合传统部署、已有基础设施、需要细粒度控制

推荐生产环境使用 Docker 部署，配合 Nginx 反向代理和 Let's Encrypt SSL 证书，可以快速搭建一个安全、稳定、高性能的后端服务。

---

**文档版本**: v1.0  
**最后更新**: 2025-10-17  
**适用版本**: .NET 8 + PostgreSQL 16 + Redis 7.2

---

© 2025 林龍香大米商城后端服务部署指南

