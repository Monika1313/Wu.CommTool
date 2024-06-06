using Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace Wu.CommTool.Modules.NetworkTool.ViewModels;

public class NetworkToolViewModel : NavigationViewModel
{
    public NetworkToolViewModel()
    {
        ExecuteCommand = new DelegateCommand<string>(Execute);

        获取物理网卡信息();
    }

    private void Execute(string obj)
    {
        switch (obj)
        {
            case "获取物理网卡信息": 获取物理网卡信息(); break;
            case "获取所有网卡信息": 获取所有网卡信息(); break;
            case "打开网络连接": 打开网络连接(); break;
        }
    }

    #region 属性
    public ObservableCollection<NetworkCard> NetworkCards { get => _NetworkCards; set => SetProperty(ref _NetworkCards, value); }
    private ObservableCollection<NetworkCard> _NetworkCards = [];
    #endregion

    /// <summary>
    /// 获取所有网卡信息
    /// </summary>
    /// <returns></returns>
    private static List<NetworkInterface> 获取所有网卡信息()
    {
        List<NetworkInterface> result = [.. NetworkInterface.GetAllNetworkInterfaces()];
        return result;
    }


    //public void SetIPAddress(string interfaceName, string ipAddress, string subnetMask, string gateway)
    //{
    //    try
    //    {
    //        ManagementScope scope = new ManagementScope("\\\\localhost\\root\\cimv2");
    //        ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapter WHERE Name='" + interfaceName + "'");
    //        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

    //        foreach (ManagementObject mo in searcher.Get())
    //        {
    //            ManagementBaseObject setIP = mo.GetMethodParameters("SetIPaddress");
    //            setIP["IpAddress"] = new string[] { ipAddress };
    //            setIP["SubnetMask"] = new string[] { subnetMask };
    //            mo.InvokeMethod("SetIPaddress", setIP, null);

    //            // 设置默认网关
    //            ManagementBaseObject setGateway = mo.GetMethodParameters("SetGateways");
    //            setGateway["DefaultIPGateway"] = new string[] { gateway };
    //            mo.InvokeMethod("SetGateways", setGateway, null);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Error setting IP: " + ex.Message);
    //    }
    //}


    #region 命令
    public DelegateCommand<string> ExecuteCommand { get; private set; } 
    #endregion


    void 获取物理网卡信息()
    {
        string query = @"SELECT * FROM Win32_NetworkAdapter WHERE Manufacturer!='Microsoft' AND NOT PNPDeviceID LIKE 'ROOT\\%'";
        ManagementObjectSearcher mos = new(query);
        ManagementObjectCollection moc = mos.Get();
        //foreach (ManagementObject mo in moc.Cast<ManagementObject>())
        //{
        //    NetworkCards.Add(new NetworkCard(mo));
        //}
        NetworkCards = [];
        NetworkCards.AddRange(moc.OfType<ManagementObject>().Select(mo => new NetworkCard(mo)));
    }

    void 打开网络连接()
    {
        ProcessStartInfo startInfo = new("NCPA.cpl")
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }
}
