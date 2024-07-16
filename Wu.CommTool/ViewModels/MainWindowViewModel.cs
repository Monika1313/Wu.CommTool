namespace Wu.CommTool.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IConfigureService
{
    #region    *****************************************  字段  *****************************************
    private readonly IRegionManager regionManager;
    private IRegionNavigationJournal journal;
    public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
    #endregion *****************************************  字段  *****************************************


    #region    *****************************************  构造函数  *****************************************
    public MainWindowViewModel() { }
    public MainWindowViewModel(IRegionManager regionManager)
    {
        this.regionManager = regionManager;
        CreateMenuBar();
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
            new() { Icon = "LanConnect", Title = "Modbus Tcp", NameSpace = nameof(ModbusTcpView) },

            new() { Icon = "LadyBug", Title = "Mqtt Server", NameSpace = nameof(MqttServerView) },
            new() { Icon = "Bug", Title = "Mqtt Client", NameSpace = nameof(MqttClientView) },
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
}
