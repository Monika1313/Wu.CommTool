using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Wu.CommTool.Modules.CryptoTool.ViewModels;

internal static class CryptoAlgorithms
{
    internal enum Sm4CipherMode
    {
        Cbc,
        Ecb
    }

    internal enum Sm4PaddingMode
    {
        Pkcs7,
        None
    }

    internal enum Sm4TextFormat
    {
        Utf8,
        Hex,
        Base64
    }

    internal enum Sm4KeyFormat
    {
        Md5OfText,
        Hex16
    }

    public static string Base64Encrypt(string plainText)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    public static string Base64Decrypt(string cipherText)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
    }

    public static string EncryptAes(string plainText, string key, string iv)
    {
        using Aes aes = CreateAes(key, iv);
        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter writer = new(cryptoStream, Encoding.UTF8);
        writer.Write(plainText);
        writer.Flush();
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string DecryptAes(string cipherText, string key, string iv)
    {
        byte[] buffer = Convert.FromBase64String(cipherText);
        using Aes aes = CreateAes(key, iv);
        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader reader = new(cryptoStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string EncryptSm4(string plainText, string key, string iv)
    {
        return EncryptSm4(plainText, key, iv, Sm4CipherMode.Cbc, Sm4PaddingMode.Pkcs7, Sm4TextFormat.Utf8, Sm4TextFormat.Base64, Sm4KeyFormat.Md5OfText);
    }

    public static string DecryptSm4(string cipherText, string key, string iv)
    {
        return DecryptSm4(cipherText, key, iv, Sm4CipherMode.Cbc, Sm4PaddingMode.Pkcs7, Sm4TextFormat.Utf8, Sm4TextFormat.Base64, Sm4KeyFormat.Md5OfText);
    }

    public static string EncryptSm4(
        string plainText,
        string key,
        string iv,
        Sm4CipherMode cipherMode,
        Sm4PaddingMode paddingMode,
        Sm4TextFormat plainFormat,
        Sm4TextFormat cipherFormat,
        Sm4KeyFormat keyFormat)
    {
        byte[] inputBytes = ParseByFormat(plainText, plainFormat);
        IBufferedCipher cipher = CreateSm4Cipher(true, key, iv, cipherMode, paddingMode, keyFormat);
        byte[] result = DoFinal(cipher, inputBytes);
        return FormatByType(result, cipherFormat);
    }

    public static string DecryptSm4(
        string cipherText,
        string key,
        string iv,
        Sm4CipherMode cipherMode,
        Sm4PaddingMode paddingMode,
        Sm4TextFormat plainFormat,
        Sm4TextFormat cipherFormat,
        Sm4KeyFormat keyFormat)
    {
        byte[] inputBytes = ParseByFormat(cipherText, cipherFormat);
        IBufferedCipher cipher = CreateSm4Cipher(false, key, iv, cipherMode, paddingMode, keyFormat);
        byte[] result = DoFinal(cipher, inputBytes);
        return FormatByType(result, plainFormat);
    }

    private static Aes CreateAes(string key, string iv)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required in AES mode.");
        }

        using SHA256 sha256 = SHA256.Create();
        byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

        using MD5 md5 = MD5.Create();
        byte[] ivBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(iv) ? key : iv));

        Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = keyBytes;
        aes.IV = ivBytes;
        return aes;
    }

    private static IBufferedCipher CreateSm4Cipher(bool forEncrypt, string key, string iv, Sm4CipherMode cipherMode, Sm4PaddingMode paddingMode, Sm4KeyFormat keyFormat)
    {
        byte[] keyBytes = GetSm4KeyBytes(key, keyFormat);

        string mode = cipherMode == Sm4CipherMode.Ecb ? "ECB" : "CBC";
        string padding = paddingMode == Sm4PaddingMode.None ? "NoPadding" : "PKCS7Padding";
        IBufferedCipher cipher = CipherUtilities.GetCipher($"SM4/{mode}/{padding}");

        ICipherParameters parameters = cipherMode == Sm4CipherMode.Ecb
            ? new KeyParameter(keyBytes)
            : new ParametersWithIV(new KeyParameter(keyBytes), GetSm4IvBytes(key, iv, keyFormat));

        cipher.Init(forEncrypt, parameters);
        return cipher;
    }

    private static byte[] GetSm4KeyBytes(string key, Sm4KeyFormat keyFormat)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required in SM4 mode.");
        }

        if (keyFormat == Sm4KeyFormat.Hex16)
        {
            byte[] keyBytes = ParseHex(key);
            if (keyBytes.Length != 16)
            {
                throw new ArgumentException("SM4 key must be 16 bytes (32 hex chars).");
            }

            return keyBytes;
        }

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(key);
        if (utf8Bytes.Length == 16)
        {
            return utf8Bytes;
        }

        using MD5 md5 = MD5.Create();
        return md5.ComputeHash(utf8Bytes);
    }

    private static byte[] GetSm4IvBytes(string key, string iv, Sm4KeyFormat keyFormat)
    {
        if (keyFormat == Sm4KeyFormat.Hex16)
        {
            if (string.IsNullOrWhiteSpace(iv))
            {
                throw new ArgumentException("IV is required when key format is Hex16.");
            }

            byte[] ivBytes = ParseHex(iv);
            if (ivBytes.Length != 16)
            {
                throw new ArgumentException("SM4 IV must be 16 bytes (32 hex chars).");
            }

            return ivBytes;
        }

        string source = string.IsNullOrWhiteSpace(iv) ? key : iv;
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(source);
        if (utf8Bytes.Length == 16)
        {
            return utf8Bytes;
        }

        using MD5 md5 = MD5.Create();
        return md5.ComputeHash(utf8Bytes);
    }

    private static byte[] ParseByFormat(string text, Sm4TextFormat format)
    {
        return format switch
        {
            Sm4TextFormat.Utf8 => Encoding.UTF8.GetBytes(text ?? string.Empty),
            Sm4TextFormat.Hex => ParseHex(text),
            Sm4TextFormat.Base64 => Convert.FromBase64String(text ?? string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static string FormatByType(byte[] bytes, Sm4TextFormat format)
    {
        return format switch
        {
            Sm4TextFormat.Utf8 => Encoding.UTF8.GetString(bytes),
            Sm4TextFormat.Hex => ToHexString(bytes),
            Sm4TextFormat.Base64 => Convert.ToBase64String(bytes),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static byte[] ParseHex(string text)
    {
        string value = NormalizeHexText(text);
        if (value.Length % 2 != 0)
        {
            throw new ArgumentException("Hex text length must be even.");
        }

        try
        {
            byte[] bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
        catch
        {
            throw new ArgumentException("Invalid hex text.");
        }
    }

    private static string ToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    private static string NormalizeHexText(string? text)
    {
        string value = (text ?? string.Empty)
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring(2);
        }

        return value;
    }

    private static byte[] DoFinal(IBufferedCipher cipher, byte[] input)
    {
        byte[] output = new byte[cipher.GetOutputSize(input.Length)];
        int length = cipher.ProcessBytes(input, 0, input.Length, output, 0);
        length += cipher.DoFinal(output, length);

        if (length == output.Length)
        {
            return output;
        }

        byte[] result = new byte[length];
        Buffer.BlockCopy(output, 0, result, 0, length);
        return result;
    }
}
