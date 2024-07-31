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
        byte[] valuesToFind = [0x03, 0x04, 0x10];//先判断03 04功能码
        //byte[] valuesToFind = [0x01, 0x02, 0x03, 0x04, 0x0F, 0x10];//仅处理常用的功能码
        for (int i = 0; i < bytes.Count; i++)
        {
            if (valuesToFind.Contains(bytes[i]))
            {
                indices.Add(i);
            }
        }
        return indices;
    }

    /// <summary>
    /// 数据类型对应的字节数
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static int GetMrtuDataTypeLength(MrtuDataType dataType)
    {
        return dataType switch
        {
            //MrtuDataType.Byte=>1,
            MrtuDataType.uShort => 2,
            MrtuDataType.Short => 2,
            MrtuDataType.uInt => 4,
            MrtuDataType.Int => 4,
            MrtuDataType.uLong => 8,
            MrtuDataType.Long => 8,
            MrtuDataType.Float => 4,
            MrtuDataType.Double => 8,
            MrtuDataType.Hex => 2,
            //case DataType.Bool:
            //    return 1;
            _ => 1,
        };
    }

    /// <summary>
    /// 字节序转换
    /// </summary>
    /// <param name="val"></param>
    /// <param name="byteOrder"></param>
    /// <returns></returns>
    public static byte[] ByteOrder(byte[] val, ModbusByteOrder byteOrder)
    {
        //若为单字节的则直接返回
        if (val.Length <= 1)
        {
            return val;
        }
        //字节序处理
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
                return val;
            case ModbusByteOrder.DCBA:
                return val.Reverse().ToArray();
            default:
                return val;
        }
    }

}
