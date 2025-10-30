# 林龍香大米商城 - 后端服务 Dockerfile
# 基于 .NET 8 多阶段构建

# ========== 阶段 1: 基础运行时镜像 ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 安装必要的工具
RUN apk add --no-cache \
    curl \
    tzdata \
    && cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
    && echo "Asia/Shanghai" > /etc/timezone \
    && apk del tzdata

# 创建非 root 用户
RUN addgroup -g 1000 appuser \
    && adduser -D -u 1000 -G appuser appuser

# ========== 阶段 2: 构建镜像 ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 复制项目文件并恢复依赖
COPY ["src/LLXRice.Api/LLXRice.Api.csproj", "LLXRice.Api/"]
RUN dotnet restore "LLXRice.Api/LLXRice.Api.csproj"

# 复制所有源代码
COPY src/ .

# 构建项目
WORKDIR "/src/LLXRice.Api"
RUN dotnet build "LLXRice.Api.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# ========== 阶段 3: 发布镜像 ==========
FROM build AS publish
RUN dotnet publish "LLXRice.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ========== 阶段 4: 最终镜像 ==========
FROM base AS final
WORKDIR /app

# 复制发布的文件
COPY --from=publish /app/publish .

# 创建日志目录
RUN mkdir -p /app/logs \
    && chown -R appuser:appuser /app/logs

# 切换到非 root 用户
USER appuser

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_PRINT_TELEMETRY_MESSAGE=false \
    DOTNET_RUNNING_IN_CONTAINER=true

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "LLXRice.Api.dll"]

