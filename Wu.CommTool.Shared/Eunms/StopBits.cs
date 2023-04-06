using System.ComponentModel;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Shared.Enums
{
    /// <summary>
    /// Modbus停止位
    /// </summary>
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
