
namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class MrtuDeviceMonitorViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private static readonly ILog log = LogManager.GetLogger(typeof(MrtuDeviceMonitorViewModel));
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public MrtuDeviceMonitorViewModel() { }
    public MrtuDeviceMonitorViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
    }

    /// <summary>
    /// 导航至该页面时执行
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
    object currentDto = new();

    /// <summary>
    /// ModbusRtu设备管理
    /// </summary>
    [ObservableProperty]
    MrtuDeviceManager mrtuDeviceManager = new();

    #endregion **************************************** 属性 ****************************************


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            default: break;
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new()
        {
            { "Value", CurrentDto }
        };
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    [RelayCommand]
    private void Cancel()
    {
        //若窗口处于打开状态则关闭
        if (DialogHost.IsDialogOpen(DialogHostName))
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
    }

    /// <summary>
    /// 配置ModbusRtu设备
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private async Task ConfigMrtuDevice(MrtuDevice mrtuDevice)
    {
        try
        {
            DialogParameters param = new()
            {
                { "Value", mrtuDevice }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(ConfigMrtuDeviceView), param, nameof(ModbusRtuView));

            if (dialogResult.Result == ButtonResult.OK)
            {
                //更新配置
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }


    [RelayCommand]
    [property: JsonIgnore]
    private void Config()
    {

    }


    #endregion
}
