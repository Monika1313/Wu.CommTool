namespace Wu.CommTool.Modules.NetworkTool.ViewModels.DesignViewModels;

public class NetworkToolDesignViewModel : NetworkToolViewModel
{
    private static NetworkToolDesignViewModel _Instance = new();
    public static NetworkToolDesignViewModel Instance => _Instance ??= new();
    public NetworkToolDesignViewModel()
    {
        获取物理网卡信息();

        NetworkCardConfig n1 = new()
        {
            Ipv4s = [new Ipv4("192.168.1.3", "255.255.255.0"),
                     new Ipv4("192.168.2.3", "255.255.255.0"),
                     new Ipv4("192.168.3.3", "255.255.255.0"),
                    ]
        };
        NetworkCardConfig n2 = new()
        {
            Ipv4s = [new Ipv4("192.168.1.3", "255.255.255.0"),
                     new Ipv4("192.168.2.3", "255.255.255.0"),
                     new Ipv4("192.168.3.3", "255.255.255.0"),
                    ]
        };
        NetworkCardConfigs.Add(n1);
        NetworkCardConfigs.Add(n2);
        SelectedConfig = NetworkCardConfigs.First();
    }
}
