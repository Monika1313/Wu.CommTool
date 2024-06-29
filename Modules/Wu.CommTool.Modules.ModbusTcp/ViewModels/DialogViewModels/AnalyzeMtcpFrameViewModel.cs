namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DialogViewModels;

public partial class AnalyzeMtcpFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public AnalyzeMtcpFrameViewModel() { }
    public AnalyzeMtcpFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        //IRelayCommand
        //SearchCommand
        //CurrentDto
    }

    [RelayCommand]
    public void Test()
    {

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
        //if (parameters != null && parameters.ContainsKey("Value"))
        //{
        //    var oldDto = parameters.GetValue<Dto>("Value");
        //    var getResult = await service.GetDataAsync(oldDto);
        //    if (getResult != null && getResult.Status)
        //    {
        //        CurrentDto = getResult.Result;
        //    }
        //}
    }
    #endregion


    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    [ObservableProperty]
    object currentDto = new();
    #endregion **************************************** 属性 ****************************************


    #region **************************************** 命令 ****************************************

    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
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
        DialogParameters param = new DialogParameters();
        param.Add("Value", CurrentDto);
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    /// <summary>
    /// 取消
    /// </summary>
    //[RelayCommand]
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
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        try
        {
            UpdateLoading(true);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
        finally
        {
            UpdateLoading(false);
        }
    }
    #endregion
}
