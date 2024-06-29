namespace Wu.CommTool.Modules.ModbusTcp.ViewModels;

public partial class ModbusTcpViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private readonly IRegionManager regionManager;
    #endregion

    public ModbusTcpViewModel() { }
    public ModbusTcpViewModel(IContainerProvider provider, IDialogHostService dialogHost, IRegionManager regionManager) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        this.regionManager = regionManager;
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        //首次导航时, 跳转到初始页面
        if (!InitFlag)
        {
            InitFlag = true;
            this.regionManager.RequestNavigate(PrismRegionNames.ModbusTcpViewRegionName, nameof(ModbusTcpCustomFrameView), back =>
            {
                if (back.Error != null)
                {

                }
            });
        }
    }



    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    [ObservableProperty]
    object currentDto = new();

    /// <summary>
    /// 功能菜单
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MenuBar> menuBars =
        [
            new MenuBar() { Icon = "Number1", Title = "自定义帧", NameSpace = nameof(ModbusTcpCustomFrameView) },
            //new MenuBar() { Icon = "Number1", Title = "主站Master", NameSpace = nameof(ModbusTcpMasterView) },
            //new MenuBar() { Icon = "Number2", Title = "搜索设备", NameSpace = nameof(SearchDeviceView) },
            //new MenuBar() { Icon = "Number3", Title = "数据监控", NameSpace = nameof(DataMonitorView) },
            //new MenuBar() { Icon = "Number4", Title = "自动应答", NameSpace = nameof(AutoResponseView) },
        ];

    /// <summary>
    /// 初始化完成标志
    /// </summary>
    [ObservableProperty]
    bool initFlag;
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }

    /// <summary>
    /// 打开ModbusTcp客户端
    /// </summary>
    public void OnModbusTcpClient()
    {
        TcpClient client = new TcpClient("192.168.1.10", 502);
        //IModbusMaster master = ModbusIpMaster.CreateIp(client);
    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {
       
    }

    [RelayCommand]
    void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new DialogParameters();
        param.Add("Value", CurrentDto);
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    [RelayCommand]
    void Cancel()
    {
        //若窗口处于打开状态则关闭
        if (DialogHost.IsDialogOpen(DialogHostName))
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
    }

    /// <summary>
    /// 弹窗
    /// </summary>
    private void OpenDialogView()
    {
        
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private void Search()
    {
      
    }

    /// <summary>
    /// 页面切换
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void SelectedIndexChanged(MenuBar obj)
    {
        try
        {
            regionManager.RequestNavigate(PrismRegionNames.ModbusTcpViewRegionName, obj.NameSpace);
        }
        catch { }
    }
    #endregion
}
