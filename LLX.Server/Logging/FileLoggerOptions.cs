namespace LLX.Server.Logging;

/// <summary>
/// 文件日志配置选项
/// </summary>
public class FileLoggerOptions
{
    /// <summary>
    /// 日志目录路径
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// 保留的文件数量限制（0表示不限制）
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 30;

    /// <summary>
    /// 最小日志级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}

