using System.Collections.Concurrent;
using System.Text;

namespace LLX.Server.Logging;

/// <summary>
/// 文件日志记录器
/// </summary>
public sealed class FileLogger : ILogger, IDisposable
{
    private readonly string _categoryName;
    private readonly FileLoggerOptions _options;
    private readonly ConcurrentDictionary<LogLevel, StreamWriter> _writers = new();
    private readonly object _lock = new();
    private bool _disposed;

    public FileLogger(string categoryName, FileLoggerOptions options)
    {
        _categoryName = categoryName;
        _options = options;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && !_disposed;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || _disposed)
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        WriteMessage(logLevel, message, exception);
    }

    private void WriteMessage(LogLevel logLevel, string message, Exception? exception)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                var writer = GetOrCreateWriter(logLevel);
                if (writer == null)
                {
                    return;
                }

                var logEntry = FormatLogEntry(logLevel, message, exception);
                writer.WriteLine(logEntry);
                writer.Flush();

                // 清理过期日志
                CleanupOldLogsIfNeeded();
            }
            catch (Exception ex)
            {
                // 日志写入失败时输出到控制台
                Console.Error.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }

    private StreamWriter? GetOrCreateWriter(LogLevel logLevel)
    {
        return _writers.GetOrAdd(logLevel, level =>
        {
            var filePath = GetLogFilePath(level);
            var fileInfo = new FileInfo(filePath);

            // 确保目录存在
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            // 创建或追加到文件，允许多个进程同时读写
            var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            return new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        });
    }

    private string GetLogFilePath(LogLevel logLevel)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var levelName = GetLogLevelName(logLevel);
        var fileName = $"{levelName}-{date}.log";
        return Path.Combine(_options.LogDirectory, fileName);
    }

    private string GetLogLevelName(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warning",
            LogLevel.Error => "error",
            LogLevel.Critical => "critical",
            _ => "unknown"
        };
    }

    private string FormatLogEntry(LogLevel logLevel, string message, Exception? exception)
    {
        var sb = new StringBuilder();
        
        // 时间戳
        sb.Append('[');
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        sb.Append("] ");

        // 日志级别
        sb.Append('[');
        sb.Append(GetLogLevelName(logLevel).ToUpper().PadRight(8));
        sb.Append("] ");

        // 类别名称
        sb.Append(_categoryName);
        sb.Append(": ");

        // 消息
        sb.AppendLine(message);

        // 异常信息
        if (exception != null)
        {
            sb.AppendLine("Exception:");
            sb.AppendLine(exception.ToString());
        }

        return sb.ToString();
    }

    private void CleanupOldLogsIfNeeded()
    {
        if (_options.RetainedFileCountLimit <= 0)
        {
            return;
        }

        try
        {
            var directory = new DirectoryInfo(_options.LogDirectory);
            if (!directory.Exists)
            {
                return;
            }

            // 获取所有日志文件，按修改时间排序
            var logFiles = directory.GetFiles("*.log")
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            // 删除超过保留数量的文件
            var filesToDelete = logFiles.Skip(_options.RetainedFileCountLimit);
            foreach (var file in filesToDelete)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // 忽略删除失败
                }
            }
        }
        catch
        {
            // 忽略清理失败
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // 释放所有 writer
            foreach (var writer in _writers.Values)
            {
                try
                {
                    writer.Flush();
                    writer.Dispose();
                }
                catch
                {
                    // 忽略释放错误
                }
            }

            _writers.Clear();
        }
    }
}

