using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AES256Encryption
{
    private static readonly string aesKey = "$$$%%%0y1d1o2o3b11A%%%$$$";
    public static string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            // Ensure the key length is 32 bytes for AES-256
            aesAlg.Key = GetValidKey(aesKey);
            aesAlg.IV = new byte[16]; // 16 bytes for AES block size

            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                // Convert to base64 and replace '+' with '_', '/' with '_', and remove '=' padding
                string base64 = Convert.ToBase64String(encryptedBytes);
                return base64.Replace('+', '_').Replace('/', '_').Replace("=", "");
            }
        }
    }

    public static string Decrypt(string encryptedText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            // Ensure the key length is 32 bytes for AES-256
            aesAlg.Key = GetValidKey(aesKey);
            aesAlg.IV = new byte[16]; // 16 bytes for AES block size

            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                // Reverse the encoding (replace '_' with '+', add padding if necessary)
                string base64 = encryptedText.Replace('_', '+');

                // Add padding back to the base64 string if it's missing
                int paddingLength = base64.Length % 4;
                if (paddingLength > 0)
                {
                    base64 = base64.PadRight(base64.Length + (4 - paddingLength), '=');
                }

                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(base64);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
                catch (FormatException ex)
                {
                    throw new Exception("The input is not a valid Base-64 string.", ex);
                }
            }
        }
    }

    private static byte[] GetValidKey(string key)
    {
        // Ensure the key is 32 bytes long (AES-256 requires 32 bytes)
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        // If the key is too short, pad it with spaces
        if (keyBytes.Length < 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        // If the key is too long, truncate it to 32 bytes
        else if (keyBytes.Length > 32)
        {
            Array.Resize(ref keyBytes, 32);
        }

        // Ensure the key does not contain restricted characters (+, /, -)
        string keyString = Encoding.UTF8.GetString(keyBytes);
        keyString = keyString.Replace('+', '_').Replace('/', '_').Replace('-', '_');

        // Convert back to bytes and ensure it's exactly 32 bytes
        return Encoding.UTF8.GetBytes(keyString.Substring(0, 32));
    }

}
