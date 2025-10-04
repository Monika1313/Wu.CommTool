namespace Wu.CommTool.Modules.Udp.ViewModels.DesignViewModels;

public class UdpDesignViewModel : UdpViewModel
{
    private static UdpDesignViewModel _Instance = new();
    public static UdpDesignViewModel Instance => _Instance ??= new();
    public UdpDesignViewModel()
    {

    }
}
