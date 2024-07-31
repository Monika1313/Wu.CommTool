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
    /// 名称
    /// </summary>
    [ObservableProperty]
    private string name = string.Empty;

    /// <summary>
    /// <summary>
    /// 寄存器类型
    /// </summary>
    [ObservableProperty]
    RegisterType registerType;

    /// 寄存器地址
    /// </summary>
    [ObservableProperty]
    private int registerAddr;

    /// 寄存器地址16进制
    /// </summary>
    [ObservableProperty]
    private int registerAddrHex;

    /// <summary>
    /// 数据类型需读取的长度 单位:字节
    /// </summary>
    public int MrtuDataTypeByteLength => ModbusUtils.GetMrtuDataTypeLengthForRead(MrtuDataType);

    /// <summary>
    /// 转换后的显示值
    /// </summary>
    [ObservableProperty]
    double value;

    /// <summary>
    /// 数据类型
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Value))]
    MrtuDataType mrtuDataType = MrtuDataType.Int;

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



    ///// <summary>
    ///// 原始数据字节数组
    ///// </summary>
    //[ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(Value))]
    //private byte[] originBytes;

    ///// <summary>
    ///// 根据字节数据 数据类型 获取值
    ///// </summary>
    ///// <param name="databytes"></param>
    ///// <param name="dataType"></param>
    ///// <param name="rate"></param>
    ///// <returns></returns>
    //public static dynamic GetVal(byte[] databytes, DataType dataType, double rate)
    //{
    //    if (databytes == null)
    //    {
    //        return 0;
    //    }

    //    return dataType switch
    //    {
    //        DataType.uShort => Math.Round(BitConverter.ToUInt16(databytes, 0) * rate, 3),
    //        DataType.Short => Math.Round(BitConverter.ToInt16(databytes, 0) * rate, 3),
    //        DataType.uInt => Math.Round(BitConverter.ToUInt32(databytes, 0) * rate, 3),
    //        DataType.Int => Math.Round(BitConverter.ToInt32(databytes, 0) * rate, 3),
    //        DataType.uLong => Math.Round(BitConverter.ToUInt64(databytes, 0) * rate, 3),
    //        DataType.Long => Math.Round(BitConverter.ToInt64(databytes, 0) * rate, 3),
    //        DataType.Float => Math.Round((BitConverter.ToSingle(databytes, 0) * rate), 2),
    //        DataType.Double => Math.Round(BitConverter.ToDouble(databytes, 0) * rate, 2),
    //        DataType.Hex => $"0x{BitConverter.ToString(databytes.Reverse().ToArray()).Replace("-", "")}",
    //        _ => (dynamic)0,
    //    };
    //}

}
