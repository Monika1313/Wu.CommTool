using HandyControl.Controls;

namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class MrtuDeviceManagerLogViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public MrtuDeviceManagerLogViewModel() { }
    public MrtuDeviceManagerLogViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
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
        try
        {
            if (parameters != null && parameters.ContainsKey("Value"))
            {
                MrtuDeviceManager = parameters.GetValue<MrtuDeviceManager>("Value");
            }

            if (MrtuDeviceManager.SelectedMrtuDevice == null)
            {
                MrtuDeviceManager.SelectedMrtuDevice = MrtuDeviceManager.MrtuDevices.FirstOrDefault();
            }

            SelectedMrtuSerialPort = MrtuDeviceManager.MrtuSerialPorts.FirstOrDefault(x=>x.ComConfig.ComPort == MrtuDeviceManager.SelectedMrtuDevice.ComConfig.ComPort);
        }
        catch (Exception ex)
        {
            Growl.Error(ex.Message);
        }
    }
    #endregion


    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    /// <summary>
    /// ModbusRtu设备管理
    /// </summary>
    [ObservableProperty]
    MrtuDeviceManager mrtuDeviceManager = new();

    [ObservableProperty]
    MrtuSerialPort selectedMrtuSerialPort ;

    [ObservableProperty]
    object currentDto = new();
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
    #endregion
}
