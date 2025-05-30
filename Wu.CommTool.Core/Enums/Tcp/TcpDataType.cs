namespace Wu.CommTool.Core.Enums.Tcp;

/// <summary>
/// Tcp数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum TcpDataType
{
    [Description("UTF-8")]
    Uft8,
    [Description("Hex")]
    Hex,
    [Description("Ascii")]
    Ascii,
    [Description("Unicode")]
    Unicode,
}
