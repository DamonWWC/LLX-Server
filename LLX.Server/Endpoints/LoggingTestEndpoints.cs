using LLX.Server.Logging;
using Microsoft.AspNetCore.Mvc;

namespace LLX.Server.Endpoints;

/// <summary>
/// 日志测试端点
/// </summary>
public static class LoggingTestEndpoints
{
    public static void MapLoggingTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logging-test")
            .WithTags("Logging Test");

        // 测试所有日志级别
        group.MapGet("/test-levels", (ILogger<LoggingTest> logger) =>
        {
            var test = new LoggingTest(logger);
            test.TestAllLogLevels();
            
            return Results.Ok(new { 
                message = "所有日志级别测试完成，请检查日志文件",
                timestamp = DateTime.UtcNow 
            });
        })
        .WithName("TestLogLevels")
        .WithSummary("测试所有日志级别")
        .WithDescription("生成所有级别的日志记录，用于验证日志系统是否正常工作");

        // 测试结构化日志
        group.MapGet("/test-structured", (ILogger<LoggingTest> logger) =>
        {
            var test = new LoggingTest(logger);
            test.TestStructuredLogging();
            
            return Results.Ok(new { 
                message = "结构化日志测试完成，请检查日志文件",
                timestamp = DateTime.UtcNow 
            });
        })
        .WithName("TestStructuredLogging")
        .WithSummary("测试结构化日志")
        .WithDescription("生成包含结构化参数的日志记录");

        // 测试异常日志
        group.MapGet("/test-exceptions", (ILogger<LoggingTest> logger) =>
        {
            var test = new LoggingTest(logger);
            test.TestExceptionLogging();
            
            return Results.Ok(new { 
                message = "异常日志测试完成，请检查错误日志文件",
                timestamp = DateTime.UtcNow 
            });
        })
        .WithName("TestExceptionLogging")
        .WithSummary("测试异常日志")
        .WithDescription("生成包含异常信息的日志记录");

        // 测试不同类别日志
        group.MapGet("/test-categories", (ILogger<LoggingTest> logger) =>
        {
            var test = new LoggingTest(logger);
            test.TestDifferentCategories();
            
            return Results.Ok(new { 
                message = "不同类别日志测试完成，请检查日志文件",
                timestamp = DateTime.UtcNow 
            });
        })
        .WithName("TestDifferentCategories")
        .WithSummary("测试不同类别日志")
        .WithDescription("生成不同服务类别的日志记录");

        // 获取日志文件信息
        group.MapGet("/log-files", () =>
        {
            var logDirectory = "logs";
            if (!Directory.Exists(logDirectory))
            {
                return Results.Ok(new { 
                    message = "日志目录不存在",
                    files = new string[0]
                });
            }

            var files = Directory.GetFiles(logDirectory, "*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new
                {
                    name = f.Name,
                    size = f.Length,
                    lastWriteTime = f.LastWriteTime,
                    fullPath = f.FullName
                })
                .ToArray();

            return Results.Ok(new { 
                message = $"找到 {files.Length} 个日志文件",
                logDirectory,
                files
            });
        })
        .WithName("GetLogFiles")
        .WithSummary("获取日志文件信息")
        .WithDescription("列出当前所有日志文件及其信息");
    }
}
