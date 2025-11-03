using System;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace RileyCache.MultiLevel.Policies;

/// <summary>
/// 弹性策略（重试、熔断）工厂，基于 Polly。
/// </summary>
public static class ResiliencePolicy
{
    /// <summary>
    /// 创建带指数退避的异步重试策略。
    /// </summary>
    /// <param name="retryCount">重试次数。</param>
    /// <param name="baseDelay">基础等待时长，未提供时默认 100ms。</param>
    /// <returns>异步重试策略。</returns>
    public static AsyncRetryPolicy CreateRetryPolicy(int retryCount = 3, TimeSpan? baseDelay = null)
    {
        var delay = baseDelay ?? TimeSpan.FromMilliseconds(100);
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt - 1)));
    }

    /// <summary>
    /// 创建异步熔断策略。
    /// </summary>
    /// <param name="exceptionsAllowedBeforeBreaking">触发熔断前允许的连续异常次数。</param>
    /// <param name="durationOfBreak">熔断持续时间，未提供时默认 10 秒。</param>
    /// <returns>异步熔断策略。</returns>
    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(
        int exceptionsAllowedBeforeBreaking = 5,
        TimeSpan? durationOfBreak = null)
    {
        var breakDuration = durationOfBreak ?? TimeSpan.FromSeconds(10);
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking, breakDuration);
    }
}


