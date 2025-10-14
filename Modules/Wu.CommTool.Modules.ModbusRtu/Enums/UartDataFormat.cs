namespace Wu.CommTool.Modules.ModbusRtu.Enums;

/// <summary>
/// 串口的收发数据格式
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum UartDataFormat
{
    /// <summary>
    /// ASCII文本格式
    /// </summary>
    Ascii,

    /// <summary>
    /// 十六进制格式
    /// </summary>
    Hex,
}