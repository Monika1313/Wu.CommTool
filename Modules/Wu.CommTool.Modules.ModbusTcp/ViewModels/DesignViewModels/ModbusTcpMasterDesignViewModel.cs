namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DesignViewModels;

public class ModbusTcpMasterDesignViewModel : ModbusTcpMasterViewModel
{
    private static ModbusTcpMasterDesignViewModel _Instance = new();
    public static ModbusTcpMasterDesignViewModel Instance => _Instance ??= new();
    public ModbusTcpMasterDesignViewModel()
    {

    }
}
