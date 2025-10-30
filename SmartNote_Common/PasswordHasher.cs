using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_Common
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000; // 迭代次数
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        /// <summary>
        /// 哈希一个明文密码
        /// </summary>
        public string Hash(string password)
        {
            // 1. 生成一个随机盐 (Salt)
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            // 2. 使用 Salt 和迭代次数计算哈希
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                _hashAlgorithmName,
                KeySize);

            // 3. 将 Salt 和 Hash 组合成一个字符串存储
            return $"{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// 验证明文密码是否与存储的哈希匹配
        /// </summary>
        public bool Verify(string passwordHash, string inputPassword)
        {
            try
            {
                // 1. 从存储的字符串中分离 Salt 和 Hash
                var elements = passwordHash.Split(Delimiter);
                var salt = Convert.FromBase64String(elements[0]);
                var hash = Convert.FromBase64String(elements[1]);

                // 2. 使用相同的 Salt 和迭代次数计算输入密码的哈希
                var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
                    inputPassword,
                    salt,
                    Iterations,
                    _hashAlgorithmName,
                    KeySize);

                // 3. 比较两个哈希是否完全一致
                return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
            }
            catch (Exception)
            {
                // (例如格式错误、Base64 解码失败等)
                return false; // 验证失败
            }
        }
    }
}
