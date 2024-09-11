namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// ModbusTcp设备 管理
/// </summary>
public partial class MtcpDeviceManager : ObservableObject
{
    /// <summary>
    /// ModbusTcp设备列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MtcpDevice> mtcpDevices = [];

    /// <summary>
    /// 选中的Mtcp设备
    /// </summary>
    [ObservableProperty]
    MtcpDevice selectedMtcpDevice;

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
        //ComPorts = ModbusUtils.GetComPorts();//获取当前设备的串口
        //Status = true;
        //MtcpSerialPorts = [];
        ////遍历MtcpDevice,获取需要使用的串口
        //foreach (var device in MtcpDevices)
        //{
        //    if (!string.IsNullOrWhiteSpace(device.ComConfig.ComPort.Port))
        //    {
        //        device.AnalyzeDataAddress();//更新请求帧列表

        //        //判断是否已存在,已存在则不创建
        //        if (MtcpSerialPorts.FindFirst(x => x.ComConfig.ComPort.Port == device.ComConfig.ComPort.Port) != null)
        //        {
        //            continue;
        //        }

        //        //尝试获取指定串口
        //        var comport = ComPorts.FirstOrDefault(x => x.Port == device.ComConfig.ComPort.Port);
        //        //若没有指定的串口则退出循环
        //        if (comport == null)
        //        {
        //            continue;
        //        }

        //        //串口实例
        //        MtcpSerialPort MtcpSerialPort = new(device.ComConfig)
        //        {
        //            Owner = this//设置所有者
        //        };
        //        MtcpSerialPorts.Add(MtcpSerialPort);
        //        MtcpSerialPort.Run();
        //    }
        //}

        //updateDeviceStateTaskCts = new();
        //updateDeviceStateTask = new Task(UpdateDeviceState,updateDeviceStateTaskCts.Token);
        //updateDeviceStateTask.Start();
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void Stop()
    {
        Status = false;
        //MtcpSerialPorts = null;
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
                foreach (var x in MtcpDevices)
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
    /// 串口列表
    /// </summary>
    public List<ComPort> ComPorts { get; set; } = [];

    ///// <summary>
    ///// Mtcp串口列表
    ///// </summary>
    //[ObservableProperty]
    //[property: JsonIgnore]
    //ObservableCollection<MtcpSerialPort> MtcpSerialPorts = [];

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
    private void AddNewMtcpDevice(MtcpDevice MtcpDevice)
    {
        if (MtcpDevice == null || !MtcpDevices.Contains(MtcpDevice))
        {
            MtcpDevices.Add(new MtcpDevice() { Name = "未命名" });
            return;
        }
        else
        {
            MtcpDevices.Insert(MtcpDevices.IndexOf(MtcpDevice) + 1, new MtcpDevice() { Name = "未命名" });
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void DeleteMtcpDevice(MtcpDevice MtcpDevice)
    {
        if (MtcpDevices.Contains(MtcpDevice))
        {
            MtcpDevices.Remove(MtcpDevice);
        }
    }
}
