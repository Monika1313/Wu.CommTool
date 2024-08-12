namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class MrtuDeviceEditViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public MrtuDeviceEditViewModel() { }
    public MrtuDeviceEditViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
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
    public async void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            MrtuDevice = parameters.GetValue<MrtuDevice>("Value");
        }

        var task = new Task(GetComPortsAndSet);
        task.Start();
    }
    #endregion


    

    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    [ObservableProperty]
    MrtuDevice mrtuDevice = new();

    [ObservableProperty]
    ObservableCollection<ComPort> comPorts=[];
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
            { "Value", MrtuDevice }
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

    [RelayCommand]
    [property: JsonIgnore]
    private void GetComPortsAndSet()
    {
        var oldSelected = MrtuDevice.ComConfig?.ComPort?.Port;
        ComPorts = new ObservableCollection<ComPort>(ModbusUtils.GetComPorts());
        var x = ComPorts.FirstOrDefault(x => x.Port == oldSelected);
        if (x != null)
        {
            MrtuDevice.ComConfig.ComPort = x;
        }
    }
    #endregion
}
