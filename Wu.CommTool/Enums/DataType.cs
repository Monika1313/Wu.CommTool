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
        [Description("字节")]
        Byte,

        [Description("无符号16位整型")]
        uShort,
        [Description("有符号16位整型")]
        Short,

        [Description("无符号32位整型")]
        uInt,
        [Description("有符号32位整型")]
        Int,

        [Description("无符号64位整型")]
        uLong,
        [Description("有符号64位整型")]
        Long,

        [Description("32位浮点型")]
        Float,
        [Description("64位浮点型")]
        Double,

        [Description("布尔")]
        Bool,
        //[Description("字符串")]
        //String
    }
}
