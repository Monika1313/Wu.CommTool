namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class UartViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private readonly string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\UartConfig");
    private readonly string configExtension = "uartc";
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public UartViewModel() { }
    public UartViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        UartModel.GetComPorts();//更新串口列表
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

    [ObservableProperty] object currentDto = new();

    [ObservableProperty] UartModel uartModel = new();

    [ObservableProperty] OpenDrawers openDrawers = new();

    #endregion **************************************** 属性 ****************************************



    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenDialogView": OpenDialogView(); break;
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
            case "OpenCom":                    //打开串口
                UartModel.OpenCom();
                OpenDrawers.LeftDrawer = false;//关闭左侧抽屉;
                break;
            case "CloseCom":
                _ = UartModel.CloseCom();                              //关闭串口
                break;
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
            Growl.Warning(ex.Message);
        }
    }
    #endregion

    /// <summary>
    /// 自定义帧 编辑
    /// </summary>
    [RelayCommand]
    public async Task EditCustomFrame(UartCustomFrame model)
    {
        try
        {
            //添加参数
            DialogParameters param = new();
            if (model != null)
                param.Add("Value", model);

            var dialogResult = await dialogHost.ShowDialog(nameof(EditUartCustomnFrameView), param, EditUartCustomnFrameView.ViewName);

            if (dialogResult.Result == ButtonResult.OK)
            {
                try
                {
                    //从结果中获取数据
                    var resultDto = dialogResult.Parameters.GetValue<UartCustomFrame>("Value");
                }
                catch (Exception ex)
                {
                    Growl.Error(ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
        }
    }

    #region ******************************  配置文件  ******************************
    /// <summary>
    /// 导出配置文件
    /// </summary>
    [RelayCommand]
    public void ExportConfig()
    {
        try
        {
            //配置文件目录
            Wu.Utils.IoUtil.Exists(configFolder);
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",        //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",    //文件格式过滤器
                FilterIndex = 1,                     //默认选中的过滤器
                FileName = "Default",                //默认文件名
                DefaultExt = $"{configExtension}",              //默认扩展名
                InitialDirectory = configFolder,            //指定初始的目录
                OverwritePrompt = true,                                                  //文件已存在警告
                AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(UartModel);
            //保存文件
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
            HcGrowlExtensions.Success($"配置导出成功 {Path.GetFileNameWithoutExtension(sfd.FileName)}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置导出失败 {ex.Message}");
        }
    }

    /// <summary>
    /// 导入配置文件
    /// </summary>
    [RelayCommand]
    public void ImportConfig()
    {
        try
        {
            //配置文件目录
            Wu.Utils.IoUtil.Exists(configFolder);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入自动应答配置文件...",                                //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",    //文件格式过滤器
                FilterIndex = 1,                                                    //默认选中的过滤器
                InitialDirectory = configFolder
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var xxx = JsonConvert.DeserializeObject<UartModel>(xx)!;
            var importUartModel = JsonConvert.DeserializeObject<UartModel>(xx)!;
            UpdateUartModel(importUartModel);//更新当前模型
            HcGrowlExtensions.Success($"配置导入成功 {Path.GetFileNameWithoutExtension(dlg.FileName)}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置导入失败 {ex.Message}");
        }
    }

    private void UpdateUartModel(UartModel import)
    {
        UartModel.ComConfig = import.ComConfig;
        UartModel.ComConfig.ComPort = UartModel.ComPorts.FirstOrDefault(x => x.ComId == import.ComConfig.ComPort.ComId);
        UartModel.IsPause = import.IsPause;
        UartModel.CrcMode = import.CrcMode;
        UartModel.SendDataFormat = import.SendDataFormat;
        UartModel.ReceiveDataFormat = import.ReceiveDataFormat;
        UartModel.ByteOrder = import.ByteOrder;

        int old = UartModel.CustomFrames.Count;

        foreach (var x in import.CustomFrames)
        {
            UartModel.CustomFrames.Add(x);
        }

        for (int i = 0; i < old + 3; i++)
        {
            var xxx = UartModel.CustomFrames.First();
            UartModel.CustomFrames.Remove(xxx);
        }

    }
    #endregion
}
