﻿namespace Wu.CommTool.Core.Common;

public static class Utils
{
    #region 判断16进制字符串
    /// <summary>
    /// 判断是否仅含16进制字符
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsHexString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;
        string val = str!.Replace(" ", "");
        return Regex.IsMatch(val, "^[0-9A-Fa-f]+$");//验证字符串仅包含16进制字符 至少一个字符
    }

    /// <summary>
    /// 判断是否为16位(包含1-4位16进制字符)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool Is16BitHexString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;
        string val = str.Replace(" ", "");
        return Regex.IsMatch(val, "^[0-9A-Fa-f]{1,4}$");//验证字符串仅包含1-4个16进制字符
    }

    /// <summary>
    /// 判断是否为包含32位(包含1-8位16进制字符)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool Is32BitHexString(string str)
    {
        string val = str.Replace(" ", "");
        //验证字符串仅包含1-8个16进制字符
        if (Regex.IsMatch(val, "^[0-9A-Fa-f]{1,8}$"))
            return true;
        return false;
    }

    /// <summary>
    /// 判断是否为64位(包含1-8位16进制字符)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool Is64BitHexString(string str)
    {
        string val = str.Replace(" ", "");
        //验证字符串仅包含1-16个16进制字符
        if (Regex.IsMatch(val, "^[0-9A-Fa-f]{1,16}$"))
            return true;
        return false;
    }
    #endregion


    #region 16进制字节序变换
    public static string ConvertByteOrder(string str, ModbusByteOrder byteOrder)
    {
        var aa = str.GetBytes();
        var xx = ConvertByteOrder(str.GetBytes(), byteOrder);
        return BitConverter.ToString(ConvertByteOrder(str.GetBytes(), byteOrder)).Replace("-", string.Empty);
    }

    /// <summary>
    /// 字节序变换
    /// </summary>
    /// <param name="val"></param>
    /// <param name="byteOrder"></param>
    /// <returns></returns>
    public static byte[] ConvertByteOrder(byte[] val, ModbusByteOrder byteOrder)
    {
        //若为单字节的则直接返回
        if (val.Length <= 1)
        {
            return val;
        }
        switch (byteOrder)
        {
            case ModbusByteOrder.ABCD:
                return val;
            case ModbusByteOrder.BADC:
                byte[] re = new byte[val.Length];
                for (int i = 0; i < val.Length; i++)
                {
                    byte item = val[i];
                    if (i % 2 == 1)
                    {
                        re[i - 1] = item;
                    }
                    else
                    {
                        re[i + 1] = item;
                    }
                }
                return re;
            case ModbusByteOrder.CDAB:
                var temp = val.Reverse().ToArray();
                byte[] result = new byte[temp.Length];
                for (int i = 0; i < temp.Length; i++)
                {
                    byte item = temp[i];
                    if (i % 2 == 1)
                    {
                        result[i - 1] = item;
                    }
                    else
                    {
                        result[i + 1] = item;
                    }
                }
                return result;
            case ModbusByteOrder.DCBA:
                return val.Reverse().ToArray();
            default:
                return val;
        }
    }
    #endregion


    /// <summary>
    /// 将序列化的json字符串内容写入Json文件，并且保存
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="jsonConents">Json内容</param>
    public static void WriteJsonFile(string path, string jsonConents)
    {
        //清除旧的文本
        using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write))
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
        }
        using FileStream fs = new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        using StreamWriter sw = new(fs, Encoding.UTF8);
        sw.WriteLine(jsonConents);
    }

    /// <summary>
    /// 获取到本地的Json文件并且解析返回对应的json字符串
    /// </summary>
    /// <param name="filepath">文件路径</param>
    /// <returns>Json内容</returns>
    public static string ReadJsonFile(string filepath)
    {
        string json = string.Empty;
        using (FileStream fs = new(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            using StreamReader sr = new(fs, Encoding.UTF8);
            json = sr.ReadToEnd().ToString();
        }
        return json;
    }

}
