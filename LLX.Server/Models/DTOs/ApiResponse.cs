namespace LLX.Server.Models.DTOs;

/// <summary>
/// 统一 API 响应格式
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = "操作成功";

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="message">消息</param>
    /// <returns>成功响应</returns>
    public static ApiResponse<T> SuccessResponse(T data, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建错误响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="errors">详细错误列表</param>
    /// <returns>错误响应</returns>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
