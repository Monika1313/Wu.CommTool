namespace Wu.CommTool.Modules.ModbusRtu.Enums;

/// <summary>
/// 波特率
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum BaudRate : int
{
    [Description("300")]
    _300 = 300,
    [Description("600")]
    _600 = 600,
    [Description("1200")]
    _1200 = 1200,
    [Description("2400")]
    _2400 = 2400,
    [Description("4800")]
    _4800 = 4800,
    [Description("9600")]
    _9600 = 9600,
    [Description("14400")]
    _14400 = 14400,
    [Description("19200")]
    _19200 = 19200,
    [Description("38400")]
    _38400 = 38400,
    [Description("56000")]
    _56000 = 56000
}
