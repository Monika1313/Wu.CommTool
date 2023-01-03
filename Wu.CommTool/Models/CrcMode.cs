using System.ComponentModel;
using Wu.CommTool.Converters;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// Crc校验模式
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CrcMode : int
    {
        [Description("无")]
        None = 0,

        [Description("Modbus")]
        Modbus = 1
    }
}
