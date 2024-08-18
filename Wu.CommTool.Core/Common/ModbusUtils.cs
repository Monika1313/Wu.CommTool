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
    /// 字符串添加校验码
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string StrCombineCrcCode(string str)
    {
        var code = Wu.Utils.Crc.Crc16Modbus(str.GetBytes());
        //若校验码为0
        if (code.All(x => x == 0))
        {
            return str;
        }
        Array.Reverse(code);
        return $"{str}{BitConverter.ToString(code).Replace("-", string.Empty)}";
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
    /// 数据类型对应需读取的字节数
    /// </summary>
    /// <param name="mrtuDataType"></param>
    /// <returns></returns>
    public static int GetMrtuDataTypeLengthForRead(MrtuDataType mrtuDataType)
    {
        return mrtuDataType switch
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
    /// 字节序转换 输入字节序视为ABCD, 再根据输入字节序进行转换
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


    /// <summary>
    /// 获取当前设备的串口列表
    /// </summary>
    /// <returns></returns>
    public static List<ComPort> GetComPorts()
    {
        List<ComPort> ports = [];
        //查找Com口
        using System.Management.ManagementObjectSearcher searcher = new("select * from Win32_PnPEntity where Name like '%(COM[0-999]%'");
        var hardInfos = searcher.Get();
        //获取串口设备列表
        foreach (var hardInfo in hardInfos)
        {
            if (hardInfo.Properties["Name"].Value != null)
            {
                string deviceName = hardInfo.Properties["Name"].Value.ToString()!;         //获取名称
                List<string> portList = [];
                //从名称中截取串口编号
                foreach (Match mch in Regex.Matches(deviceName, @"COM\d{1,3}").Cast<Match>())
                {
                    string x = mch.Value.Trim();
                    portList.Add(x);
                }
                int startIndex = deviceName.IndexOf("(");
                string port = portList[0];
                string name = deviceName[..(startIndex - 1)];
                ports.Add(new ComPort(port, name));
            }
        }
        return ports;
    }


    /// <summary>
    /// 返回该数组是否Modbus校验通过
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public static bool IsModbusCrcOk(byte[] frame)
    {
        var code = Wu.Utils.Crc.Crc16Modbus(frame);

        //校验通过
        if (code.All(x => x == 0))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 验证帧是否通过CRC校验
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public static bool IsModbusCrcOk(List<byte> frame)
    {
        return IsModbusCrcOk(frame.ToArray());
    }




    #region 字节数组处理
    /// <summary>
    /// 截取字节数组
    /// </summary>
    /// <param name="input">字节数组</param>
    /// <param name="skip">起始位置</param>
    /// <param name="take">读取数量</param>
    /// <returns></returns>
    public static byte[] TakeBytes(byte[] input, int skip, int take)
    {
        return input.Skip(skip).Take(take).ToArray();
    }

    /// <summary>
    /// 从字节数据中截取 16位无符号整型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static ushort GetUInt16(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToUInt16(ConvertByteOrder(TakeBytes(data, p, 2), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从字节数据中截取 16位有符号整型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static short GetInt16(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToInt16(ConvertByteOrder(TakeBytes(data, p, 2), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从字节数据中截取 32位无符号整型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static uint GetUInt32(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToUInt32(ConvertByteOrder(TakeBytes(data, p, 4), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从字节数据中截取 32位有符号整型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static int GetInt(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToInt32(ConvertByteOrder(TakeBytes(data, p, 4), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从字节数组中指定位置读取float数据 32位浮点型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p">指示数据在数组中的起始位置</param>
    /// <returns></returns>
    public static float GetFloat(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToSingle(ConvertByteOrder(TakeBytes(data, p, 4), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从大端数组指定位置读取unsigned Long数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static ulong GetUInt64(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToUInt64(ConvertByteOrder(TakeBytes(data, p, 8), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从大端数组指定位置读取Long数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static long GetInt64(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToInt64(ConvertByteOrder(TakeBytes(data, p, 8), modbusByteOrder), 0);
    }

    /// <summary>
    /// 从大端数组指定位置读取Double数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="p"></param>
    /// /// <param name="modbusByteOrder"></param>
    /// <returns></returns>
    public static double GetDouble(byte[] data, int p, ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA)
    {
        return BitConverter.ToDouble(ConvertByteOrder(TakeBytes(data, p, 8), modbusByteOrder), 0);
    }
    #endregion
}
