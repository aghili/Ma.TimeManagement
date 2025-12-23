using System.Security.Cryptography;
using System.Text;

namespace Ma.TimeManagement.Services
{
    public class PatEncryption : IPatEncryption
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Ma.TimeManagement-2025");

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;
            byte[] encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plainText),
                Entropy,
                DataProtectionScope.CurrentUser);  // only current OS user can decrypt
            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return null;
            byte[] decrypted = ProtectedData.Unprotect(
                Convert.FromBase64String(cipherText),
                Entropy,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
