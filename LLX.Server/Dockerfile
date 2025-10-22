# 林龍香大米商城后端服务 Dockerfile
# 多阶段构建，优化镜像大小和安全性

# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 设置工作目录
WORKDIR /src

# 复制项目文件并恢复依赖
COPY ["LLX.Server/LLX.Server.csproj", "LLX.Server/"]
RUN dotnet restore "LLX.Server/LLX.Server.csproj"

# 复制源代码
COPY . .

# 构建项目
WORKDIR "/src/LLX.Server"
RUN dotnet build "LLX.Server.csproj" -c Release -o /app/build

# 发布阶段
FROM build AS publish
RUN dotnet publish "LLX.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 运行时阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# 创建非root用户
RUN groupadd -r appuser && useradd -r -g appuser appuser

# 设置工作目录
WORKDIR /app

# 安装必要的工具
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# 复制发布文件
COPY --from=publish /app/publish .

# 创建日志目录并设置权限
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app && \
    chmod -R 755 /app/logs

# 切换到非root用户
USER appuser

# 暴露端口
EXPOSE 8080

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# 设置环境变量
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# 启动应用
ENTRYPOINT ["dotnet", "LLX.Server.dll"]