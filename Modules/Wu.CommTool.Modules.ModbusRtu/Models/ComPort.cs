namespace Wu.CommTool.Modules.ModbusRtu.Models;

public partial class ComPort : ObservableObject
{
    public ComPort()
    {
            
    }

    public ComPort(string port, string deviceName)
    {
        Port = port;
        DeviceName = deviceName;
    }

    /// <summary>
    /// Com n
    /// </summary>
    [ObservableProperty]
    string port;

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
