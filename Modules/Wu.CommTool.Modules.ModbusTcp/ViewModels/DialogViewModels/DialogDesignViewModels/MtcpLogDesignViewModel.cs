namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DialogViewModels.DialogDesignViewModels;

public class MtcpLogDesignViewModel : MtcpLogViewModel
{
    private static MtcpLogDesignViewModel _Instance = new();
    public static MtcpLogDesignViewModel Instance => _Instance ??= new();
    public MtcpLogDesignViewModel()
    {

    }
}
