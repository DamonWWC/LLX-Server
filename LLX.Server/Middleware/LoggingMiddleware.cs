using System.Diagnostics;

namespace LLX.Server.Middleware;

/// <summary>
/// 请求日志中间件
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // 记录请求开始
        _logger.LogInformation(
            "Request {RequestId} started: {Method} {Path} from {RemoteIp}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress
        );

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // 记录请求完成
            _logger.LogInformation(
                "Request {RequestId} completed: {Method} {Path} with {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}
