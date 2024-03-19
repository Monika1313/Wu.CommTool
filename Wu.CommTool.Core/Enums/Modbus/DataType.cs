namespace Wu.CommTool.Core.Enums.Modbus;

/// <summary>
/// 数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum DataType
{
    //[Description("8位无符号整型")]
    //Byte,
    //[Description("8位有符号整型")]
    //Sint,

    [Description("uShort 16位无符号整型")]
    uShort = 0,
    [Description("Short 16位有符号整型")]
    Short = 1,

    [Description("uInt 32无符号位整型")]
    uInt = 2,
    [Description("Int 32有符号位整型")]
    Int = 3,

    [Description("uLong 64位无符号整型")]
    uLong = 4,
    [Description("Long 64位有符号整型")]
    Long = 5,

    [Description("Float 32位浮点型")]
    Float = 6,

    [Description("Double 64位浮点型")]
    Double = 7,

    [Description("Hex 16位16进制字符")]
    Hex = 8,



    //[Description("布尔")]
    //Bool,
    //[Description("字符串")]
    //String
    //[Description("16位BCD码")]
    //BCD16
}
