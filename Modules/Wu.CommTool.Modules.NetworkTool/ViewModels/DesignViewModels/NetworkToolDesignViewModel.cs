namespace Wu.CommTool.Modules.NetworkTool.ViewModels.DesignViewModels;

public class NetworkToolDesignViewModel : NetworkToolViewModel
{
    private static NetworkToolDesignViewModel _Instance = new();
    public static NetworkToolDesignViewModel Instance => _Instance ??= new();
    public NetworkToolDesignViewModel()
    {

    }
}
