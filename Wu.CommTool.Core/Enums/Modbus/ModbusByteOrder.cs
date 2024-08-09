namespace Wu.CommTool.Core.Enums.Modbus;

/// <summary>
/// ModbusRtu字节序
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum ModbusByteOrder
{
    [Description("ABCD")]
    ABCD,
    [Description("BADC")]
    BADC,
    [Description("CDAB")]
    CDAB,
    [Description("DCBA")]
    DCBA
}
