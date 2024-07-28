namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu设备
/// </summary>
public partial class MrtuDevice : ObservableObject
{
    /// <summary>
    /// 设备名
    /// </summary>
    [ObservableProperty]
    string name = string.Empty;

    /// <summary>
    /// 通讯口
    /// </summary>
    [ObservableProperty]
    ComPort comPort;

    /// <summary>
    /// 备注
    /// </summary>
    [ObservableProperty]
    string remark;
}
