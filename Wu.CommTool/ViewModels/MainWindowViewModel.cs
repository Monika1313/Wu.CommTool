using HandyControl.Controls;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wu.CommTool.DynamicTheme.Core;
using Wu.CommTool.Modules.MrtuSlave.Views;
using Wu.CommTool.Modules.TcpClient.Views;
using Wu.CommTool.Modules.TcpServer;
using Wu.CommTool.Modules.TcpServer.Views;
using Wu.CommTool.Modules.Udp.Views;

namespace Wu.CommTool.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IConfigureService
{
    #region    *****************************************  字段  *****************************************
    private readonly IRegionManager regionManager;
    private readonly IDialogHostService dialogHost;
    private IRegionNavigationJournal journal;
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly ThemeManager themeManager;
    #endregion *****************************************  字段  *****************************************


    #region    *****************************************  构造函数  *****************************************
    public MainWindowViewModel() { }
    public MainWindowViewModel(IRegionManager regionManager, IDialogHostService dialogHost, ThemeManager themeManager)
    {
        this.regionManager = regionManager;
        this.dialogHost = dialogHost;
        CreateMenuBar();
        AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;//AutoUpdater使用自定义的窗口
        this.themeManager = themeManager;
        themeManager.RegisterTheme("Dark", "Wu.CommTool.DynamicTheme.Resources", "DarkTheme.xaml");
        themeManager.RegisterTheme("Light", "Wu.CommTool.DynamicTheme.Resources", "LightTheme.xaml");
    }
    #endregion *****************************************  构造函数  *****************************************


    #region *****************************************  属性  *****************************************
    /// <summary>
    /// 标题
    /// </summary>
    [ObservableProperty]
    string title = "Wu.CommTool";

    /// <summary>
    /// 主菜单
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MenuBar> menuBars;

    /// <summary>
    /// 是否最大化
    /// </summary>
    [ObservableProperty]
    bool isMaximized = false;
    #endregion *****************************************  属性  *****************************************

    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            default:
                break;
        }
    }

    [RelayCommand]
    private async Task Support()
    {
        try
        {
            DialogParameters param = [];
            var dialogResult = await dialogHost.ShowDialog(nameof(SupportView), param);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 初始化配置
    /// </summary>
    public void Configure() => regionManager.Regions[PrismRegionNames.MainViewRegionName].RequestNavigate(App.AppConfig.DefaultView);//导航至页面

    /// <summary>
    /// 创建主菜单
    /// </summary>
    void CreateMenuBar()
    {
        MenuBars =
        [
            new() { Icon = "LanConnect", Title = "Modbus Rtu", NameSpace = nameof(ModbusRtuView) },
            new() { Icon = "LanConnect", Title = "ModbusRtu设备监控", NameSpace = nameof(MrtuDeviceMonitorView) },
#if DEBUG
            new() { Icon = "LanConnect", Title = "ModbusRtu从站", NameSpace = nameof(MrtuSlaveView) },
#endif

            new() { Icon = "LanConnect", Title = "Modbus Tcp", NameSpace = nameof(ModbusTcpView) },
            new() { Icon = "LanConnect", Title = "ModbusTcp设备监控", NameSpace = nameof(MtcpDeviceMonitorView) },
#if DEBUG
#endif
            new() { Icon = "LanConnect", Title = "串口", NameSpace = nameof(UartView) },
            new() { Icon = "LadyBug", Title = "Mqtt Server", NameSpace = nameof(MqttServerView) },
            new() { Icon = "Bug", Title = "Mqtt Client", NameSpace = nameof(MqttClientView) },
            new() { Icon = "ServerNetwork", Title = "Tcp Server", NameSpace = nameof(TcpServerView) },
            new() { Icon = "NetworkOutline", Title = "Tcp Client", NameSpace = nameof(TcpClientView) },
            new() { Icon = "LanConnect", Title = "UDP", NameSpace = nameof(UdpView) },
            new() { Icon = "ViewInAr", Title = "Json查看工具", NameSpace = "JsonToolView" },
            new() { Icon = "SwapHorizontal", Title = "转换工具", NameSpace = nameof(ConvertToolsView)},
            new() { Icon = "Lan", Title = "网络设置", NameSpace = nameof(NetworkToolView) },
#if DEBUG
#endif
            new() { Icon = "Clyde", Title = "关于", NameSpace = nameof(AboutView) },
        ];
    }

    /// <summary>
    /// 窗口导航
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    void Navigate(MenuBar obj)
    {
        if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
            return;
        try
        {
            App.AppConfig.DefaultView = obj.NameSpace ?? string.Empty;
            log.Info($"切换界面{obj.NameSpace}");
            regionManager.Regions[PrismRegionNames.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
            {
                journal = back.Context.NavigationService.Journal;
                if (back.Error != null)
                {
                    log.Error(back.Error.Message + "\n" + back.Error.InnerException?.Message);
                }
            });
        }
        catch (Exception ex)
        {
            log.Info($"窗口导航时出错惹:{ex.Message}");
            Growl.Error($"窗口导航时出错惹:{ex.Message}");
        }
    }

    /// <summary>
    /// 导航后退
    /// </summary>
    [RelayCommand]
    void GoBack()
    {
        if (journal != null && journal.CanGoBack)
            journal.GoBack();
    }

    /// <summary>
    /// 导航前进
    /// </summary>
    [RelayCommand]
    void GoForwar()
    {
        if (journal != null && journal.CanGoForward)
            journal.GoForward();
    }


    [RelayCommand]
    [property: JsonIgnore]
    private void AppUpdate()
    {
        AutoUpdater.InstalledVersion = new Version(AppInfo.Version);//当前的App版本
        AutoUpdater.HttpUserAgent = "AutoUpdater";
        AutoUpdater.ReportErrors = true;

        //设置为要下载更新文件的文件夹路径。如果没有提供，则默认为临时文件夹。
        AutoUpdater.DownloadPath = Path.Combine(Environment.CurrentDirectory, "AutoUpdater");

        //设置zip解压路径
        AutoUpdater.InstallationPath = Environment.CurrentDirectory;

        //AutoUpdater.ShowSkipButton = false;//禁用跳过
        //AutoUpdater.ShowRemindLaterButton = false;//禁用稍后提醒

        ////稍后提醒设置
        //AutoUpdater.LetUserSelectRemindLater = false;
        //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
        //AutoUpdater.RemindLaterAt = 2;
        //AutoUpdater.TopMost = true;

        //窗口使用的logo
        Uri iconUri = new("pack://application:,,,/Wu.CommTool;component/Images/Logo.png", UriKind.RelativeOrAbsolute);
        BitmapImage bitmap = new(iconUri);
        AutoUpdater.Icon = BitmapImage2Bitmap(bitmap);

        AutoUpdater.Start("http://salight.cn/Downloads/Wu.CommTool.Autoupdater.xml");
    }

    private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
    {
        using MemoryStream outStream = new();
        BitmapEncoder encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
        encoder.Save(outStream);
        Bitmap bitmap = new Bitmap(outStream);
        return new Bitmap(bitmap);
    }

    private async void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (args.IsUpdateAvailable)
            {
                var result = await dialogHost.Question("发现新版本", $"发现新版本 V{args.CurrentVersion}\n当前版本为 V{args.InstalledVersion}", "Root");

                #region 强制更新
                ////强制更新
                //if (args.Mandatory.Value)
                //{
                //}
                ////非强制更新
                //else
                //{
                //} 
                #endregion

                if (result.Result == ButtonResult.OK)
                {
                    try
                    {
                        if (AutoUpdater.DownloadUpdate(args))
                        {
                            Environment.Exit(0);
                        }
                    }
                    catch (Exception exception)
                    {
                        HcGrowlExtensions.Warning(exception.Message);
                    }
                }
            }
            else
            {
                //var result = await dialogHost.Question("更新检测", $"当前已是最新版本啦! V{AppInfo.Version}", "Root");
                HcGrowlExtensions.Success($"当前已是最新版本啦! V{AppInfo.Version}", "", 5);
            }
        }
        else
        {
            if (args.Error is WebException)
            {
                //var result = await dialogHost.Question("网络错误", "无法连接到服务器诶!", "Root");
                HcGrowlExtensions.Error("无法连接到更新服务器诶!");
            }
            else
            {
                HcGrowlExtensions.Warning(args.Error.Message);
            }
        }
    }


    [RelayCommand]
    [property: JsonIgnore]
    private void DarkTheme()
    {
        ResourceDictionary dark = new()
        {
            Source = new Uri("/Wu.CommTool.DynamicTheme.Resources;component/DarkTheme.xaml", UriKind.Relative)
        };
        Application.Current.Resources.MergedDictionaries.Add(dark);

        #region 添加单个资源
        //方法1 使用Add Remove
        //if (Application.Current.Resources.Contains("FontBrush"))
        //    Application.Current.Resources.Remove("FontBrush");
        //SolidColorBrush color1 = new(Colors.Pink);
        //Application.Current.Resources.Add("FontBrush", color1); 

        //方法2 用索引
        //Application.Current.Resources["FontBrush"] = color1; 
        #endregion
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void LightTheme()
    {
        ResourceDictionary light = new()
        {
            Source = new Uri("/Wu.CommTool.DynamicTheme.Resources;component/LightTheme.xaml", UriKind.Relative)
        };
        Application.Current.Resources.MergedDictionaries.Add(light);
    }
}
