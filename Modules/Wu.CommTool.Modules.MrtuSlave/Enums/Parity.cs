namespace Wu.CommTool.Modules.MrtuSlave.Enums;

/// <summary>
/// 校验
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum Parity
{
    [Description("None   无校验")]
    None = 0,
    [Description("Odd    奇校验")]
    Odd = 1,
    [Description("Even   偶校验")]
    Even = 2,
    [Description("Mark   固定1")]
    Mark = 3,
    [Description("Space  固定0")]
    Space = 4,
}
