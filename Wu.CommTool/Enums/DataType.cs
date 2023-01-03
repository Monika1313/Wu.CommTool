using System.ComponentModel;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Enums
{
    /// <summary>
    /// 数据类型
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DataType : int
    {
        //[Description("8位无符号整型")]
        //Byte,
        //[Description("8位有符号整型")]
        //Sint,

        [Description("uShort16 位无符号整型")]
        uShort,
        [Description("Short16 位有符号整型")]
        Short,

        [Description("uInt32 无符号位整型")]
        uInt,
        [Description("Int32 有符号位整型")]
        Int,

        [Description("uLong 64位无符号整型")]
        uLong,
        [Description("Long 64位有符号整型")]
        Long,

        [Description("Float 32位浮点型")]
        Float,
        [Description("Double 64位浮点型")]
        Double,

        //[Description("布尔")]
        //Bool,
        //[Description("字符串")]
        //String
        //[Description("16位BCD码")]
        //BCD16
    }
}
