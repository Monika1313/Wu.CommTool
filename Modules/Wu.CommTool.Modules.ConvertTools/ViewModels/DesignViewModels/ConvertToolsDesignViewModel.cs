namespace Wu.CommTool.Modules.ConvertTools.ViewModels.DesignViewModels;

public class ConvertToolsDesignViewModel : ConvertToolsViewModel
{
    private static ConvertToolsDesignViewModel _Instance = new();
    public static ConvertToolsDesignViewModel Instance => _Instance ??= new();
    public ConvertToolsDesignViewModel()
    {
        
    }
}
