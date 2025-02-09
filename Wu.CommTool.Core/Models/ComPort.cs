namespace Wu.CommTool.Core.Models;

/// <summary>
/// 串口类
/// </summary>
public partial class ComPort : ObservableObject
{
    public ComPort()
    {

    }

    public ComPort(string port, string deviceName)
    {
        Port = port;
        DeviceName = deviceName;
        try
        {
            ComId = int.Parse(port.Replace("COM", ""));
        }
        catch { }
    }

    /// <summary>
    /// COM1
    /// </summary>
    [ObservableProperty]
    string port;

    /// <summary>
    /// 串口号:COM1
    /// </summary>
    [ObservableProperty]
    int comId;

    /// <summary>
    /// 设备名称
    /// </summary>
    [ObservableProperty]
    string deviceName;

    public override string ToString()
    {
        return $"{Port} : {DeviceName}";
    }
}
