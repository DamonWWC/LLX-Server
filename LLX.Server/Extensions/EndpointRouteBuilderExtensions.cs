using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LLX.Server.Extensions;

/// <summary>
/// 端点路由构建器扩展方法
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 映射健康检查端点
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");
        return app;
    }

    /// <summary>
    /// 映射 Swagger 端点
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IEndpointRouteBuilder MapSwaggerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapSwagger();
        app.MapGet("/", () => Results.Redirect("/swagger"))
            .ExcludeFromDescription();
        return app;
    }
}
