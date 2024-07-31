namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels;

public class MrtuDeviceMonitorDesignViewModel : MrtuDeviceMonitorViewModel
{
    private static MrtuDeviceMonitorDesignViewModel _Instance = new();
    public static MrtuDeviceMonitorDesignViewModel Instance => _Instance ??= new();
    public MrtuDeviceMonitorDesignViewModel()
    {
        MrtuDeviceManager = new MrtuDeviceManager();
        MrtuDeviceManager.MrtuDevices.Add(new MrtuDevice() { Name = "测试设备1", Status = DeviceStatus.Online });
        MrtuDeviceManager.MrtuDevices.Add(new MrtuDevice() { Name = "测试设备2" });
        MrtuDeviceManager.MrtuDevices.Add(new MrtuDevice() { Name = "测试设备3" });
        CurrentDevice = MrtuDeviceManager.MrtuDevices[0];
        CurrentDevice.MrtuDatas.Add(new MrtuData() { Name = "测点1" });
        CurrentDevice.MrtuDatas.Add(new MrtuData() { Name = "测点2" });
        CurrentDevice.MrtuDatas.Add(new MrtuData() { Name = "测点3" });
    }
}
