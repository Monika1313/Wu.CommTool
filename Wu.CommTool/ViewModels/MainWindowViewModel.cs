﻿namespace Wu.CommTool.ViewModels;

public class MainWindowViewModel : BindableBase, IConfigureService
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get => _Title; set => SetProperty(ref _Title, value); }
    private string _Title = "Wu";
    private readonly IRegionManager regionManager;
    private IRegionNavigationJournal journal;
    public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    public MainWindowViewModel()
    {

    }
    public MainWindowViewModel(IRegionManager regionManager)
    {
        this.regionManager = regionManager;

        CreateMenuBar();
        ExecuteCommand = new DelegateCommand<string>(Execute);
        NavigateCommand = new DelegateCommand<MenuBar>(Navigate);

        GoBackCommand = new DelegateCommand(() =>
        {
            if (journal != null && journal.CanGoBack)
                journal.GoBack();
        });
        GoForwarCommand = new DelegateCommand(() =>
        {
            if (journal != null && journal.CanGoForward)
                journal.GoForward();
        });
    }

    #region *****************************************  属性  *****************************************
    /// <summary>
    /// 主菜单
    /// </summary>
    public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
    private ObservableCollection<MenuBar> _MenuBars;

    /// <summary>
    /// 是否最大化
    /// </summary>
    public bool IsMaximized { get => _IsMaximized; set => SetProperty(ref _IsMaximized, value); }
    private bool _IsMaximized = false;
    #endregion

    #region 命令
    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }

    /// <summary>
    /// 导航命令
    /// </summary>
    public DelegateCommand<MenuBar> NavigateCommand { get; private set; }

    /// <summary>
    /// 导航返回
    /// </summary>
    public DelegateCommand GoBackCommand { get; private set; }

    /// <summary>
    /// 导航前进
    /// </summary>
    public DelegateCommand GoForwarCommand { get; private set; }

    #endregion

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
        MenuBars = new ObservableCollection<MenuBar>()
        {
            new() { Icon = "LanConnect", Title = "Modbus Rtu", NameSpace = nameof(ModbusRtuView) },
            new() { Icon = "LanConnect", Title = "Modbus Tcp", NameSpace = nameof(ModbusTcpView) },

            new() { Icon = "LadyBug", Title = "Mqtt Server", NameSpace = nameof(MqttServerView) },
            new() { Icon = "Bug", Title = "Mqtt Client", NameSpace = nameof(MqttClientView) },
            new() { Icon = "ViewInAr", Title = "Json查看工具", NameSpace = "JsonToolView" },
            new() { Icon = "SwapHorizontal", Title = "转换工具", NameSpace = nameof(ConvertToolsView)},
#if DEBUG
            new() { Icon = "Lan", Title = "网络设置", NameSpace = nameof(NetworkToolView) },
#endif
            new() { Icon = "Clyde", Title = "关于", NameSpace = nameof(AboutView) },
        };
    }

    /// <summary>
    /// 窗口导航
    /// </summary>
    /// <param name="obj"></param>
    private void Navigate(MenuBar obj)
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
}
