namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels.DialogDesignViewModels;

public class MrtuDeviceLogDesignViewModel : MrtuDeviceLogViewModel
{
    private static MrtuDeviceLogDesignViewModel _Instance = new();
    public static MrtuDeviceLogDesignViewModel Instance => _Instance ??= new();
    public MrtuDeviceLogDesignViewModel()
    {

    }
}
