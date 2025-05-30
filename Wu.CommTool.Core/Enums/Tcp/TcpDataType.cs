namespace Wu.CommTool.Core.Enums.Tcp;

/// <summary>
/// Tcp数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum TcpDataType
{
    [Description("Ascii")]
    Ascii = 0,
    [Description("Hex")]
    Hex = 1,
    [Description("UTF-8")]
    Uft8 = 2,
    //[Description("GB2312")]
    //GB2312 = 3,
    [Description("Unicode")]
    Unicode,
}
