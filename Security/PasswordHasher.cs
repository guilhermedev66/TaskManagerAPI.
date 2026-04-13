using System.Security.Cryptography;
using System.Text;

namespace TaskManagerAPI.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var payload = new byte[salt.Length + passwordBytes.Length];

            Buffer.BlockCopy(salt, 0, payload, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, payload, salt.Length, passwordBytes.Length);

            var hash = SHA256.HashData(payload);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string hash)
        {
            var parts = hash.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[0]);
                var expectedHash = Convert.FromBase64String(parts[1]);
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var payload = new byte[salt.Length + passwordBytes.Length];

                Buffer.BlockCopy(salt, 0, payload, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, payload, salt.Length, passwordBytes.Length);

                var actualHash = SHA256.HashData(payload);
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
