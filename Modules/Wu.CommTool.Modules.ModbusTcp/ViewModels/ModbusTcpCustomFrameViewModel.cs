namespace Wu.CommTool.Modules.ModbusTcp.ViewModels;

public partial class ModbusTcpCustomFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion

    #region **************************************** 构造函数 ****************************************
    public ModbusTcpCustomFrameViewModel() { }
    public ModbusTcpCustomFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ExecuteCommand = new(Execute);
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        //Search();
    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {
        
    }
    #endregion

    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// ModbusTcp
    /// </summary>
    public MtcpMaster MtcpMaster { get => _MtcpMaster; set => SetProperty(ref _MtcpMaster, value); }
    private MtcpMaster _MtcpMaster = new();

    /// <summary>
    /// 抽屉
    /// </summary>
    public OpenDrawers OpenDrawers { get => _OpenDrawers; set => SetProperty(ref _OpenDrawers, value); }
    private OpenDrawers _OpenDrawers = new();
    #endregion


    #region **************************************** 命令 ****************************************
    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }




    /// <summary>
    /// 保存
    /// </summary>
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

    /// <summary>
    /// 取消
    /// </summary>
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
        //try
        //{
        //    DialogParameters param = new()
        //    {
        //        { "Value", CurrentDto }
        //    };
        //    //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning(ex.Message);
        //}
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private void Search()
    {
      
    }
    #endregion
}
