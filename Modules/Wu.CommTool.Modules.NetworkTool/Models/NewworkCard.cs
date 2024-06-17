using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Prism.Services.Dialogs;
using System.Diagnostics;
using System.Security.Principal;
using Wu.CommTool.Core.Extensions;

namespace Wu.CommTool.Modules.NetworkTool.Models;

/// <summary>
/// 网卡
/// </summary>
public partial class NetworkCard : ObservableObject
{

    public NetworkCard(ManagementObject mo)
    {
        this.mo = mo;
        UpdateInfo();
    }

    

    public void UpdateInfo()
    {
        try
        {
            //DhcpEnable = Convert.ToBoolean(mo["DHCPEnabled"] ?? false);//DHCP状态
            Enable = Convert.ToBoolean(mo["NetEnabled"] ?? false);//启用状态
            ConnectionId = mo["NetConnectionID"]?.ToString();//连接名称
            Name = mo["Name"]?.ToString();                   //驱动程序
            Manufacturer = mo["Manufacturer"]?.ToString();   //制造商


            var xx = (System.Array)(mo.Properties["IPAddress"].Value);
            //Console.WriteLine("IP(" + st + ")|" + "MAC(" + WmiObj["MACAddress"] + ")" + "\n");
            //netmac = WmiObj["MACAddress"];
            //netmac = netmac.ToString().Replace(":", "");
        }
        catch (Exception ex)
        {

        }
    }

    readonly ManagementObject mo;

    [ObservableProperty]
    string name;

    [ObservableProperty]
    bool enable;

    [ObservableProperty]
    bool dhcpEnable;

    [ObservableProperty]
    string connectionId;

    [ObservableProperty]
    string manufacturer;

    [RelayCommand]
    public async void 启用DHCP()
    {
        try
        {
            











            ////如果没有启用IP设置的网络设备则跳过
            ////重置DNS为空
            //mo.InvokeMethod("SetDNSServerSearchOrder", null);
            ////开启DHCP
            //mo.InvokeMethod("EnableDHCP", null);



            //需要管理员权限
            // 设置网卡为DHCP
            ProcessStartInfo psi = new()
            {
                FileName = "netsh",
                Arguments = $"interface ip set address \"{ConnectionId}\" source=dhcp",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using Process process = new();
            process.StartInfo = psi;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                HcGrowlExtensions.Warning("设置失败,需要管理员权限...");
            }
        }
        catch (Exception ex)
        {
        }
    }
}
