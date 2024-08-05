namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels.DialogDesignViewModels;

public class MrtuDeviceEditDesignViewModel : MrtuDeviceEditViewModel
{
    private static MrtuDeviceEditDesignViewModel _Instance = new();
    public static MrtuDeviceEditDesignViewModel Instance => _Instance ??= new();
    public MrtuDeviceEditDesignViewModel()
    {

    }
}
