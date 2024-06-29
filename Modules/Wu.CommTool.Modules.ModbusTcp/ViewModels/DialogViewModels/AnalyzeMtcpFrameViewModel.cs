namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DialogViewModels;

public partial class AnalyzeMtcpFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public AnalyzeMtcpFrameViewModel() { }
    public AnalyzeMtcpFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
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
        //if (parameters != null && parameters.ContainsKey("ModbusByteOrder"))
        //{
        //    ModbusByteOrder = parameters.GetValue<ModbusByteOrder>("ModbusByteOrder");
        //}
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            MtcpFrame = parameters.GetValue<MtcpFrame>("Value");
            //if (ModbusRtuFrame.RegisterValues?.Length > 0)
            //{
            //    ModbusRtuDatas.AddRange(Enumerable.Range(0, ModbusRtuFrame.RegisterValues.Length / 2).Select(x => new ModbusRtuData()));
            //}

            //将读取的数据写入
            //for (int i = 0; i < ModbusRtuDatas.Count; i++)
            //{
            //    ModbusRtuDatas[i].Location = i * 2;         //在源字节数组中的起始位置 源字节数组为完整的数据帧,帧头部分3字节 每个值为1个word2字节
            //    ModbusRtuDatas[i].ModbusByteOrder = ModbusByteOrder; //字节序
            //    ModbusRtuDatas[i].OriginValue = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(ModbusRtuFrame.RegisterValues, 2 * i);
            //    ModbusRtuDatas[i].OriginBytes = ModbusRtuFrame.RegisterValues;        //源字节数组
            //}
        }
    }
    #endregion


    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    [ObservableProperty]
    MtcpFrame mtcpFrame;

    /// <summary>
    /// 用于响应字节序修改后的显示
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MtcpSubMessageData> mtcpSubMessageDatas = [];
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
        DialogParameters param = [];
        //param.Add("Value", CurrentDto);
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
