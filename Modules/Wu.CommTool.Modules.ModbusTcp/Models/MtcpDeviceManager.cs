namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// ModbusTcp设备 管理
/// </summary>
public partial class MtcpDeviceManager : ObservableObject
{
    /// <summary>
    /// ModbusTcp设备列表
    /// </summary>
    [ObservableProperty] ObservableCollection<MtcpDevice> mtcpDevices = [];

    /// <summary>
    /// 选中的Mtcp设备
    /// </summary>
    [ObservableProperty] MtcpDevice selectedMtcpDevice;

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    bool state;

    [RelayCommand]
    [property: JsonIgnore]
    private void Run()
    {
        State = true;
        
        foreach (var mtcpDevice in MtcpDevices)
        {
            mtcpDevice.Owner = this;
            mtcpDevice.RunMonitorTask();  //打开数据监控任务
        }

        updateDeviceStateTaskCts = new();
        updateDeviceStateTask = new Task(UpdateDeviceState, updateDeviceStateTaskCts.Token);
        updateDeviceStateTask.Start();
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void Stop()
    {
        try
        {
            State = false;
            updateDeviceStateTaskCts.Cancel();
            foreach (var mtcpDevice in MtcpDevices)
            {
                mtcpDevice.DisConnect();
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    private CancellationTokenSource updateDeviceStateTaskCts;
    private Task updateDeviceStateTask;

    /// <summary>
    /// 周期更新设备状态
    /// </summary>
    private async void UpdateDeviceState()
    {
        while (true)
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

    [RelayCommand]
    [property: JsonIgnore]
    private void AddNewMtcpDevice(MtcpDevice MtcpDevice)
    {
        try
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
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void DeleteMtcpDevice(MtcpDevice MtcpDevice)
    {
        try
        {
            if (MtcpDevices.Contains(MtcpDevice))
            {
                MtcpDevices.Remove(MtcpDevice);
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }
}
