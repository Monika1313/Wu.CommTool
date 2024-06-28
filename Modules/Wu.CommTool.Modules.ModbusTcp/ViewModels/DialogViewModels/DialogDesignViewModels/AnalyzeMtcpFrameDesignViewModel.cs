namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DialogViewModels.DialogDesignViewModels;

public class AnalyzeMtcpFrameDesignViewModel : AnalyzeMtcpFrameViewModel
{
    private static AnalyzeMtcpFrameDesignViewModel _Instance = new();
    public static AnalyzeMtcpFrameDesignViewModel Instance => _Instance ??= new();
    public AnalyzeMtcpFrameDesignViewModel()
    {

    }
}
