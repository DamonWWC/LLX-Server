using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LLX.Server.Logging;

/// <summary>
/// 文件日志扩展方法
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// 添加文件日志
    /// </summary>
    public static ILoggingBuilder AddFileLogger(
        this ILoggingBuilder builder,
        Action<FileLoggerOptions>? configure = null)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());

        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        return builder;
    }

    /// <summary>
    /// 添加文件日志（从配置读取）
    /// </summary>
    public static ILoggingBuilder AddFileLogger(
        this ILoggingBuilder builder,
        IConfiguration configuration)
    {
        builder.AddFileLogger(options =>
        {
            configuration.GetSection("Logging:File").Bind(options);
        });

        return builder;
    }
}

