namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class DataMonitorViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    public DataMonitorViewModel() { }
    public DataMonitorViewModel(IContainerProvider provider, IDialogHostService dialogHost, ModbusRtuModel modbusRtuModel) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ModburRtuDataWriteCommand = new DelegateCommand<ModbusRtuData>(ModburRtuDataWrite);
        QuickImportConfigCommand = new DelegateCommand<ConfigFile>(QuickImportConfig);
        OpenAnalyzeFrameViewCommand = new DelegateCommand<ModbusRtuMessageData>(OpenAnalyzeFrameView);
        CopyModbusRtuFrameCommand = new DelegateCommand<ModbusRtuMessageData>(CopyModbusRtuFrame);

        ModbusRtuModel = modbusRtuModel;
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        ModbusRtuModel.RefreshQuickImportList();
    }


    #region **************************************** 属性 ****************************************
    [ObservableProperty]
    object currentDto = new();

    /// <summary>
    /// ModbusRtuModel
    /// </summary>
    [ObservableProperty]
    ModbusRtuModel modbusRtuModel;

    /// <summary>
    /// 抽屉
    /// </summary>
    [ObservableProperty]
    OpenDrawers openDrawers = new();
    #endregion


    #region **************************************** 命令 ****************************************
    /// <summary>
    /// ModburRtu数据写入
    /// </summary>
    public DelegateCommand<ModbusRtuData> ModburRtuDataWriteCommand { get; private set; }

    /// <summary>
    /// 快速导入数据监控配置
    /// </summary>
    public DelegateCommand<ConfigFile> QuickImportConfigCommand { get; private set; }

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
    [RelayCommand]
    void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;

            case "ExportConfig": ModbusRtuModel.ExportConfig(); break;
            case "ImportConfig": ModbusRtuModel.ImportConfig(); break;
            case "RefreshQuickImportList": ModbusRtuModel.RefreshQuickImportList(); break;

            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
            case "OpenRightDrawer": OpenDrawers.RightDrawer = true; break;
            case "OpenDialogView": OpenDialogView(); break;
            case "OpenAutoRead": ModbusRtuModel.OpenAutoRead(); break;
            case "CloseAutoRead": ModbusRtuModel.CloseAutoRead(); break;

            case "OpenCom":                                             //打开串口
                ModbusRtuModel.OpenCom();
                break;
            case "CloseCom":
                ModbusRtuModel.CloseCom();                              //关闭串口
                break;
            case "OperateFilter": OperateFilter(); break;
            default: break;
        }
    }

    private void OperateFilter()
    {
        ModbusRtuModel.OperateFilter();
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
    [RelayCommand]
    void Save()
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
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private void Search()
    {
        try
        {
            UpdateLoading(true);

        }
        catch (Exception ex)
        {
            //aggregator.SendMessage($"{ex.Message}", "Main");
        }
        finally
        {
            UpdateLoading(false);
        }
    }

    private void ModburRtuDataWrite(ModbusRtuData data)
    {
        ModbusRtuModel.ModburRtuDataWrite(data);
    }

    private void QuickImportConfig(ConfigFile file)
    {
        ModbusRtuModel.QuickImportConfig(file);
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
