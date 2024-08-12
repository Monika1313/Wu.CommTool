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

    /// <summary>
    /// 选中的Mrtu设备
    /// </summary>
    [ObservableProperty]
    MrtuDevice selectedMrtuDevice;

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    bool status;

    [RelayCommand]
    [property: JsonIgnore]
    private async Task Run()
    {
        ComPorts = ModbusUtils.GetComPorts();//获取当前设备的串口
        Status = true;
        ComTaskDict = [];//TODO 关闭时需要正确退出所有串口子线程
        MrtuSerialPorts = [];
        //TODO 遍历MrtuDevice,获取需要使用的串口
        foreach (var device in MrtuDevices)
        {
            if (!string.IsNullOrWhiteSpace(device.ComConfig.ComPort.Port))
            {
                //TODO 需要在测点修改后实现自动更新
                device.AnalyzeDataAddress();//更新请求帧列表

                //判断是否已存在,已存在则不创建
                if (ComTaskDict.FindFirst(x => x.Key == device.ComConfig.ComPort.Port).Key != null)
                {
                    continue;
                }

                var config = device.ComConfig;
                //尝试获取指定串口
                var comport = ComPorts.FirstOrDefault(x => x.Port == config.ComPort.Port);
                //若没有指定的串口则不执行
                if (comport == null)
                {
                    continue;
                }
                Task task = new(() => ComTask(device.ComConfig));
                ComTaskDict.Add(device.ComConfig.ComPort.Port, task);
                task.Start();
            }
        }
    }

    /// <summary>
    /// 串口线程  执行读写
    /// </summary>
    /// <param name="com"></param>
    public async Task ComTask(ComConfig config)
    {
        //尝试获取指定串口
        var comport = ComPorts.FirstOrDefault(x => x.Port == config.ComPort.Port);
        //若没有指定的串口则退出循环
        if (comport == null)
        {
            return;
        }
        //串口实例
        MrtuSerialPort mrtuSerialPort = new(config)
        {
            Owner = this//设置所有者
        };
        MrtuSerialPorts.Add(mrtuSerialPort);
        await mrtuSerialPort.Run();
    }

    [RelayCommand]
    [property: JsonIgnore]
    private async Task Stop()
    {
        Status = false;
        ComTaskDict = [];
    }

    /// <summary>
    /// 串口列表
    /// </summary>
    public List<ComPort> ComPorts { get; set; } = [];

    /// <summary>
    /// 用于管理串口线程
    /// </summary>
    public Dictionary<string, Task> ComTaskDict { get; set; } = [];

    /// <summary>
    /// Mrtu串口列表
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    ObservableCollection<MrtuSerialPort> mrtuSerialPorts = [];

    /// <summary>
    /// 获取串口完整名字（包括驱动名字）
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    public void GetComPorts()
    {
        ComPorts = ModbusUtils.GetComPorts();
    }


    [RelayCommand]
    [property: JsonIgnore]
    private void AddNewMrtuDevice(MrtuDevice mrtuDevice)
    {
        if (mrtuDevice == null || !MrtuDevices.Contains(mrtuDevice))
        {
            MrtuDevices.Add(new MrtuDevice() { Name ="未命名"});
            return;
        }
        else
        {
            MrtuDevices.Insert(MrtuDevices.IndexOf(mrtuDevice) + 1, new MrtuDevice() { Name = "未命名" });
        }
    }


    [RelayCommand]
    [property: JsonIgnore]
    private void DeleteMrtuDevice(MrtuDevice mrtuDevice)
    {
        if (MrtuDevices.Contains(mrtuDevice))
        {
            MrtuDevices.Remove(mrtuDevice);
        }
    }


}
