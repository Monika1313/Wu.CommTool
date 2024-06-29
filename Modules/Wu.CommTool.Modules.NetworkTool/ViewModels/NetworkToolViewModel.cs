namespace Wu.CommTool.Modules.NetworkTool.ViewModels;

public partial class NetworkToolViewModel : NavigationViewModel
{
    public NetworkToolViewModel() { }

    public NetworkToolViewModel(IDialogHostService dialogHost)
    {
        获取物理网卡信息();
        this.dialogHost = dialogHost;
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
    #endregion


    [RelayCommand]
    void Execute(string obj)
    {
        switch (obj)
        {
            case "获取物理网卡信息": 获取物理网卡信息(); break;
            case "获取所有网卡信息": 获取所有网卡信息(); break;
            case "打开网络连接": NetworkToolViewModel.打开网络连接(); break;
            case "测试": 测试(); break;
        }
    }

    /// <summary>
    /// 执行Netsh命令
    /// </summary>
    /// <param name="arguments"></param>
    public static void ExecuteNetshCommand(string arguments)
    {
        ProcessStartInfo psi = new ProcessStartInfo("netsh", arguments)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(output);
        }
    }


    /// <summary>
    /// 修改网卡启用禁用状态
    /// </summary>
    /// <param name="nwc"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task NetEnable(NetworkCard nwc)
    {
        try
        {
            if (await 是否有管理员权限())
            {
                return;
            }

            if (nwc.NetEnabled)
            {
                //nwc.mo.InvokeMethod("Disable", null);
                // 禁用网卡
                ExecuteNetshCommand($"interface set interface \"{nwc.NetConnectionId}\" admin=disable");
                await Task.Delay(1000);
            }
            else
            {
                // 禁用网卡
                ExecuteNetshCommand($"interface set interface \"{nwc.NetConnectionId}\" admin=enable");
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    private void 测试()
    {

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
    public async Task EnableDhcp(NetworkCard nwc)
    {
        try
        {
            //判断管理员权限 非管理员权限请求提权
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                //提示获取管理员权限
                var result = await dialogHost.Question("警告", "该操作需要管理员权限,点击确认以管理员权限重启该软件，重启后再使用该功能。", "Root");
                // 如果不是管理员，则重新启动具有管理员权限的应用程序
                if (result.Result != ButtonResult.OK)
                {
                    return;
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
                    return;
                }
                Application.Current.Shutdown();
                return;
            }

            //以下需要管理员权限
            //设置网卡为DHCP
            ProcessStartInfo psi = new()
            {
                FileName = "netsh",
                Arguments = $"interface ip set address \"{nwc.NetConnectionId}\" source=dhcp",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using Process process = new();
            process.StartInfo = psi;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            //TODO 若已经启用DHCP则会报错, 需要设置已启用DHCP时按钮不可用
            if (process.ExitCode != 0)
            {
                HcGrowlExtensions.Warning("设置失败,需要管理员权限...");
            }
            else
            {
                HcGrowlExtensions.Success("启用成功");
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }



    void 获取物理网卡信息()
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
}
