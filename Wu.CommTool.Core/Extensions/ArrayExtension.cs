namespace Wu.CommTool.Core.Extensions;

/// <summary>
/// 数组扩展方法
/// </summary>
public static class ArrayExtension
{
    /// <summary>
    /// 字节数组转换为16进制字符串形式
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static string ToHexString(this byte[] arr)
    {
        return BitConverter.ToString(arr).Replace('-', ' ');
    }
}
