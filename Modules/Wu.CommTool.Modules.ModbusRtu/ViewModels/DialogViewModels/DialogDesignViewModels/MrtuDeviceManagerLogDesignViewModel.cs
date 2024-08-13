namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels.DialogDesignViewModels;

public class MrtuDeviceManagerLogDesignViewModel : MrtuDeviceManagerLogViewModel
{
    private static MrtuDeviceManagerLogDesignViewModel _Instance = new();
    public static MrtuDeviceManagerLogDesignViewModel Instance => _Instance ??= new();
    public MrtuDeviceManagerLogDesignViewModel()
    {

    }
}
