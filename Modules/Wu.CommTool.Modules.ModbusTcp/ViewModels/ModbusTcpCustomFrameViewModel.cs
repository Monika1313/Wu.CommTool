namespace Wu.CommTool.Modules.ModbusTcp.ViewModels;

public partial class ModbusTcpCustomFrameViewModel : NavigationViewModel
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
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {

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

    [ObservableProperty]
    MtcpMaster mtcpMaster = new();

    [ObservableProperty]
    OpenDrawers openDrawers = new();
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
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
        //    var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning(ex.Message);
        //}
    }
    #endregion
}
