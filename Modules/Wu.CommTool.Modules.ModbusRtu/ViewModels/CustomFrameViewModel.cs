﻿namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class CustomFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;

    public string DialogHostName { get; set; }
    #endregion

    public CustomFrameViewModel() { }
    public CustomFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost, ModbusRtuModel modbusRtuModel) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        ModbusRtuModel = modbusRtuModel;

        OpenAnalyzeFrameViewCommand = new DelegateCommand<ModbusRtuMessageData>(OpenAnalyzeFrameView);

        SendCustomFrameCommand = new DelegateCommand<CustomFrame>(SendCustomFrame);
        CreateFrameCommand = new DelegateCommand<CustomFrame>(CreateFrame);
        DeleteLineCommand = new DelegateCommand<CustomFrame>(DeleteLine);

        //更新串口列表
        ModbusRtuModel.GetComPorts();
    }


    #region **************************************** 属性 ****************************************
    [ObservableProperty]
    object currentDto = new();

    [ObservableProperty]
    ModbusRtuModel modbusRtuModel;

    /// <summary>
    /// 抽屉
    /// </summary>
    [ObservableProperty]
    OpenDrawers _OpenDrawers = new();
    #endregion


    #region **************************************** 命令 ****************************************
    /// <summary>
    /// 打开帧解析界面
    /// </summary>
    public DelegateCommand<ModbusRtuMessageData> OpenAnalyzeFrameViewCommand { get; private set; }

    /// <summary>
    /// 发送自定义帧
    /// </summary>
    public DelegateCommand<CustomFrame> SendCustomFrameCommand { get; private set; }

    /// <summary>
    /// 生成帧命令
    /// </summary>
    public DelegateCommand<CustomFrame> CreateFrameCommand { get; private set; }

    /// <summary>
    /// 删除行命令
    /// </summary>
    public DelegateCommand<CustomFrame> DeleteLineCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;//打开左侧抽屉
            case "OpenDialogView": OpenDialogView(); break;             //打开弹窗
            case "SendCustomFrame": ModbusRtuModel.SendCustomFrame(); break;  //发送自定义帧
            case "OpenCom":                                             //打开串口
                ModbusRtuModel.OpenCom();
                OpenDrawers.LeftDrawer = false;                         //关闭左侧抽屉;
                break;
            case "CloseCom":
                ModbusRtuModel.CloseCom();                              //关闭串口
                break;
            case "帧生成器":
                OpenEditFrameView();
                break;
            case "新增行":
                ModbusRtuModel.AddNewCustomFrame();
                //ModbusRtuModel.CustomFrames.Add(new CustomFrame());
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

    /// <summary>
    /// 复制Modbus数据帧
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
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
                { "ModbusByteOrder", ModbusRtuModel.ByteOrder }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(AnalyzeFrameView), param, nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 打开帧生成器页面
    /// </summary>
    /// <param name="data"></param>
    private async void OpenEditFrameView()
    {
        try
        {
            DialogParameters param = new()
            {
                //{ "Value", data.ModbusRtuFrame }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(EditFrameView), param, nameof(ModbusRtuView));
            if (dialogResult.Result == ButtonResult.OK)
            {
                var x = dialogResult.Parameters.GetValue<string>("Value");
                if (!string.IsNullOrWhiteSpace(x))
                {
                    ModbusRtuModel.InputMessage = x;
                }
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 发送自定义帧
    /// </summary>
    /// <param name="frame"></param>
    private void SendCustomFrame(CustomFrame frame)
    {
        ModbusRtuModel.SendCustomFrame(frame);
    }

    /// <summary>
    /// 生成帧内容
    /// </summary>
    /// <param name="frame"></param>
    private async void CreateFrame(CustomFrame frame)
    {
        try
        {
            DialogParameters param = [];
            var dialogResult = await dialogHost.ShowDialog(nameof(EditFrameView), param, nameof(ModbusRtuView));
            if (dialogResult.Result == ButtonResult.OK)
            {
                var result = dialogResult.Parameters.GetValue<string>("Value");
                if (!string.IsNullOrWhiteSpace(result))
                {
                    frame.Frame = result;
                }
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 删除行
    /// </summary>
    /// <param name="frame"></param>
    private void DeleteLine(CustomFrame frame)
    {
        if (ModbusRtuModel.CustomFrames.Count > 1)
        {
            ModbusRtuModel.CustomFrames.Remove(frame);
        }
        else
        {
            ModbusRtuModel.ShowErrorMessage("不能删除最后一行...");
        }
    }
    #endregion
}
