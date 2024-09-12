namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu设备
/// </summary>
public partial class ModbusRtuDevice : ObservableObject
{
    /// <summary>
    /// 从站地址
    /// </summary>
    [ObservableProperty]
    byte address;

    /// <summary>
    /// 波特率
    /// </summary>
    [ObservableProperty]
    BaudRate baudRate = BaudRate._9600;

    /// <summary>
    /// 校验位
    /// </summary>
    [ObservableProperty]
    Parity parity = Parity.None;

    /// <summary>
    /// 数据位
    /// </summary>
    [ObservableProperty]
    int dataBits = 8;

    /// <summary>
    /// 停止位
    /// </summary>
    [ObservableProperty]
    StopBits stopBits = StopBits.One;

    /// <summary>
    /// 接收的消息
    /// </summary>
    [ObservableProperty]
    string receiveMessage = string.Empty;
}
