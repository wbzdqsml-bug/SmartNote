using System.Security.Cryptography;
using System.Text;

namespace SmartNote.Common.Helpers
{
    public static class PasswordHasher
    {
        // 基本安全：用户名参与加盐，避免简单撞库；毕设场景足够用
        public static string Hash(string username, string password)
        {
            using var sha = SHA256.Create();
            // 统一小写，避免大小写变体造成的不同哈希
            var combined = $"{username.Trim().ToLowerInvariant()}::{password}";
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string username, string password, string storedHash)
            => Hash(username, password) == storedHash;
    }
}
