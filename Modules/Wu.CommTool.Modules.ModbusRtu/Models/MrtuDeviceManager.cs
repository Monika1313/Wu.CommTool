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
    /// Mrtu串口列表
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    ObservableCollection<MrtuSerialPort> mrtuSerialPorts = [];

    /// <summary>
    /// 选中的Mrtu设备
    /// </summary>
    [ObservableProperty]
    MrtuDevice selectedMrtuDevice;

    /// <summary>
    /// 串口列表
    /// </summary>
    public List<ComPort> ComPorts { get; set; } = [];

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    bool status;

    [RelayCommand]
    [property: JsonIgnore]
    private void Run()
    {
        ComPorts = ModbusUtils.GetComPorts();//获取当前设备的串口
        Status = true;
        MrtuSerialPorts = [];
        //遍历MrtuDevice,获取需要使用的串口
        foreach (var device in MrtuDevices)
        {
            if (!string.IsNullOrWhiteSpace(device.ComConfig.ComPort.Port))
            {
                //device.AnalyzeDataAddress();//更新请求帧列表

                //判断是否已存在,已存在则不创建
                if (MrtuSerialPorts.FindFirst(x => x.ComConfig.ComPort.Port == device.ComConfig.ComPort.Port) != null)
                {
                    continue;
                }

                //尝试获取指定串口
                var comport = ComPorts.FirstOrDefault(x => x.Port == device.ComConfig.ComPort.Port);
                //若没有指定的串口则退出循环
                if (comport == null)
                {
                    continue;
                }

                //串口实例
                MrtuSerialPort mrtuSerialPort = new(device.ComConfig)
                {
                    Owner = this//设置所有者
                };
                MrtuSerialPorts.Add(mrtuSerialPort);
                mrtuSerialPort.Run();
            }
        }

        updateDeviceStateTaskCts = new();
        updateDeviceStateTask = new Task(UpdateDeviceState,updateDeviceStateTaskCts.Token);
        updateDeviceStateTask.Start();
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void Stop()
    {
        Status = false;
        MrtuSerialPorts = null;
        updateDeviceStateTaskCts.Cancel();
    }

    private CancellationTokenSource updateDeviceStateTaskCts;
    private Task updateDeviceStateTask;

    private async void UpdateDeviceState()
    {
        while(true)
        {
            try
            {
                foreach (var x in MrtuDevices)
                {
                    x.UpdateState();
                }
                await Task.Delay(1000);
            }
            catch (Exception)
            {

            }
        }
    }

    /// <summary>
    /// 获取串口完整名字（包括驱动名字）
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    public void GetComPorts()
    {
        Task.Run(() => ComPorts = ModbusUtils.GetComPorts());
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void AddNewMrtuDevice(MrtuDevice mrtuDevice)
    {
        if (mrtuDevice == null || !MrtuDevices.Contains(mrtuDevice))
        {
            MrtuDevices.Add(new MrtuDevice() { Name = "未命名" });
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
