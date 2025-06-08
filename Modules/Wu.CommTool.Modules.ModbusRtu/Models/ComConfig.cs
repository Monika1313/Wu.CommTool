namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// 串口配置
/// </summary>
public partial class ComConfig : ObservableObject
{
    /// <summary>
    /// Com口
    /// </summary>
    [ObservableProperty] ComPort comPort = new();

    /// <summary>
    /// 波特率
    /// </summary>
    [ObservableProperty] BaudRate baudRate = BaudRate._9600;

    /// <summary>
    /// 校验
    /// </summary>
    [ObservableProperty] Parity parity = Parity.None;

    /// <summary>
    /// 数据位
    /// </summary>
    [ObservableProperty] int dataBits = 8;

    /// <summary>
    /// 停止位
    /// </summary>
    [ObservableProperty] StopBits stopBits = StopBits.One;

    /// <summary>
    /// 是否已打开
    /// </summary>
    [ObservableProperty] [property: JsonIgnore] bool isOpened = false;

    /// <summary>
    /// 是否处于接收数据状态
    /// </summary>
    [ObservableProperty] [property: JsonIgnore] bool isReceiving = false;

    /// <summary>
    /// 是否处于发送数据状态
    /// </summary>
    [ObservableProperty] [property: JsonIgnore] bool isSending = false;

    /// <summary>
    /// 分包超时时间
    /// </summary>
    [ObservableProperty] int timeOut = 50;

    /// <summary>
    /// 分包最大字节
    /// </summary>
    [ObservableProperty] int maxLength = 500;

    /// <summary>
    /// 自动搜索设备的间隔 单位ms
    /// </summary>
    [ObservableProperty] int searchInterval = 100;

    /// <summary>
    /// 自动分帧 对于连续时间间隔较短的帧,可自动识别正确的帧起止位置
    /// </summary>
    [ObservableProperty] Enable autoFrame = Enable.启用;
}
