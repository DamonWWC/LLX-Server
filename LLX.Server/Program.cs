using DotNetEnv;
using LLX.Server.Data;
using LLX.Server.Endpoints;
using LLX.Server.Extensions;
using LLX.Server.Logging;
using LLX.Server.Middleware;
using LLX.Server.Services;
using LLX.Server.Utils;
using Microsoft.EntityFrameworkCore;

namespace LLX.Server;

public class Program
{

    private static AutoResetEvent auto = new AutoResetEvent(false);
    private static void AutoResetEventHandler()
    {
        Console.WriteLine("当前线程id" + Thread.CurrentThread.ManagedThreadId);
        auto.WaitOne();//阻塞线程
        Console.WriteLine("等待一秒后执行");
    }
    private static void AutoResetEventHandlerTwo()
    {
        auto.WaitOne();//阻塞线程
        Console.WriteLine("我是第二个等待执行");
    }
    public static async Task Main(string[] args)
    {
        Thread thread1 = new Thread(AutoResetEventHandler);
        Console.WriteLine("当前线程id" + Thread.CurrentThread.ManagedThreadId);
        thread1.Start();
        Thread.Sleep(5000);
        auto.Set();
        // auto.Reset();  在这种情况下new AutoResetEvent(ture) 的 类实例 会变成无信号未终止状态的 如果阻塞线程没有接收到信号量将会一直阻塞下去，直到接收到信号量
        Thread thread2 = new Thread(AutoResetEventHandlerTwo);
        thread2.Start();
        Thread.Sleep(10000);//等待3秒

        Env.Load();

        try
        {
            if (args.Length > 0 && IsEncryptionToolCommand(args[0]))
            {
                await ConfigurationEncryptionTool.RunAsync(args);
                return;
            }

            var builder = WebApplication.CreateBuilder(args);

            // 配置日志系统
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.AddFileLogger(builder.Configuration);

            // 配置 JSON 序列化
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = true;
            });
            builder.Services.AddSingleton<IConfigurationEncryptionService, ConfigurationEncryptionService>();
            // 获取配置
            var configuration = builder.Configuration;          
            // 注册服务
            builder.Services
                .AddSwagger();

            // 只在有数据库连接字符串时注册核心服务
            if (!string.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
            {
                builder.Services
                    .AddCoreServices(configuration)
                    .AddHealthChecks(configuration);
            }
            else
            {
                // 开发模式：注册模拟服务
                builder.Services.AddScoped<LLX.Server.Services.ICacheService, LLX.Server.Services.RedisCacheService>();
            }

            // 添加 CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // 配置中间件管道

            // 性能监控（最先执行）
            app.UsePerformanceMonitoring();

            // 全局异常处理
            app.UseMiddleware<ExceptionMiddleware>();

            // 请求日志
            app.UseMiddleware<LoggingMiddleware>();

            // CORS
            app.UseCors("AllowAll");

            // Swagger（开发环境）
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "林龍香大米商城 API v1");
                    c.RoutePrefix = "swagger";
                });
                app.MapSwagger();
            }

            // 健康检查
            app.MapHealthChecks("/health");

            // 注册API端点
            app.MapProductEndpoints();
            app.MapAddressEndpoints();
            app.MapOrderEndpoints();
            app.MapShippingEndpoints();
            
            // 注册日志测试端点（仅开发环境）
            if (app.Environment.IsDevelopment())
            {
                app.MapLoggingTestEndpoints();
            }

            // 根路径重定向到 Swagger
            app.MapGet("/", () => Results.Redirect("/swagger"))
                .ExcludeFromDescription();

            // 数据库迁移（开发环境）
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("Database migration completed");
                }
            }

            Console.WriteLine("Application configured successfully");

            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application terminated unexpectedly: {ex.Message}");
        }
    }
    private static bool IsEncryptionToolCommand(string command)
    {
        var encryptionCommands = new[] { "encrypt", "decrypt", "encrypt-config", "generate-key" };
        return encryptionCommands.Contains(command.ToLower());
    }
}
