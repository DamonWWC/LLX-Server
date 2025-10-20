# Nginx 配置文件说明

## 📁 文件结构

```
nginx/
├── nginx.conf           # Nginx 主配置文件
├── conf.d/
│   └── llxrice.conf    # 站点配置文件
├── ssl/                 # SSL 证书目录 (需自行创建)
│   ├── fullchain.pem   # SSL 证书
│   └── privkey.pem     # 私钥
└── logs/               # 日志目录 (自动创建)
```

## 🚀 快速开始

### Docker Compose 部署

配置文件已在 `docker-compose.yml` 中引用，直接启动即可：

```bash
# 1. 创建 SSL 证书目录
mkdir -p nginx/ssl

# 2. 生成自签名证书（开发环境）
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/privkey.pem \
  -out nginx/ssl/fullchain.pem \
  -subj "/C=CN/ST=Beijing/L=Beijing/O=LLXRice/CN=api.llxrice.com"

# 3. 启动所有服务
docker-compose up -d

# 4. 查看 Nginx 日志
docker-compose logs -f nginx

# 5. 测试配置
docker exec llxrice_nginx nginx -t

# 6. 重新加载配置（无需重启）
docker exec llxrice_nginx nginx -s reload
```

### 独立部署

将配置文件复制到 Nginx 配置目录：

```bash
# 复制主配置文件
sudo cp nginx.conf /etc/nginx/nginx.conf

# 复制站点配置
sudo cp conf.d/llxrice.conf /etc/nginx/sites-available/llxrice
sudo ln -s /etc/nginx/sites-available/llxrice /etc/nginx/sites-enabled/

# 测试配置
sudo nginx -t

# 重启 Nginx
sudo systemctl restart nginx
```

## 🔧 配置说明

### 主要功能

1. **HTTPS/SSL 终止**
   - 自动将 HTTP 请求重定向到 HTTPS
   - 支持 TLS 1.2 和 TLS 1.3
   - 安全的加密套件配置

2. **负载均衡**
   - 使用 `least_conn` 策略（最少连接）
   - 支持多个后端实例
   - 自动健康检查

3. **请求限流**
   - API 请求：100 请求/秒，突发 200
   - 登录请求：5 请求/秒
   - 单 IP 最多 20 个并发连接

4. **Gzip 压缩**
   - 压缩 JSON、JavaScript、CSS 等
   - 压缩级别：6
   - 减少网络传输量

5. **静态资源服务**
   - 缓存 7 天
   - 添加 immutable 标记
   - 提升加载速度

6. **日志记录**
   - 访问日志：包含响应时间
   - 错误日志：warn 级别
   - 便于问题排查

### 需要修改的地方

**1. 域名配置**

在 `conf.d/llxrice.conf` 中：

```nginx
server_name api.llxrice.com;  # 替换为您的域名
```

**2. 后端地址**

如果后端不是运行在 `api:8080`，修改上游配置：

```nginx
upstream llxrice_api {
    server your-backend-host:port;
}
```

**3. SSL 证书路径**

如果使用 Let's Encrypt：

```nginx
ssl_certificate /etc/letsencrypt/live/your-domain/fullchain.pem;
ssl_certificate_key /etc/letsencrypt/live/your-domain/privkey.pem;
```

## 🔒 SSL 证书配置

### 使用 Let's Encrypt（推荐）

```bash
# 安装 Certbot
sudo apt install certbot python3-certbot-nginx

# 获取证书
sudo certbot --nginx -d api.llxrice.com

# 自动续期
sudo certbot renew --dry-run

# 设置自动续期（cron）
(crontab -l 2>/dev/null; echo "0 0 1 * * /usr/bin/certbot renew --quiet") | crontab -
```

### 使用自签名证书（开发）

```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/privkey.pem \
  -out ssl/fullchain.pem \
  -subj "/C=CN/ST=Beijing/L=Beijing/O=LLXRice/CN=api.llxrice.com"
```

## 📊 监控和维护

### 查看日志

```bash
# Docker 环境
docker exec llxrice_nginx tail -f /var/log/nginx/llxrice_access.log
docker exec llxrice_nginx tail -f /var/log/nginx/llxrice_error.log

# 独立部署
sudo tail -f /var/log/nginx/llxrice_access.log
sudo tail -f /var/log/nginx/error.log
```

### 统计分析

```bash
# 统计访问最多的 IP
awk '{print $1}' access.log | sort | uniq -c | sort -rn | head -10

# 统计访问最多的 URL
awk '{print $7}' access.log | sort | uniq -c | sort -rn | head -10

# 统计 HTTP 状态码
awk '{print $9}' access.log | sort | uniq -c | sort -rn

# 统计响应时间（最慢的 10 个请求）
awk '{print $NF, $7}' access.log | sort -rn | head -10
```

### 性能测试

```bash
# 使用 ab (Apache Bench)
ab -n 1000 -c 10 https://api.llxrice.com/health

# 使用 wrk
wrk -t4 -c100 -d30s https://api.llxrice.com/api/products

# 使用 hey
hey -n 1000 -c 50 https://api.llxrice.com/health
```

## ⚙️ 性能优化

### Worker 进程优化

根据 CPU 核心数调整：

```nginx
worker_processes auto;  # 自动检测 CPU 核心数

events {
    worker_connections 4096;  # 增加连接数
    use epoll;                # Linux 下使用 epoll
    multi_accept on;          # 一次接受多个连接
}
```

### 缓存配置

添加代理缓存：

```nginx
# 在 http 块中
proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=api_cache:10m inactive=60m;

# 在 location 块中
location /api/ {
    proxy_cache api_cache;
    proxy_cache_valid 200 5m;
    proxy_cache_valid 404 1m;
    add_header X-Cache-Status $upstream_cache_status;
    # ...
}
```

### 连接保持

```nginx
keepalive_timeout 65;
keepalive_requests 100;

upstream llxrice_api {
    keepalive 32;
    keepalive_timeout 60s;
}
```

## 🔍 故障排查

### 常见问题

**1. 502 Bad Gateway**

- 检查后端服务是否运行
- 检查防火墙设置
- 查看错误日志

```bash
docker-compose ps
docker exec llxrice_nginx cat /var/log/nginx/error.log
```

**2. 配置加载失败**

```bash
# 测试配置文件
docker exec llxrice_nginx nginx -t

# 查看错误信息
docker-compose logs nginx
```

**3. SSL 证书错误**

- 检查证书文件路径
- 确认证书未过期
- 验证证书链完整

```bash
# 检查证书有效期
openssl x509 -in ssl/fullchain.pem -noout -dates

# 验证证书链
openssl verify -CAfile ssl/fullchain.pem ssl/fullchain.pem
```

## 📝 最佳实践

1. **定期更新 Nginx**
   ```bash
   docker pull nginx:alpine
   docker-compose up -d --build nginx
   ```

2. **配置日志轮转**
   ```bash
   # 使用 logrotate
   cat > /etc/logrotate.d/nginx << EOF
   /var/log/nginx/*.log {
       daily
       rotate 30
       compress
       delaycompress
       notifempty
       create 0640 nginx nginx
       sharedscripts
       postrotate
           docker exec llxrice_nginx nginx -s reload
       endscript
   }
   EOF
   ```

3. **监控告警**
   - 使用 Prometheus + Grafana 监控
   - 配置 nginx-prometheus-exporter
   - 设置告警规则

4. **安全加固**
   - 隐藏 Nginx 版本号
   - 配置 HSTS
   - 启用 CSP（内容安全策略）
   - 定期更新 SSL 配置

## 📚 参考资料

- [Nginx 官方文档](https://nginx.org/en/docs/)
- [Let's Encrypt 文档](https://letsencrypt.org/docs/)
- [Mozilla SSL 配置生成器](https://ssl-config.mozilla.org/)
- [Nginx 性能优化指南](https://www.nginx.com/blog/tuning-nginx/)

## 🆘 需要帮助？

如有问题，请查看：
1. [后端服务设计方案.md](../后端服务设计方案.md) - 完整架构说明
2. [DEPLOYMENT.md](../DEPLOYMENT.md) - 详细部署指南
3. Nginx 错误日志 - 具体错误信息

---

**版本**：v1.0  
**更新时间**：2025-10-17

