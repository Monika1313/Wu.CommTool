namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class EditFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion

    #region **************************************** 构造函数 ****************************************
    public EditFrameViewModel() { }
    public EditFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ModbusRtuFrameTypes =
        [
            ModbusRtuFrameType._0x01请求帧,
            ModbusRtuFrameType._0x02请求帧,
            ModbusRtuFrameType._0x03请求帧,
            ModbusRtuFrameType._0x04请求帧,
            ModbusRtuFrameType._0x05请求帧,
            ModbusRtuFrameType._0x06请求帧,
        ];
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
    object currentDto = new();

    /// <summary>
    /// 帧生成
    /// </summary>
    [ObservableProperty]
    ModbusRtuFrameCreator modbusRtuFrameCreator = new();

    /// <summary>
    /// 可生成的帧列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<ModbusRtuFrameType> modbusRtuFrameTypes;

    /// <summary>
    /// ModbusRtu帧
    /// </summary>
    [ObservableProperty]
    ModbusRtuFrame modbusRtuFrame = new();
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }

    [RelayCommand]
    void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new DialogParameters();
        param.Add("Value", ModbusRtuFrameCreator.FrameStr);
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
    #endregion
}
