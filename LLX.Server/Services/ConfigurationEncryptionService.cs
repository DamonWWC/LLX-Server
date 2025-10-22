using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace LLX.Server.Services
{
    /// <summary>
    /// 配置加密服务实现
    /// </summary>
    public class ConfigurationEncryptionService : IConfigurationEncryptionService
    {
        private readonly string _encryptionKey;
        private readonly string _encryptionPrefix = "ENC:";
        private readonly ILogger<ConfigurationEncryptionService> _logger;

        public ConfigurationEncryptionService(IConfiguration configuration, ILogger<ConfigurationEncryptionService> logger)
        {
            _logger = logger;
            _encryptionKey = configuration["EncryptionSettings:Key"] ??
                            Environment.GetEnvironmentVariable("RILEY_ENCRYPTION_KEY") ??
                            throw new InvalidOperationException("未配置加密密钥。请在配置文件中设置 EncryptionSettings:Key 或环境变量 RILEY_ENCRYPTION_KEY");

            if (_encryptionKey.Length < 32)
            {
                throw new InvalidOperationException("加密密钥长度必须至少32个字符");
            }
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>加密后的字符串</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey[..32]); // 使用前32个字符作为密钥
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                var iv = aes.IV;
                var encryptedContent = msEncrypt.ToArray();
                var result = new byte[iv.Length + encryptedContent.Length];

                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                var encryptedString = _encryptionPrefix + Convert.ToBase64String(result);
                _logger.LogDebug("字符串加密成功，长度: {Length}", encryptedString.Length);
                return encryptedString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加密字符串时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="encryptedText">加密的文本</param>
        /// <returns>解密后的明文</returns>
        public string Decrypt(string encryptedText)
        {

            Debug.WriteLine(_encryptionKey);
            if (string.IsNullOrEmpty(encryptedText) || !IsEncrypted(encryptedText))
                return encryptedText;

            try
            {
                var base64String = encryptedText[_encryptionPrefix.Length..];
                var fullCipher = Convert.FromBase64String(base64String);

                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey[..32]);

                var iv = new byte[aes.BlockSize / 8];
                var cipherText = new byte[fullCipher.Length - iv.Length];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipherText);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                var decryptedText = srDecrypt.ReadToEnd();
                _logger.LogDebug("字符串解密成功");
                return decryptedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"解密字符串时发生错误: {encryptedText},EncryptionKey:{_encryptionKey}", encryptedText);
                return string.Empty;
                //throw new InvalidOperationException("解密失败，可能是密钥错误或数据损坏", ex);
            }
        }

        /// <summary>
        /// 判断字符串是否已加密
        /// </summary>
        /// <param name="text">要检查的文本</param>
        /// <returns>如果已加密返回true</returns>
        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(_encryptionPrefix);
        }

        /// <summary>
        /// 解密数据库连接字符串（如果已加密）
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>解密后的连接字符串</returns>
        public string DecryptConnectionStringIfNeeded(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            if (IsEncrypted(connectionString))
            {
                var decrypted = Decrypt(connectionString);
                _logger.LogInformation("数据库连接字符串已解密");
                return decrypted;
            }

            _logger.LogWarning("数据库连接字符串未加密，建议加密存储");
            return connectionString;
        }
    }
}
