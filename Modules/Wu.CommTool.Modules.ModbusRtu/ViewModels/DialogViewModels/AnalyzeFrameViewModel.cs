namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class AnalyzeFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    
    #endregion

    #region **************************************** 构造函数 ****************************************
    public AnalyzeFrameViewModel() { }
    public AnalyzeFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
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
        if (parameters != null && parameters.ContainsKey("ModbusByteOrder"))
        {
            ModbusByteOrder = parameters.GetValue<ModbusByteOrder>("ModbusByteOrder");
        }
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            ModbusRtuFrame = parameters.GetValue<ModbusRtuFrame>("Value");
            if (ModbusRtuFrame.RegisterValues?.Length > 0)
            {
                ModbusRtuDatas.AddRange(Enumerable.Range(0, ModbusRtuFrame.RegisterValues.Length / 2).Select(x => new ModbusRtuData()));

            }

            //将读取的数据写入
            for (int i = 0; i < ModbusRtuDatas.Count; i++)
            {
                ModbusRtuDatas[i].Location = i * 2;         //在源字节数组中的起始位置 源字节数组为完整的数据帧,帧头部分3字节 每个值为1个word2字节
                ModbusRtuDatas[i].ModbusByteOrder = ModbusByteOrder; //字节序
                ModbusRtuDatas[i].OriginValue = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(ModbusRtuFrame.RegisterValues, 2 * i);
                ModbusRtuDatas[i].OriginBytes = ModbusRtuFrame.RegisterValues;        //源字节数组
            }
        }
    }
    #endregion

    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    [ObservableProperty]
    object currentDto = new();

    [ObservableProperty]
    ModbusRtuFrame modbusRtuFrame;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty]
    ModbusByteOrder modbusByteOrder = ModbusByteOrder.DCBA;

    /// <summary>
    /// ModbusRtu的寄存器值
    /// </summary>
    [ObservableProperty]
    ObservableCollection<ModbusRtuData> modbusRtuDatas = [];
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
        
    }

    /// <summary>
    /// 字节序切换
    /// </summary>
    /// <param name="order"></param>
    [RelayCommand]
    private void ModbusByteOrderChanged(ModbusByteOrder? order)
    {
        try
        {
            if (order == null)
            {
                return;
            }


            foreach (var item in ModbusRtuDatas)
            {
                item.ModbusByteOrder = ModbusByteOrder;
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }
    #endregion
}
