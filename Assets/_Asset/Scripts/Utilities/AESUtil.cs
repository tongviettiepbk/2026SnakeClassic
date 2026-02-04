using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AESUtil
{
    // ⚠️ KEY phải đủ 32 byte (AES256). Có thể random rồi lưu base64 sau đó decode thành byte[].
    private static readonly string KEY = "a7D93kLm1Qw8E2xT5Cz9V4nH6Rj3YpBd";
    private static readonly string IV  = "G5t9K3s8Q2m1B7x4"; // 16 byte

    public static string EncryptAES(string plain)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(KEY);
        aes.IV  = Encoding.UTF8.GetBytes(IV);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] inputBytes = Encoding.UTF8.GetBytes(plain);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        return Convert.ToBase64String(encryptedBytes); // lưu dạng text trong .bytes
    }

    public static string DecryptAES(string encryptedBase64)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(KEY);
        aes.IV  = Encoding.UTF8.GetBytes(IV);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
