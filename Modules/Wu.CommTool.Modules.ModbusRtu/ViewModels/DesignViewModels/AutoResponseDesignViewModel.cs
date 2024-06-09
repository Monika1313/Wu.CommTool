namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels;

public class AutoResponseDesignViewModel : AutoResponseViewModel
{
    private static AutoResponseDesignViewModel _Instance = new();
    public static AutoResponseDesignViewModel Instance => _Instance ??= new();
    public AutoResponseDesignViewModel()
    {
        ModbusRtuModel = new Models.ModbusRtuModel();
    }
}
