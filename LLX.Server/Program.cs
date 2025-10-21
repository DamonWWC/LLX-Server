using DotNetEnv;
using LLX.Server.Data;
using LLX.Server.Extensions;
using LLX.Server.Middleware;
using LLX.Server.Services;
using LLX.Server.Utils;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LLX.Server;

public class Program
{
    public static async Task Main(string[] args)
    {

        Env.Load();
        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build())
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            Log.Information("Starting LLX.Server application");


            if (args.Length > 0 && IsEncryptionToolCommand(args[0]))
            {
                await ConfigurationEncryptionTool.RunAsync(args);
                return;
            }

            var builder = WebApplication.CreateBuilder(args);

            // 使用 Serilog
            builder.Host.UseSerilog();

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
                .AddDatabase(configuration)
                .AddRedis(configuration)
                .AddHealthChecks(configuration)
                .AddSwagger();

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
            app.UseSerilogRequestLogging();

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
                    Log.Information("Database migration completed");
                }
            }

            Log.Information("Application configured successfully");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    private static bool IsEncryptionToolCommand(string command)
    {
        var encryptionCommands = new[] { "encrypt", "decrypt", "encrypt-config", "generate-key" };
        return encryptionCommands.Contains(command.ToLower());
    }
}
