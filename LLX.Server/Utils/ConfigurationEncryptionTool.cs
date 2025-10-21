using LLX.Server.Services;
using System.Text;
using System.Text.Json;

namespace LLX.Server.Utils
{
    /// <summary>
    /// 配置加密工具
    /// 用于加密和解密配置文件中的敏感信息
    /// </summary>
    public static class ConfigurationEncryptionTool
    {
        /// <summary>
        /// 运行加密工具
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static async Task RunAsync(string[] args)
        {
            Console.WriteLine("=== LLX 配置加密工具 ===");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();

            switch (command)
            {
                case "encrypt":
                    await EncryptCommand(args);
                    break;
                case "decrypt":
                    await DecryptCommand(args);
                    break;
                case "encrypt-config":
                    await EncryptConfigFileCommand(args);
                    break;
                case "generate-key":
                    GenerateEncryptionKey();
                    break;
                case "help":
                case "-h":
                case "--help":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"未知命令: {command}");
                    ShowHelp();
                    break;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("用法:");
            Console.WriteLine("  dotnet run -- encrypt \"要加密的字符串\"");
            Console.WriteLine("  dotnet run -- decrypt \"ENC:加密的字符串\"");
            Console.WriteLine("  dotnet run -- encrypt-config [配置文件路径]");
            Console.WriteLine("  dotnet run -- generate-key");
            Console.WriteLine();
            Console.WriteLine("命令说明:");
            Console.WriteLine("  encrypt        加密指定的字符串");
            Console.WriteLine("  decrypt        解密指定的字符串");
            Console.WriteLine("  encrypt-config 加密配置文件中的数据库连接字符串");
            Console.WriteLine("  generate-key   生成新的加密密钥");
            Console.WriteLine();
            Console.WriteLine("环境变量:");
            Console.WriteLine("  RILEY_ENCRYPTION_KEY  - 加密密钥（必需）");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  dotnet run -- encrypt \"Server=localhost;Database=test;\"");
            Console.WriteLine("  dotnet run -- encrypt-config appsettings.json");
        }

        private static Task EncryptCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("错误: 请提供要加密的字符串");
                Console.WriteLine("用法: dotnet run -- encrypt \"要加密的字符串\"");
                return Task.CompletedTask;
            }

            try
            {
                var encryptionService = CreateEncryptionService();
                var plainText = args[1];
                var encrypted = encryptionService.Encrypt(plainText);

                Console.WriteLine("加密成功!");
                Console.WriteLine($"原始字符串: {plainText}");
                Console.WriteLine($"加密字符串: {encrypted}");
                Console.WriteLine();
                Console.WriteLine("请将加密字符串复制到配置文件中替换原始连接字符串。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加密失败: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task DecryptCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("错误: 请提供要解密的字符串");
                Console.WriteLine("用法: dotnet run -- decrypt \"ENC:加密的字符串\"");
                return Task.CompletedTask;
            }

            try
            {
                var encryptionService = CreateEncryptionService();
                var encryptedText = args[1];
                var decrypted = encryptionService.Decrypt(encryptedText);

                Console.WriteLine("解密成功!");
                Console.WriteLine($"加密字符串: {encryptedText}");
                Console.WriteLine($"解密字符串: {decrypted}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解密失败: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private static async Task EncryptConfigFileCommand(string[] args)
        {
            var configPath = args.Length > 1 ? args[1] : "appsettings.json";

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"错误: 配置文件不存在: {configPath}");
                return;
            }

            try
            {
                Console.WriteLine($"正在处理配置文件: {configPath}");
                Console.WriteLine();

                var jsonContent = await File.ReadAllTextAsync(configPath);
                
                // 使用 JsonElement 来保持原始 JSON 结构和格式
                using var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;
                
                var encryptionService = CreateEncryptionService();
                bool hasChanges = false;
                int encryptedCount = 0;
                int alreadyEncryptedCount = 0;

                // 构建新的 JSON 对象
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                
                writer.WriteStartObject();

                // 遍历所有根级属性
                foreach (var property in root.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);

                    // 特殊处理 ConnectionStrings 节点
                    if (property.Name == "ConnectionStrings" && property.Value.ValueKind == JsonValueKind.Object)
                    {
                        writer.WriteStartObject();
                        
                        foreach (var connStr in property.Value.EnumerateObject())
                        {
                            var connectionString = connStr.Value.GetString();
                            
                            if (!string.IsNullOrEmpty(connectionString))
                            {
                                if (encryptionService.IsEncrypted(connectionString))
                                {
                                    // 已经加密，保持不变
                                    writer.WriteString(connStr.Name, connectionString);
                                    alreadyEncryptedCount++;
                                    Console.WriteLine($"✓ [{connStr.Name}] 已经加密，跳过");
                                }
                                else
                                {
                                    // 需要加密
                                    var encrypted = encryptionService.Encrypt(connectionString);
                                    writer.WriteString(connStr.Name, encrypted);
                                    hasChanges = true;
                                    encryptedCount++;
                                    Console.WriteLine($"✓ [{connStr.Name}] 加密成功");
                                    Console.WriteLine($"   原文: {MaskSensitiveInfo(connectionString)}");
                                    Console.WriteLine($"   密文: {encrypted}");
                                    Console.WriteLine();
                                }
                            }
                            else
                            {
                                writer.WriteString(connStr.Name, connectionString);
                            }
                        }
                        
                        writer.WriteEndObject();
                    }
                    else
                    {
                        // 其他属性保持原样
                        property.Value.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
                writer.Flush();

                // 输出统计信息
                Console.WriteLine("===========================================");
                Console.WriteLine($"处理完成:");
                Console.WriteLine($"  - 新加密字段: {encryptedCount}");
                Console.WriteLine($"  - 已加密字段: {alreadyEncryptedCount}");
                Console.WriteLine($"  - 配置文件: {configPath}");
                Console.WriteLine("===========================================");
                Console.WriteLine();

                if (hasChanges)
                {
                    // 创建备份
                    var backupPath = $"{configPath}.backup.{DateTime.Now:yyyyMMddHHmmss}";
                    File.Copy(configPath, backupPath);
                    Console.WriteLine($"✓ 原始配置文件已备份到: {backupPath}");

                    // 保存加密后的配置
                    var encryptedJson = Encoding.UTF8.GetString(stream.ToArray());
                    await File.WriteAllTextAsync(configPath, encryptedJson);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ 配置文件加密完成!");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("注意: 请确保设置了正确的加密密钥环境变量:");
                    Console.WriteLine("  RILEY_ENCRYPTION_KEY");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("ℹ 所有连接字符串都已加密，无需处理。");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ 处理配置文件失败: {ex.Message}");
                Console.WriteLine($"详细信息: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 遮蔽敏感信息（用于日志输出）
        /// </summary>
        private static string MaskSensitiveInfo(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            // 遮蔽密码信息
            var patterns = new[] { "Password=", "password=", "Pwd=", "pwd=" };
            var result = connectionString;

            foreach (var pattern in patterns)
            {
                var index = result.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var startIndex = index + pattern.Length;
                    var endIndex = result.IndexOfAny(new[] { ';', ',' }, startIndex);
                    if (endIndex < 0) endIndex = result.Length;

                    var length = endIndex - startIndex;
                    if (length > 0)
                    {
                        result = result.Substring(0, startIndex) + "****" + result.Substring(endIndex);
                    }
                }
            }

            return result;
        }

        private static void GenerateEncryptionKey()
        {
            var key = GenerateRandomKey(64); // 生成64字符的密钥

            Console.WriteLine("已生成新的加密密钥:");
            Console.WriteLine(key);
            Console.WriteLine();
            Console.WriteLine("请将此密钥设置为环境变量或配置文件中的 EncryptionSettings:Key");
            Console.WriteLine("环境变量设置命令:");
            Console.WriteLine($"set RILEY_ENCRYPTION_KEY={key}");
            Console.WriteLine();
            Console.WriteLine("或在 appsettings.json 中添加:");
            Console.WriteLine("{");
            Console.WriteLine("  \"EncryptionSettings\": {");
            Console.WriteLine($"    \"Key\": \"{key}\"");
            Console.WriteLine("  }");
            Console.WriteLine("}");
        }

        private static string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static IConfigurationEncryptionService CreateEncryptionService()
        {
            // 创建临时配置来获取加密密钥
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            // 创建日志记录器
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<ConfigurationEncryptionService>();

            return new ConfigurationEncryptionService(configuration, logger);
        }
    }
}
