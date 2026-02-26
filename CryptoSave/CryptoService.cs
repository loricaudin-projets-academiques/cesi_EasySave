using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoSave
{
    /// <summary>
    /// File encryption service using AES-256.
    /// </summary>
    class CryptoService
    {
        /// <summary>
        /// Default AES-256 encryption key (32 bytes).
        /// </summary>
        private static readonly byte[] DefaultKey =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        };

        /// <summary>
        /// Fixed salt for PBKDF2 derivation (deterministic encryption across runs).
        /// </summary>
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("EasySave_CryptoSoft_Salt_2025");

        private readonly byte[] _key;

        /// <summary>
        /// Creates a CryptoService with an optional password.
        /// If null or empty, the default key is used.
        /// Otherwise, the key is derived from the password using PBKDF2.
        /// </summary>
        /// <param name="password">Password to derive the AES-256 key from, or null for default.</param>
        public CryptoService(string? password = null)
        {
            _key = DeriveKey(password) ?? DefaultKey;
        }

        private static byte[]? DeriveKey(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            using var pbkdf2 = new Rfc2898DeriveBytes(password, Salt, 100_000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        /// <summary>
        /// Encrypts a file in-place using AES-256.
        /// </summary>
        /// <param name="filePath">Path to the file to encrypt.</param>
        public void Encrypt(string filePath)
        {
            // Read file content
            byte[] fileData = File.ReadAllBytes(filePath);

            // Configure AES-256
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = _key;
            aes.GenerateIV();  // Generate random initialization vector

            // Write to file
            using FileStream fs = new FileStream(filePath, FileMode.Create);

            // Write IV at the beginning of file (required for decryption)
            fs.Write(aes.IV, 0, aes.IV.Length);

            // Encrypt and write data
            using CryptoStream cryptoStream =
                new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(fileData, 0, fileData.Length);
            cryptoStream.FlushFinalBlock();
        }
    }
}
