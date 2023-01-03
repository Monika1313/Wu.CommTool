using System.ComponentModel;
using Wu.CommTool.Converters;

namespace Wu.CommTool.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StopBits
    {
        [Description("1")]
        One = 1,
        [Description("2")]
        Two,
        [Description("1.5")]
        OnePointFive
    }
}
