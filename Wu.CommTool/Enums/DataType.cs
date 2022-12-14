using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Description("uShort16位无符号整型")]
        uShort,
        [Description("Short16位有符号整型")]
        Short,

        [Description("uInt32无符号位整型")]
        uInt,
        [Description("Int32有符号位整型")]
        Int,

        [Description("uLong64位无符号整型")]
        uLong,
        [Description("Long64位有符号整型")]
        Long,

        [Description("Float32位浮点型")]
        Float,
        [Description("Double64位浮点型")]
        Double,

        //[Description("布尔")]
        //Bool,
        //[Description("字符串")]
        //String
        //[Description("16位BCD码")]
        //BCD16
    }
}
