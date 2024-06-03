namespace Wu.CommTool.Modules.NetworkTool.Models;

/// <summary>
/// 网卡
/// </summary>
public partial class NetworkCard : ObservableObject
{

    public NetworkCard(ManagementObject mo)
    {
            //Name=mo.
    }

    [ObservableProperty]
    string name;


}
