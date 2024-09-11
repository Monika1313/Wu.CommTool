namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DesignViewModels;

public class MtcpDeviceMonitorDesignViewModel : MtcpDeviceMonitorViewModel
{
    private static MtcpDeviceMonitorDesignViewModel _Instance = new();
    public static MtcpDeviceMonitorDesignViewModel Instance => _Instance ??= new();
    public MtcpDeviceMonitorDesignViewModel()
    {

    }
}
