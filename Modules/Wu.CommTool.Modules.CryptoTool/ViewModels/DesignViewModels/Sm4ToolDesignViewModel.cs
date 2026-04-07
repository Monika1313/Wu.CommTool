namespace Wu.CommTool.Modules.CryptoTool.ViewModels.DesignViewModels;

public class Sm4ToolDesignViewModel : Sm4ToolViewModel
{
    private static Sm4ToolDesignViewModel _Instance = new();
    public static Sm4ToolDesignViewModel Instance => _Instance ??= new();
    public Sm4ToolDesignViewModel()
    {

    }
}
