namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DialogViewModels.DialogDesignViewModels;

public class MtcpDeviceEditDesignViewModel : MtcpDeviceEditViewModel
{
    private static MtcpDeviceEditDesignViewModel _Instance = new();
    public static MtcpDeviceEditDesignViewModel Instance => _Instance ??= new();
    public MtcpDeviceEditDesignViewModel()
    {
        
    }
}
