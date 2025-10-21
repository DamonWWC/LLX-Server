namespace LLX.Server.Services
{
    /// <summary>
    /// 配置加密服务接口
    /// </summary>
    public interface IConfigurationEncryptionService
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>加密后的字符串</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="encryptedText">加密的文本</param>
        /// <returns>解密后的明文</returns>
        string Decrypt(string encryptedText);

        /// <summary>
        /// 判断字符串是否已加密
        /// </summary>
        /// <param name="text">要检查的文本</param>
        /// <returns>如果已加密返回true</returns>
        bool IsEncrypted(string text);

        /// <summary>
        /// 解密数据库连接字符串（如果已加密）
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>解密后的连接字符串</returns>
        string DecryptConnectionStringIfNeeded(string connectionString);
    }
}
