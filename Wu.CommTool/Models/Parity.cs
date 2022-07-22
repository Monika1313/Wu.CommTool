using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wu.CommTool.Converters;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 校验
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Parity
    {
        [Description("None 无校验")]
        None = 0,
        [Description("Odd 奇校验")]
        Odd = 1,
        [Description("Even 偶校验")]
        Even = 2,
        [Description("Mark 固定1")]
        Mark = 3,
        [Description("Space 固定0")]
        Space = 4,
    }
}
