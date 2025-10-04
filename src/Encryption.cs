using System.Security.Cryptography;
using System.Text;

namespace OdinVault;

public static class Encryption
{
    public static byte[] GenerateKey()
    {
        return RandomNumberGenerator.GetBytes(32);
    }
    
    public static byte[] GenerateIV()
    {
        return RandomNumberGenerator.GetBytes(16);
    }

    public static string HashPassword(string password)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
    
    public static (byte[] Key, byte[] Salt) GenerateKeyFromPassword(string password, byte[]? salt = null, int iterations = 10000)
    {
        salt ??= RandomNumberGenerator.GetBytes(16);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
        return (key, salt);
    }

    public static byte[] ExtractIV(string encrypted)
    {
        return ExtractIV(Convert.FromBase64String(encrypted));
    }
    
    public static byte[] ExtractIV(byte[] encrypted)
    {
        // Extract the IV from the beginning.
        var iv = new byte[16];
        Array.Copy(encrypted, 0, iv, 0, iv.Length);
        
        return iv;
    }
    
    public static string Encrypt(string plainText, string keyBase64, byte[]? iv = null)
    {
        return Encrypt(plainText, Convert.FromBase64String(keyBase64), iv);
    }

    public static string Encrypt(string plainText, byte[] key, byte[]? iv = null)
    {
        return Convert.ToBase64String(EncryptToBytes(plainText, key, iv));
    }
    
    public static byte[] EncryptToBytes(string plainText, byte[] key, byte[]? iv = null)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv ?? RandomNumberGenerator.GetBytes(16);

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length); // Write IV at the start of the stream.

        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return ms.ToArray();
    }

    public static string Decrypt(string encrypted, string keyBase64)
    {
        return Decrypt(encrypted, Convert.FromBase64String(keyBase64));
    }

    public static string Decrypt(string encrypted, byte[] key)
    {
        return Decrypt(Convert.FromBase64String(encrypted), key);
    }
    
    public static string Decrypt(byte[] encrypted, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = ExtractIV(encrypted);

        using var ms = new MemoryStream(encrypted, aes.IV.Length, encrypted.Length - aes.IV.Length);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}