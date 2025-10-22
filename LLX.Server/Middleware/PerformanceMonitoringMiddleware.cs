using System.Diagnostics;
using System.Text.Json;

namespace LLX.Server.Middleware;

/// <summary>
/// 性能监控中间件
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        // 记录请求开始
        var requestInfo = new
        {
            RequestId = requestId,
            Method = context.Request.Method,
            Path = context.Request.Path.Value,
            QueryString = context.Request.QueryString.Value,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Request started: {RequestInfo}", JsonSerializer.Serialize(requestInfo));

        // 添加请求ID到响应头
        context.Response.Headers["X-Request-ID"] = requestId;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // 记录请求完成
            var responseInfo = new
            {
                RequestId = requestId,
                StatusCode = context.Response.StatusCode,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ContentLength = context.Response.ContentLength,
                Timestamp = DateTime.UtcNow
            };

            // 根据响应时间和状态码选择日志级别
            var logLevel = GetLogLevel(stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
            
            _logger.Log(logLevel, "Request completed: {ResponseInfo}", JsonSerializer.Serialize(responseInfo));

            // 如果响应时间过长，记录警告
            var slowRequestThreshold = _configuration.GetValue<int>("Performance:SlowRequestThreshold", 1000);
            if (stopwatch.ElapsedMilliseconds > slowRequestThreshold)
            {
                _logger.LogWarning("Slow request detected: {RequestId} took {ElapsedMs}ms", 
                    requestId, stopwatch.ElapsedMilliseconds);
            }

            // 如果状态码是错误码，记录错误
            if (context.Response.StatusCode >= 400)
            {
                _logger.LogWarning("Request failed: {RequestId} returned {StatusCode}", 
                    requestId, context.Response.StatusCode);
            }
        }
    }

    /// <summary>
    /// 根据响应时间和状态码确定日志级别
    /// </summary>
    /// <param name="elapsedMs">响应时间（毫秒）</param>
    /// <param name="statusCode">状态码</param>
    /// <returns>日志级别</returns>
    private static LogLevel GetLogLevel(long elapsedMs, int statusCode)
    {
        if (statusCode >= 500)
            return LogLevel.Error;
        if (statusCode >= 400)
            return LogLevel.Warning;
        if (elapsedMs > 2000)
            return LogLevel.Warning;
        if (elapsedMs > 1000)
            return LogLevel.Information;
        
        return LogLevel.Debug;
    }
}

/// <summary>
/// 性能监控中间件扩展
/// </summary>
public static class PerformanceMonitoringMiddlewareExtensions
{
    /// <summary>
    /// 添加性能监控中间件
    /// </summary>
    /// <param name="builder">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}
