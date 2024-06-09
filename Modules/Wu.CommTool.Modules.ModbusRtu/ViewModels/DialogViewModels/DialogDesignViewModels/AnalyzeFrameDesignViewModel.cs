namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels.DialogDesignViewModels;

public class AnalyzeFrameDesignViewModel : AnalyzeFrameViewModel
{
    private static AnalyzeFrameDesignViewModel _Instance = new();
    public static AnalyzeFrameDesignViewModel Instance => _Instance ??= new();
    public AnalyzeFrameDesignViewModel()
    {

    }
}
