namespace Wu.CommTool.Modules.MrtuSlave.ViewModels.DialogViewModels;

public partial class MrtuSlaveLogViewModel : NavigationViewModel, IDialogHostAware, IDialogAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public MrtuSlaveLogViewModel() { }
    public MrtuSlaveLogViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
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
                MrtuSlaveModel = parameters.GetValue<MrtuSlaveModel>("Value");
            }

            //if (MrtuDeviceManager.SelectedMrtuDevice == null)
            //{
            //    MrtuDeviceManager.SelectedMrtuDevice = MrtuDeviceManager.MrtuDevices.FirstOrDefault();
            //}

            //SelectedMrtuSerialPort = MrtuDeviceManager.MrtuSerialPorts.FirstOrDefault(x => x.ComConfig.ComPort == MrtuDeviceManager.SelectedMrtuDevice.ComConfig.ComPort);
        }
        catch (Exception ex)
        {
            Growl.Error(ex.Message);
        }
    }
    #endregion


    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    public string Title => DialogHostName;

    [ObservableProperty]
    object currentDto = new();

    [ObservableProperty] MrtuSlaveModel mrtuSlaveModel;

    public event Action<IDialogResult> RequestClose;

    #endregion **************************************** 属性 ****************************************


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

    [RelayCommand]
    private void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        DialogParameters param = new()
        {
            { "Value", CurrentDto }
        };
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));//关闭窗口,并返回参数
    }

    [RelayCommand]
    private void Cancel()
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
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
        return;
    }
    #endregion
}
