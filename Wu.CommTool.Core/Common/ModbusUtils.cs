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


    /// <summary>
    /// 数据格式化 每间隔几个字符插入1个空格
    /// </summary>
    /// <param name="input"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static string DataFormat(this byte[] input, int interval = 4)
    {
        return BitConverter.ToString(input).Replace("-", "").InsertFormat(interval, " ");
    }


    /// <summary>
    /// 获取字节数组中的功能码位置
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static List<int> GetIndicesOfFunctions(List<byte> bytes)
    {
        List<int> indices = [];
        //byte[] valuesToFind = [0x03,0x04];//先判断03 04功能码
        byte[] valuesToFind = [0x01, 0x02, 0x03, 0x04, 0x0F, 0x10];//仅处理常用的功能码
        for (int i = 0; i < bytes.Count; i++)
        {
            if (valuesToFind.Contains(bytes[i]))
            {
                indices.Add(i);
            }
        }
        return indices;
    }
}
