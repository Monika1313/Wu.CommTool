namespace Wu.CommTool.Modules.NetworkTool.Models;

public partial class Ipv4 : ObservableObject
{
    public Ipv4() : this(string.Empty, string.Empty)
    {

    }

    public Ipv4(string address, string subnetMask = "255.255.255.0")
    {
        Address = address;
        SubnetMask = subnetMask;
    }

    public Ipv4(string address, string subnetMask, string defaultGateway)
    {
        Address = address;
        SubnetMask = subnetMask;
        DefaultGateway = defaultGateway;
    }

    /// <summary>
    /// IPv4地址
    /// </summary>
    [ObservableProperty]
    string address;

    /// <summary>
    /// 子网掩码
    /// </summary>
    [ObservableProperty]
    string subnetMask;

    /// <summary>
    /// 网关
    /// </summary>
    [ObservableProperty]
    string defaultGateway;

}
