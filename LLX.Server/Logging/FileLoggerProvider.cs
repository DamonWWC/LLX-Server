using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LLX.Server.Logging;

/// <summary>
/// 文件日志提供程序
/// </summary>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
    private readonly FileLoggerOptions _options;
    private bool _disposed;

    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        _options = options.Value;
        
        // 确保日志目录存在
        if (!Directory.Exists(_options.LogDirectory))
        {
            Directory.CreateDirectory(_options.LogDirectory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FileLoggerProvider));
        }

        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _options));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        
        // 释放所有 logger
        foreach (var logger in _loggers.Values)
        {
            logger.Dispose();
        }
        
        _loggers.Clear();
    }
}

