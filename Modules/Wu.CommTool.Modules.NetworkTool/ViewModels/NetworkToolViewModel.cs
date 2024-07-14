using System.Net;

namespace Wu.CommTool.Modules.NetworkTool.ViewModels;

public partial class NetworkToolViewModel : NavigationViewModel
{
    public NetworkToolViewModel() { }

    public NetworkToolViewModel(IDialogHostService dialogHost)
    {
        获取物理网卡信息();
        this.dialogHost = dialogHost;


        NetworkCardConfig.Ipv4s.Add(new Ipv4("192.168.3.3", "255.255.255.0"));
        NetworkCardConfig.Ipv4s.Add(new Ipv4("192.168.2.233", "255.255.255.0"));
    }

    #region 字段
    private readonly IDialogHostService dialogHost;
    #endregion


    #region 属性
    /// <summary>
    /// 网卡列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<NetworkCard> networkCards = [];

    /// <summary>
    ///  当前选中的网卡配置
    /// </summary>
    [ObservableProperty]
    NetworkCardConfig networkCardConfig = new();

    /// <summary>
    /// 网卡配置列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<NetworkCardConfig> networkCardConfigs;
    #endregion

    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "获取物理网卡信息": 获取物理网卡信息(); break;
            case "获取所有网卡信息": 获取所有网卡信息(); break;
            case "打开网络连接": 打开网络连接(); break;
        }
    }

    /// <summary>
    /// 执行Netsh命令
    /// </summary>
    /// <param name="arguments"></param>
    public static ExecuteCmdResult ExecuteNetshCommand(string arguments)
    {
        ProcessStartInfo psi = new("netsh", arguments)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return new ExecuteCmdResult(process.ExitCode, output);
    }




    /// <summary>
    /// 为指定的网卡设置静态IP
    /// </summary>
    /// <param name="nwc"></param>
    /// <returns></returns>
    [RelayCommand]
    internal async Task SetIpv4(NetworkCard nwc)
    {
        try
        {
            if (await 是否有管理员权限())
            {
                return;
            }

            #region 执行netsh命令
            //string netshCmd = $"interface ipv4 set address {nwc.NetConnectionId} static {NetworkCardConfig.Ipv4s[0].Address} {NetworkCardConfig.Ipv4s[0].SubnetMask}\r\n";
            //// 添加额外的IP地址
            //for (int i = 1; i < NetworkCardConfig.Ipv4s.Count; i++)
            //{
            //    netshCmd += $"interface ipv4 add address {nwc.NetConnectionId} {NetworkCardConfig.Ipv4s[i].Address} {NetworkCardConfig.Ipv4s[i].SubnetMask}\r\n";
            //}
            //var result = ExecuteNetshCommand(netshCmd);
            //if (result.Status)
            //{
            //    HcGrowlExtensions.Success($"设置成功...{result.Message}");
            //}
            //else
            //{
            //    HcGrowlExtensions.Warning($"设置失败...{result.Message}");
            //}
            ////延迟后更新网卡信息
            //await Task.Delay(1000);
            //nwc.UpdateInfo();
            #endregion

            //TODO 测试用netsh和cmd都只能一次执行一条命令,还需要再优化
            #region 使用cmd执行
            string cmd = $"netsh interface ipv4 set address {nwc.NetConnectionId} static {NetworkCardConfig.Ipv4s[0].Address} {NetworkCardConfig.Ipv4s[0].SubnetMask}";
            ExecuteCommands(cmd);
            // 添加额外的IP地址
            //string cmd2 = "";
            for (int i = 1; i < NetworkCardConfig.Ipv4s.Count; i++)
            {
                cmd = $"netsh interface ipv4 add address {nwc.NetConnectionId} {NetworkCardConfig.Ipv4s[i].Address} {NetworkCardConfig.Ipv4s[i].SubnetMask}";
                ExecuteCommands(cmd);
            }

            //var result = ExecuteCommands(cmd);
            //if (result.Status)
            //{
            //    HcGrowlExtensions.Success($"设置成功...{result.Message}");
            //}
            //else
            //{
            //    HcGrowlExtensions.Warning($"设置失败...{result.Message}");
            //}
            //延时后更新网卡信息
            await Task.Delay(1000);
            nwc.UpdateInfo();
            #endregion

            //ExecuteCmdResult

        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 获取所有网卡信息
    /// </summary>
    /// <returns></returns>
    private static List<NetworkInterface> 获取所有网卡信息()
    {
        List<NetworkInterface> result = [.. NetworkInterface.GetAllNetworkInterfaces()];
        return result;
    }


    public async Task<bool> 是否有管理员权限()
    {
        //判断管理员权限 非管理员权限请求提权
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            //提示获取管理员权限
            var result = await dialogHost.Question("警告", "该操作需要管理员权限,点击确认以管理员权限重启该软件，重启后再使用该功能。", "Root");
            // 如果不是管理员，则重新启动具有管理员权限的应用程序
            if (result.Result != ButtonResult.OK)
            {
                return false;
            }

            // 获取当前运行的可执行文件的完整路径
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            var processInfo = new ProcessStartInfo(currentExe)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                // 用户取消了UAC提示或其他错误处理
                HcGrowlExtensions.Warning(ex.Message);
                return false;
            }
            Application.Current.Shutdown();
            return false;
        }
        return false;
    }

    /// <summary>
    /// 指定网卡启用DHCP
    /// </summary>
    /// <param name="nwc"></param>
    [RelayCommand]
    internal async Task EnableDhcp(NetworkCard nwc)
    {
        try
        {
            //判断管理员权限 非管理员权限请求提权
            if (await 是否有管理员权限())
            {
                return;
            }

            //以下需要管理员权限
            string netshCmd = $"interface ip set address \"{nwc.NetConnectionId}\" source=dhcp";

            var result = ExecuteNetshCommand(netshCmd);

            //TODO 若已经启用DHCP则会报错, 需要设置已启用DHCP时按钮不可用
            if (result.Status)
            {
                HcGrowlExtensions.Success("启用Dhcp成功");
            }
            else
            {
                HcGrowlExtensions.Warning($"设置失败,需要管理员权限...{result.Message}");
            }

            //延时后更新网卡信息
            await Task.Delay(1000);
            nwc.UpdateInfo();
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 修改网卡启用禁用状态
    /// </summary>
    /// <param name="nwc"></param>
    /// <returns></returns>
    [RelayCommand]
    internal async Task NetEnable(NetworkCard nwc)
    {
        try
        {
            if (await 是否有管理员权限())
            {
                return;
            }
            string netshCmd = "";
            if (nwc.NetEnabled)
            {
                netshCmd = $"interface set interface \"{nwc.NetConnectionId}\" admin=disable";
            }
            else
            {
                netshCmd = $"interface set interface \"{nwc.NetConnectionId}\" admin=enable";
            }
            var result = ExecuteNetshCommand(netshCmd);

            if (result.Status)
            {

            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }


    protected void 获取物理网卡信息()
    {
        try
        {
            #region 查询Win32_NetworkAdapter
            string query = @"SELECT * FROM Win32_NetworkAdapter WHERE Manufacturer!='Microsoft' AND NOT PNPDeviceID LIKE 'ROOT\\%'";
            ManagementObjectSearcher mos = new(query);
            ManagementObjectCollection moc = mos.Get();
            #endregion

            #region 这个查询方法也能用
            //ManagementObjectSearcher mos = new("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True AND NOT PNPDeviceID LIKE 'ROOT%'");
            //ManagementObjectCollection moc = mos.Get();  
            #endregion

            NetworkCards = [];
            NetworkCards.AddRange(moc.OfType<ManagementObject>().Select(mo => new NetworkCard(mo)));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    private static void 打开网络连接()
    {
        ProcessStartInfo startInfo = new("NCPA.cpl")
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    /// <summary>
    /// 执行cmd命令
    /// </summary>
    /// <param name="commands"></param>
    public static ExecuteCmdResult ExecuteCommands(string commands)
    {
        ProcessStartInfo processInfo = new("cmd.exe", "/c " + commands)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new()
        {
            StartInfo = processInfo
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ExecuteCmdResult(process.ExitCode, output);
    }
}
