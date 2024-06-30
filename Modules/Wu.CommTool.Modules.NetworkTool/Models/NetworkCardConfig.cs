namespace Wu.CommTool.Modules.NetworkTool.Models;

public partial class NetworkCardConfig : ObservableObject
{
    /// <summary>
    /// Ipv4地址列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<Ipv4> ipv4s = [];
}
