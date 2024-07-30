namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu采集的数据
/// </summary>
public partial class MrtuData : ObservableObject
{
    /// <summary>
    /// 通讯状态
    /// </summary>
    
    public bool Status => UpdateTime != null && ((DateTime)UpdateTime).AddSeconds(30) < DateTime.Now;

    /// <summary>
    /// 寄存器类型
    /// </summary>
    [ObservableProperty]
    RegisterType registerType;

    /// <summary>
    /// 名称
    /// </summary>
    [ObservableProperty]
    private string name = string.Empty;

    /// <summary>
    /// 数据地址
    /// </summary>
    [ObservableProperty]
    private int dataAddr;

    /// <summary>
    /// 转换后的显示值
    /// </summary>
    [JsonIgnore]
    public dynamic Value=> GetVal(DataBytes, Type, Rate);

    /// <summary>
    /// 数据类型
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Value))]
    DataType type = DataType.uShort;

    /// <summary>
    /// 数据更新时间
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    private DateTime? updateTime = null;

    /// <summary>
    /// 倍率
    /// </summary>
    [ObservableProperty]
    private double rate = 1;

    /// <summary>
    /// 单位
    /// </summary>
    [ObservableProperty]
    private string unit = string.Empty;

    /// <summary>
    /// 待写入的值
    /// </summary>
    [ObservableProperty]
    private string writeValue;

    /// <summary>
    /// 数据类型的长度 单位=字节
    /// </summary>
    public int DataTypeByteLength => GetDataTypeLength(Type);

    /// <summary>
    /// 原始数据字节数组
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Value))]
    private byte[] originBytes;

    ////当前值的原始字节数组(已转换字节序)
    //[JsonIgnore]
    public byte[] DataBytes { get; set; } /*=> GetDataBytes(OriginBytes, Location, ModbusByteOrder, Type);*/

    ////高位在前 低位在后
    //[JsonIgnore]
    //public string DataHex => DataBytes is null ? "" : BitConverter.ToString(DataBytes.Reverse().ToArray()).Replace('-', ' ');

    //public override string ToString() => DataHex;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Value))]
    private ModbusByteOrder _ModbusByteOrder;


    /// <summary>
    /// 截取原始字节数组
    /// </summary>
    /// <param name="origin">字节数组</param>
    /// <param name="skip">起始位置</param>
    /// <param name="byteOrder">字节序</param>
    /// <param name="dataType">数据类型</param>
    /// <returns></returns>
    public static dynamic GetDataBytes(byte[] origin, int skip, ModbusByteOrder byteOrder, DataType dataType)
    {
        if (origin == null)
        {
            return null;
        }
        //从源数组中读取数据
        byte[] arr = origin.Skip(skip).Take(GetDataTypeLength(dataType)).ToArray();
        //进行字节序转换
        arr = ByteOrder(arr, byteOrder);
        return arr;
    }

    /// <summary>
    /// 根据字节数据 数据类型 获取值
    /// </summary>
    /// <param name="databytes"></param>
    /// <param name="dataType"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static dynamic GetVal(byte[] databytes, DataType dataType, double rate)
    {
        if (databytes == null)
        {
            return 0;
        }

        return dataType switch
        {
            DataType.uShort => Math.Round(BitConverter.ToUInt16(databytes, 0) * rate, 3),
            DataType.Short => Math.Round(BitConverter.ToInt16(databytes, 0) * rate, 3),
            DataType.uInt => Math.Round(BitConverter.ToUInt32(databytes, 0) * rate, 3),
            DataType.Int => Math.Round(BitConverter.ToInt32(databytes, 0) * rate, 3),
            DataType.uLong => Math.Round(BitConverter.ToUInt64(databytes, 0) * rate, 3),
            DataType.Long => Math.Round(BitConverter.ToInt64(databytes, 0) * rate, 3),
            DataType.Float => Math.Round((BitConverter.ToSingle(databytes, 0) * rate), 2),
            DataType.Double => Math.Round(BitConverter.ToDouble(databytes, 0) * rate, 2),
            DataType.Hex => $"0x{BitConverter.ToString(databytes.Reverse().ToArray()).Replace("-", "")}",
            _ => (dynamic)0,
        };
    }

    /// <summary>
    /// 数据类型对应的字节数
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static int GetDataTypeLength(DataType dataType)
    {
        return dataType switch
        {
            //case DataType.Byte:
            //    return 1;
            //case DataType.Sint:
            //    return 1;
            DataType.uShort => 2,
            DataType.Short => 2,
            DataType.uInt => 4,
            DataType.Int => 4,
            DataType.uLong => 8,
            DataType.Long => 8,
            DataType.Float => 4,
            DataType.Double => 8,
            DataType.Hex =>2,
            //case DataType.Bool:
            //    return 1;
            _ => 1,
        };
    }

    ///// <summary>
    ///// 从大端数组中指定位置读取short数据
    ///// </summary>
    ///// <param name="data"></param>
    ///// <param name="p"></param>
    ///// <returns></returns>
    //public static ushort GetUInt16FromBigEndianBytes(byte[] data, int p)
    //{
    //    return BitConverter.ToUInt16(SmallBigConvert(data, p, 2), 0);
    //}


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
