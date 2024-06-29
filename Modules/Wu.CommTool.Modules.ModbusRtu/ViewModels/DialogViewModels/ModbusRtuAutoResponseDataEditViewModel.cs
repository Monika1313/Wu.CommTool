namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class ModbusRtuAutoResponseDataEditViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    public ModbusRtuAutoResponseDataEditViewModel() { }
    public ModbusRtuAutoResponseDataEditViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ExecuteCommand = new(Execute);
    }

    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    public ModbusRtuAutoResponseData CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private ModbusRtuAutoResponseData _CurrentDto = new();
    #endregion


    #region **************************************** 命令 ****************************************
    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
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
    public async void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            var obj = parameters.GetValue<ModbusRtuAutoResponseData>("Value");
            CurrentDto.Name = obj.Name;
            CurrentDto.ResponseTemplate = obj.ResponseTemplate;
            CurrentDto.MateTemplate = obj.MateTemplate;
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
    private async void OpenDialogView()
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

        }
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private async void Search()
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
    #endregion
}
