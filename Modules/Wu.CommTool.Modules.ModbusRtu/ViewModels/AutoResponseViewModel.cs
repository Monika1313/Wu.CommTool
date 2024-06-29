namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class AutoResponseViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion

    public AutoResponseViewModel() { }
    public AutoResponseViewModel(IContainerProvider provider, IDialogHostService dialogHost, ModbusRtuModel modbusRtuModel) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        ModbusRtuModel = modbusRtuModel;

        //导入默认自动应答配置
        try
        {
            var xx = Core.Common.Utils.ReadJsonFile(Path.Combine(ModbusRtuModel.ModbusRtuAutoResponseConfigDict, "Default.jsonARC"));
            ModbusRtuModel.MosbusRtuAutoResponseDatas = JsonConvert.DeserializeObject<ObservableCollection<ModbusRtuAutoResponseData>>(xx)!;
            ModbusRtuModel.RefreshModbusRtuDataDataView();//更新数据视图
        }
        catch (Exception ex)
        {
            ModbusRtuModel.ShowErrorMessage(ex.Message);
        }
    }


    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    [ObservableProperty]
    object currentDto = new();

    /// <summary>
    /// ModbusRtuModel
    /// </summary>
    [ObservableProperty]
    ModbusRtuModel modbusRtuModel;

    /// <summary>
    /// OpenDrawers
    /// </summary>
    [ObservableProperty]
    OpenDrawers openDrawers = new();
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
            case "OpenDialogView": OpenDialogView(); break;
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
            case "AddMosbusRtuAutoResponseData": ModbusRtuModel.AddMosbusRtuAutoResponseData(); break;         //添加新的应答模板
            case "ExportAutoResponseConfig": ModbusRtuModel.ExportAutoResponseConfig(); break;
            case "ImportAutoResponseConfig": ModbusRtuModel.ImportAutoResponseConfig(); break;
            case "AutoResponseOff":                             //关闭自动应答
                ModbusRtuModel.AutoResponseOff();
                break;
            case "CloseComAndAutoResponse":                             //关闭com和自动应答
                ModbusRtuModel.CloseCom();
                ModbusRtuModel.AutoResponseOff();
                break;
            case "AutoResponseOn":                              //打开自动应答
                ModbusRtuModel.AutoResponseOn();
                break;
            case "OpenComAndAutoResponse":                              //打开com和自动应答
                ModbusRtuModel.OpenCom();
                ModbusRtuModel.AutoResponseOn();
                break;
            case "OpenCom":                                             //打开串口
                ModbusRtuModel.OpenCom();
                break;
            case "CloseCom":
                ModbusRtuModel.CloseCom();                              //关闭串口
                break;
            default: break;
        }
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

    /// <summary>
    /// 打开自动应答编辑界面
    /// </summary>
    [RelayCommand]
    private async Task OpenMosbusRtuAutoResponseDataEditView(ModbusRtuAutoResponseData obj)
    {
        try
        {
            if (obj == null)
                return ;
            //添加参数
            DialogParameters param = new()
            {
                { "Value", obj }
            };

            var dialogResult = await dialogHost.ShowDialog(nameof(ModbusRtuAutoResponseDataEditView), param, ModbusRtuView.ViewName);

            if (dialogResult.Result == ButtonResult.OK)
            {
                var resultDto = dialogResult.Parameters.GetValue<ModbusRtuAutoResponseData>("Value");
                if (resultDto == null)
                {
                    return;
                }
                obj.Name = resultDto.Name;
                obj.MateTemplate = resultDto.MateTemplate;
                obj.ResponseTemplate = resultDto.ResponseTemplate;
            }
            else if (dialogResult.Result == ButtonResult.Abort)
            {
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 打开解析帧页面
    /// </summary>
    /// <param name="data"></param>
    [RelayCommand]
    private async Task OpenAnalyzeFrameView(ModbusRtuMessageData data)
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
    #endregion
}
