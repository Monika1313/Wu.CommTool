namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu设备 管理
/// </summary>
public partial class MrtuDeviceManager : ObservableObject
{

    /// <summary>
    /// ModbusRtu设备列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MrtuDevice> mrtuDevices = [];

}
