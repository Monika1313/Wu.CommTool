using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wu.ComTool.Converters;

namespace Wu.ComTool.Models
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
