using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Wu.CommTool.Modules.CryptoTool.Enums;
using CryptoCipherMode = Wu.CommTool.Modules.CryptoTool.Enums.CipherMode;
using CryptoPaddingMode = Wu.CommTool.Modules.CryptoTool.Enums.PaddingMode;

namespace Wu.CommTool.Modules.CryptoTool.Services;

public static class Sm4Cryptography
{
    public static string Encrypt(string plainText, string key, string iv)
    {
        return Encrypt(plainText, key, iv, CryptoCipherMode.CBC, CryptoPaddingMode.PKCS7, PlainFormat.Utf8, CipherFormat.Base64, KeyFormat.Text);
    }

    public static string Decrypt(string cipherText, string key, string iv)
    {
        return Decrypt(cipherText, key, iv, CryptoCipherMode.CBC, CryptoPaddingMode.PKCS7, PlainFormat.Utf8, CipherFormat.Base64, KeyFormat.Text);
    }

    public static string Encrypt(string plainText, Sm4CryptoConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        return Encrypt(
            plainText,
            config.Key,
            config.Iv,
            config.CipherMode,
            config.PaddingMode,
            config.PlainFormat,
            config.CipherFormat,
            config.KeyFormat);
    }

    public static string Decrypt(string cipherText, Sm4CryptoConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        return Decrypt(
            cipherText,
            config.Key,
            config.Iv,
            config.CipherMode,
            config.PaddingMode,
            config.PlainFormat,
            config.CipherFormat,
            config.KeyFormat);
    }

    public static string Encrypt(
        string plainText,
        string key,
        string iv,
        CryptoCipherMode cipherMode,
        CryptoPaddingMode paddingMode,
        PlainFormat plainFormat,
        CipherFormat cipherFormat,
        KeyFormat keyFormat)
    {
        byte[] inputBytes = ParsePlain(plainText, plainFormat);
        IBufferedCipher cipher = CreateCipher(true, key, iv, cipherMode, paddingMode, keyFormat);
        byte[] result = DoFinal(cipher, inputBytes);
        return FormatCipher(result, cipherFormat);
    }

    public static string Decrypt(
        string cipherText,
        string key,
        string iv,
        CryptoCipherMode cipherMode,
        CryptoPaddingMode paddingMode,
        PlainFormat plainFormat,
        CipherFormat cipherFormat,
        KeyFormat keyFormat)
    {
        byte[] inputBytes = ParseCipher(cipherText, cipherFormat);
        IBufferedCipher cipher = CreateCipher(false, key, iv, cipherMode, paddingMode, keyFormat);
        byte[] result = DoFinal(cipher, inputBytes);
        return FormatPlain(result, plainFormat);
    }

    private static IBufferedCipher CreateCipher(bool forEncrypt, string key, string iv, CryptoCipherMode cipherMode, CryptoPaddingMode paddingMode, KeyFormat keyFormat)
    {
        byte[] keyBytes = GetKeyBytes(key, keyFormat);

        string mode = cipherMode == CryptoCipherMode.ECB ? "ECB" : "CBC";
        string padding = paddingMode == CryptoPaddingMode.None ? "NoPadding" : "PKCS7Padding";
        IBufferedCipher cipher = CipherUtilities.GetCipher($"SM4/{mode}/{padding}");

        ICipherParameters parameters = cipherMode == CryptoCipherMode.ECB
            ? new KeyParameter(keyBytes)
            : new ParametersWithIV(new KeyParameter(keyBytes), GetIvBytes(key, iv, keyFormat));

        cipher.Init(forEncrypt, parameters);
        return cipher;
    }

    private static byte[] GetKeyBytes(string key, KeyFormat keyFormat)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("SM4 密钥不能为空。");
        }

        if (keyFormat == KeyFormat.Hex)
        {
            byte[] keyBytes = ParseHex(key);
            if (keyBytes.Length != 16)
            {
                throw new ArgumentException("SM4 密钥必须是16字节(32位十六进制)。");
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

    private static byte[] GetIvBytes(string key, string iv, KeyFormat keyFormat)
    {
        if (keyFormat == KeyFormat.Hex)
        {
            if (string.IsNullOrWhiteSpace(iv))
            {
                throw new ArgumentException("Hex 密钥格式下，IV 不能为空。");
            }

            byte[] ivBytes = ParseHex(iv);
            if (ivBytes.Length != 16)
            {
                throw new ArgumentException("SM4 IV 必须是16字节(32位十六进制)。");
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

    private static byte[] ParsePlain(string text, PlainFormat format)
    {
        return format switch
        {
            PlainFormat.Utf8 => Encoding.UTF8.GetBytes(text ?? string.Empty),
            PlainFormat.Hex => ParseHex(text),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static byte[] ParseCipher(string text, CipherFormat format)
    {
        return format switch
        {
            CipherFormat.Hex => ParseHex(text),
            CipherFormat.Base64 => Convert.FromBase64String(text ?? string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static string FormatPlain(byte[] bytes, PlainFormat format)
    {
        return format switch
        {
            PlainFormat.Utf8 => Encoding.UTF8.GetString(bytes),
            PlainFormat.Hex => ToHexString(bytes),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static string FormatCipher(byte[] bytes, CipherFormat format)
    {
        return format switch
        {
            CipherFormat.Hex => ToHexString(bytes),
            CipherFormat.Base64 => Convert.ToBase64String(bytes),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static byte[] ParseHex(string? text)
    {
        string value = NormalizeHexText(text);
        if (value.Length % 2 != 0)
        {
            throw new ArgumentException("十六进制长度必须为偶数。");
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
            throw new ArgumentException("十六进制内容无效。");
        }
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

    private static string ToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
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
