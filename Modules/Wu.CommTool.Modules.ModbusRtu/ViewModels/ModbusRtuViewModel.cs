namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class ModbusRtuViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private readonly IRegionManager regionManager;
    #endregion

    public ModbusRtuViewModel() { }
    public ModbusRtuViewModel(IContainerProvider provider, IDialogHostService dialogHost, IRegionManager regionManager, ModbusRtuModel modbusRtuModel) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        this.regionManager = regionManager;
        ModbusRtuModel = modbusRtuModel;
    }

    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    /// <summary>
    /// ModbusRtuModel
    /// </summary>
    public ModbusRtuModel ModbusRtuModel { get => _ModbusRtuModel; set => SetProperty(ref _ModbusRtuModel, value); }
    private ModbusRtuModel _ModbusRtuModel;

    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// 初始化完成标志
    /// </summary>
    public bool InitFlag { get => _InitFlag; set => SetProperty(ref _InitFlag, value); }
    private bool _InitFlag;

    /// <summary>
    /// ModbusRtu功能菜单
    /// </summary>
    public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
    private ObservableCollection<MenuBar> _MenuBars =
        [
            new MenuBar() { Icon = "Number1", Title = "自定义帧", NameSpace = nameof(CustomFrameView) },
            new MenuBar() { Icon = "Number2", Title = "搜索设备", NameSpace = nameof(SearchDeviceView) },
            new MenuBar() { Icon = "Number3", Title = "数据监控", NameSpace = nameof(DataMonitorView) },
            new MenuBar() { Icon = "Number4", Title = "自动应答", NameSpace = nameof(AutoResponseView) },
#if DEBUG
            //new MenuBar() { Icon = "Number5", Title = "设备监控", NameSpace = nameof(MrtuDeviceMonitorView) },
#endif
        ];
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
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
            this.regionManager.RequestNavigate(PrismRegionNames.ModbusRtuViewRegionName, nameof(CustomFrameView), back =>
            {
                if (back.Error != null)
                {

                }
            });
        }
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
        try
        {
            DialogParameters param = new()
            {
                { "Value", CurrentDto }
            };
            //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
        }
        catch { }
    }
    #endregion


    #region 方法
    /// <summary>
    /// 页面切换
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void SelectedIndexChanged(MenuBar obj)
    {
        try
        {
            regionManager.RequestNavigate(PrismRegionNames.ModbusRtuViewRegionName, obj.NameSpace);
        }
        catch { }
    }
    #endregion
}
