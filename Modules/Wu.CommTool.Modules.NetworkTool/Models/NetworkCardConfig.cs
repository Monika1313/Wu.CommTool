﻿namespace Wu.CommTool.Modules.NetworkTool.Models;

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

    ///// <summary>
    ///// 默认网关
    ///// </summary>
    //[ObservableProperty]
    //ObservableCollection<string> defaultIPGateway = [];

    [RelayCommand]
    [property: JsonIgnore]
    public void DeleteLine(Ipv4 obj)
    {
        if (obj == null)
            return;
        if (Ipv4s.Contains(obj))
        {
            Ipv4s.Remove(obj);
            if (Ipv4s.Count == 0)
            {
                Ipv4s.Add(new Ipv4());
            }
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void AddNewLine(Ipv4 obj)
    {
        if (obj == null || !Ipv4s.Contains(obj))
        {
            Ipv4s.Add(new Ipv4());
            return;
        }
        else
        {
            Ipv4s.Insert(Ipv4s.IndexOf(obj)+1, new Ipv4());
        }
    }
}
