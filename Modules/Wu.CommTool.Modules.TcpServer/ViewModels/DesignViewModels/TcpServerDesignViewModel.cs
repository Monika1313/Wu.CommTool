namespace Wu.CommTool.Modules.TcpServer.ViewModels.DesignViewModels;

public class TcpServerDesignViewModel : TcpServerViewModel
{
    private static TcpServerDesignViewModel _Instance = new();
    public static TcpServerDesignViewModel Instance => _Instance ??= new();
    public TcpServerDesignViewModel()
    {

    }
}
