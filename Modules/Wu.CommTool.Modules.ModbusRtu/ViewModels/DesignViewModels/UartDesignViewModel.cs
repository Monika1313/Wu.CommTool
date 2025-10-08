namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels;

public class UartDesignViewModel : UartViewModel
{
    private static UartDesignViewModel _Instance = new();
    public static UartDesignViewModel Instance => _Instance ??= new();
    public UartDesignViewModel()
    {

    }
}
