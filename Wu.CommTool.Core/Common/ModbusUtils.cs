namespace Wu.CommTool.Core.Common;

public static class ModbusUtils
{
    // 将十六进制字符串转换为字节数组
    public static byte[] FromHex(string hexString)
    {
        hexString = hexString.Replace(" ", string.Empty);
        byte[] data = new byte[hexString.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }
        return data;
    }
}
