namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public class SearchDeviceViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    public SearchDeviceViewModel() { }
    public SearchDeviceViewModel(IContainerProvider provider, IDialogHostService dialogHost, ModbusRtuModel modbusRtuModel) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        ModbusRtuModel = modbusRtuModel;

        ExecuteCommand = new(Execute);
        SaveCommand = new DelegateCommand(Save);
        CancelCommand = new DelegateCommand(Cancel);
        BaudRateSelectionChangedCommand = new DelegateCommand<object>(BaudRateSelectionChanged);
        ParitySelectionChangedCommand = new DelegateCommand<object>(ParitySelectionChanged);
        OpenAnalyzeFrameViewCommand = new DelegateCommand<ModbusRtuMessageData>(OpenAnalyzeFrameView);
        CopyModbusRtuFrameCommand = new DelegateCommand<ModbusRtuMessageData>(CopyModbusRtuFrame);

    }

    private void ParitySelectionChanged(object obj)
    {
        ModbusRtuModel.ParitySelectionChanged(obj);
    }

    private void BaudRateSelectionChanged(object obj)
    {
        ModbusRtuModel.BaudRateSelectionChanged(obj);
    }

    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// ModbusRtuModel
    /// </summary>
    public ModbusRtuModel ModbusRtuModel { get => _ModbusRtuModel; set => SetProperty(ref _ModbusRtuModel, value); }
    private ModbusRtuModel _ModbusRtuModel;
    #endregion


    #region **************************************** 命令 ****************************************
    public DelegateCommand SaveCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }

    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }

    /// <summary>
    /// 波特率选框选项改变
    /// </summary>
    public DelegateCommand<object> BaudRateSelectionChangedCommand { get; private set; }

    /// <summary>
    /// 校验位选框选项修改
    /// </summary>
    public DelegateCommand<object> ParitySelectionChangedCommand { get; private set; }

    /// <summary>
    /// 打开帧解析界面
    /// </summary>
    public DelegateCommand<ModbusRtuMessageData> OpenAnalyzeFrameViewCommand { get; private set; }

    /// <summary>
    /// 复制Modbus帧信息
    /// </summary>
    public DelegateCommand<ModbusRtuMessageData> CopyModbusRtuFrameCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
            case "SearchDevices": ModbusRtuModel.SearchDevices(); break;
            case "StopSearchDevices": ModbusRtuModel.StopSearchDevices(); break;
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
        //Search();
    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {
       
    }


    /// <summary>
    /// 保存
    /// </summary>
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

    /// <summary>
    /// 取消
    /// </summary>
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
       
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private void Search()
    {
       
    }

    /// <summary>
    /// 打开解析帧页面
    /// </summary>
    /// <param name="data"></param>
    private async void OpenAnalyzeFrameView(ModbusRtuMessageData data)
    {
        try
        {
            if (data == null || data.ModbusRtuFrame == null)
            {
                return;
            }
            DialogParameters param = new()
            {
                { "Value", data.ModbusRtuFrame },
                { "ModbusByteOrder",ModbusRtuModel.ByteOrder }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(AnalyzeFrameView), param, nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 复制Modbus数据帧
    /// </summary>
    /// <param name="obj"></param>
    private void CopyModbusRtuFrame(ModbusRtuMessageData obj)
    {
        try
        {
            string xx = string.Empty;
            foreach (var item in obj.MessageSubContents)
            {
                xx += $"{item.Content} ";
            }
            Clipboard.SetDataObject(xx);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }
    #endregion
}
