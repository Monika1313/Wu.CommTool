using System.ComponentModel;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Modules.ModbusRtu.Enums
{
    /// <summary>
    /// Modbus Rtu 异常码
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ModbusRtuErrorCode : byte
    {
        [Description("设备不支持该功能码")]
        _0x01 = 0x01,//设备不支持该功能码
        _0x02 = 0x02,//起始地址无效
        _0x03 = 0x03,//数量无效
        [Description("设备不支持该功能码")]
        _0x04 = 0x04,//设备执行请求出现异常
    }
}
