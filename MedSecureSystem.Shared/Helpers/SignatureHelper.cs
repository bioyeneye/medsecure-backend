using System.Security.Cryptography;
using System.Text;

namespace MedSecureSystem.Shared.Helpers
{
    public static class EncryptionHelper
    {
        // Encrypts the secret using a key derived from the clientId
        public static string EncryptSecret(string secret, string clientId)
        {
            var key = GetKey(clientId);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream))
            {
                writer.Write(secret);
            }

            var iv = aes.IV;
            var encryptedContent = memoryStream.ToArray();
            var result = new byte[iv.Length + encryptedContent.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

            return Convert.ToBase64String(result);
        }

        // Decrypts the encrypted secret using a key derived from the clientId
        public static string DecryptSecret(string encryptedSecret, string clientId)
        {
            var combined = Convert.FromBase64String(encryptedSecret);
            var key = GetKey(clientId);

            using var aes = Aes.Create();
            aes.Key = key;

            var iv = new byte[aes.BlockSize / 8];
            var encryptedContent = new byte[combined.Length - iv.Length];
            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combined, iv.Length, encryptedContent, 0, encryptedContent.Length);

            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(encryptedContent);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }

        private static byte[] GetKey(string clientId)
        {
            // Key derivation using SHA256
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(clientId));
        }
    }

}
