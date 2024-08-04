namespace Wu.CommTool.Core.Enums.Modbus;

/// <summary>
/// 寄存器类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum RegisterType
{
    [Description("保持寄存器")]
    Holding,
    [Description("输入寄存器")]
    Input
}