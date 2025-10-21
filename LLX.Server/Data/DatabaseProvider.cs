namespace LLX.Server.Data;

/// <summary>
/// 数据库提供程序类型
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSQL,

    /// <summary>
    /// SQL Server
    /// </summary>
    SqlServer,

    /// <summary>
    /// MySQL
    /// </summary>
    MySql,

    /// <summary>
    /// SQLite
    /// </summary>
    Sqlite
}

/// <summary>
/// 数据库配置选项
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// 数据库提供程序
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.PostgreSQL;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用敏感数据日志记录
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// 是否启用详细查询日志
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// 命令超时时间（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
}
