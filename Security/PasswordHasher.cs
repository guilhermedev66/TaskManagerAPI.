using System.Security.Cryptography;
using System.Text;

namespace TaskManagerAPI.Security
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
