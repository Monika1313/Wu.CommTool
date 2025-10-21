namespace Wu.CommTool.Core.Enums.Tcp;

/// <summary>
/// Tcp数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum TcpDataType
{
    [Description("UTF-8")]
    UTF8,
    [Description("Hex")]
    HEX,
    [Description("Ascii")]
    ASCII,
    [Description("Unicode")]
    Unicode,
}
