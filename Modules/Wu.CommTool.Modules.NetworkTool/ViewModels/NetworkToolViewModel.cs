namespace Wu.CommTool.Modules.NetworkTool.ViewModels;

public partial class NetworkToolViewModel : NavigationViewModel
{
    public NetworkToolViewModel() { }

    public NetworkToolViewModel(IDialogHostService dialogHost)
    {
        获取物理网卡信息();
        this.dialogHost = dialogHost;

        //从默认配置文件中读取配置
        try
        {
            var re = Core.Common.Utils.ReadJsonFile(Path.Combine(networkCardConfigFolder, @"Default.jsonNCC"));
            var x = JsonConvert.DeserializeObject<NetworkCardConfig>(re);
            if (x != null)
            {
                NetworkCardConfig = x;
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }

        //TODO 后续更新为多个配置文件
        //var n = new NetworkCardConfig();
        //n.Ipv4s.Add(new Ipv4("192.168.3.3", "255.255.255.0"));
        //n.Ipv4s.Add(new Ipv4("192.168.4.3", "255.255.255.0"));
        //n.Ipv4s.Add(new Ipv4("192.168.5.3", "255.255.255.0"));
        //NetworkCardConfigs.Add(n);
    }

    #region 字段
    private readonly IDialogHostService dialogHost;
    private readonly string networkCardConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\NetworkCardConfig");
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
    NetworkCardConfig networkCardConfig = new()
    {
        Ipv4s = [new Ipv4(), new Ipv4(), new Ipv4(), new Ipv4(), new Ipv4(), new Ipv4()]
    };

    /// <summary>
    /// 网卡配置列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<NetworkCardConfig> networkCardConfigs = [];
    #endregion

    [RelayCommand]
    private async Task Execute(string obj)
    {
        switch (obj)
        {
            case "获取物理网卡信息": 获取物理网卡信息(); break;
            case "获取所有网卡信息": 获取所有网卡信息(); break;
            case "打开网络连接": await 打开网络连接(); break;
            case "ExportConfig": ExportConfig(); break;
            case "ImportConfig": ImportConfig(); break;
        }
    }

    /// <summary>
    /// 导入配置文件
    /// </summary>
    private void ImportConfig()
    {
        try
        {
            //配置文件目录
            Wu.Utils.IoUtil.Exists(networkCardConfigFolder);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonNCC)|*.jsonNCC",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                InitialDirectory = networkCardConfigFolder
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var x = JsonConvert.DeserializeObject<NetworkCardConfig>(xx);
            NetworkCardConfig = x;
            HcGrowlExtensions.Success("配置文件导入成功");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning("配置文件导入失败");
        }
    }

    /// <summary>
    /// 导出配置文件
    /// </summary>
    private void ExportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(networkCardConfigFolder);                                                                   //验证文件夹是否存在, 不存在则创建
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "请选择导出配置文件...",                             //对话框标题
                Filter = "json files(*.jsonNCC)|*.jsonNCC",                 //文件格式过滤器
                FilterIndex = 1,                                            //默认选中的过滤器
                FileName = "Default",                                       //默认文件名
                DefaultExt = "jsonNCC",                                     //默认扩展名
                InitialDirectory = networkCardConfigFolder,                                    //指定初始的目录
                OverwritePrompt = true,                                     //文件已存在警告
                AddExtension = true,                                        //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            var content = JsonConvert.SerializeObject(NetworkCardConfig);    //将当前的配置序列化为json字符串
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);              //保存文件
            HcGrowlExtensions.Success("配置文件导出成功");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning("配置文件导出失败");
        }
    }

    private static async Task 打开网络连接()
    {
        ProcessStartInfo startInfo = new("ncpa.cpl")
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);
        await Task.Delay(1000);
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
            if (!await 有管理员权限吗())
            {
                return;
            }

            List<ExecuteCmdResult> results = [];

            //TODO 测试用netsh和cmd都只能一次执行一条命令,还需要再优化
            #region 使用cmd执行
            string cmd = $"netsh interface ipv4 set address {nwc.NetConnectionId} static {NetworkCardConfig.Ipv4s[0].Address} {NetworkCardConfig.Ipv4s[0].SubnetMask}";
            results.Add(ExecuteCommands(cmd));
            // 添加额外的IP地址
            for (int i = 1; i < NetworkCardConfig.Ipv4s.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(NetworkCardConfig.Ipv4s[i].Address))
                {
                    continue;
                }
                cmd = $"netsh interface ipv4 add address {nwc.NetConnectionId} {NetworkCardConfig.Ipv4s[i].Address} {NetworkCardConfig.Ipv4s[i].SubnetMask}";
                results.Add(ExecuteCommands(cmd));
            }
            string result = string.Empty;
            foreach (var x in results)
            {
                if (x.Message != "\r\n")
                {
                    result += x.Message.Replace("\r\n", "") + "\r\n";
                }
            }
            if (result == string.Empty)
            {
                HcGrowlExtensions.Success($"设置成功...");
            }
            else
            {
                HcGrowlExtensions.Warning($"设置似乎出错惹...\r\n{result}");
            }
            //延时后更新网卡信息
            await Task.Delay(1000);
            nwc.UpdateInfo();
            #endregion
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

    /// <summary>
    /// 判断是否有管理员权限, 若无权限则请求提权
    /// </summary>
    /// <returns></returns>
    public async Task<bool> 有管理员权限吗()
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
        else
        {
            return true;
        }
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
            if (!await 有管理员权限吗())
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
            if (await 有管理员权限吗())
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
        //string error = process.StandardError?.ReadToEnd();
        process.WaitForExit();
        return new ExecuteCmdResult(process.ExitCode, output);
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
