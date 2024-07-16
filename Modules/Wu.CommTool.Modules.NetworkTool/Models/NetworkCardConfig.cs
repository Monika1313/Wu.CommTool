using ImTools;
using System.Linq;

namespace Wu.CommTool.Modules.NetworkTool.Models;

public partial class NetworkCardConfig : ObservableObject
{
    /// <summary>
    /// 配置名称
    /// </summary>
    [ObservableProperty]
    string configName = "";

    /// <summary>
    /// Ipv4地址列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<Ipv4> ipv4s = [];


    [RelayCommand]
    [property: JsonIgnore]
    public void DeleteLine(Ipv4 obj)
    {
        if (obj == null)
            return;
        if (Ipv4s.Contains(obj))
        {
            Ipv4s.Remove(obj);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void AddNewLine()
    {
        Ipv4s.Add(new Ipv4());
    }
}
