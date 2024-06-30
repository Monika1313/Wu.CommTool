namespace Wu.CommTool.Modules.NetworkTool.Models;

public partial class Ipv4 :ObservableObject
{
    public Ipv4()
    {
            
    }

    public Ipv4(string address, string subnetMask)
    {
        Address = address;
        SubnetMask = subnetMask;
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
}
