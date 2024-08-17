namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels;

public class MrtuDeviceMonitorDesignViewModel : MrtuDeviceMonitorViewModel
{
    private static MrtuDeviceMonitorDesignViewModel _Instance = new();
    public static MrtuDeviceMonitorDesignViewModel Instance => _Instance ??= new();
    public MrtuDeviceMonitorDesignViewModel()
    {
        MrtuDeviceManager = new MrtuDeviceManager();
        MrtuDeviceManager.MrtuDevices.Add(new MrtuDevice() { Name = "测试设备1", DeviceState = DeviceState.Online });
        MrtuDeviceManager.MrtuDevices.Add(new MrtuDevice() { Name = "测试设备2" });
        MrtuDeviceManager.MrtuDevices.Add(new MrtuDevice() { Name = "测试设备3" });
        var device = MrtuDeviceManager.MrtuDevices[0];
        device.MrtuDatas.Add(new MrtuData() { Name = "测点1" });
        device.MrtuDatas.Add(new MrtuData() { Name = "测点2" });
        device.MrtuDatas.Add(new MrtuData() { Name = "测点3" });
    }
}
