namespace Wu.CommTool.Modules.JsonTool.ViewModels;

public partial class JsonDataViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    public JsonDataViewModel() { }
    public JsonDataViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
    }

    #region **************************************** 属性 ****************************************
    [ObservableProperty]
    MessageData currentDto;

    [ObservableProperty]
    ObservableCollection<JsonHeaderLogic> jsonHeaderLogics = [];
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
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

    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            CurrentDto = parameters.GetValue<MessageData>("Value");
            if (string.IsNullOrWhiteSpace(CurrentDto.Content))
                return;
            try
            {
                //json字符串转JToken
                var jtoken = JToken.Parse(CurrentDto.Content);
                var json = JsonHeaderLogic.FromJToken(jtoken);
                JsonHeaderLogics.Clear();
                JsonHeaderLogics.Add(json);
            }
            catch (Exception ex)
            {
                HcGrowlExtensions.Warning(ex.Message);
            }
        }
    }

    [RelayCommand]
    void Save()
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
    /// 查询数据
    /// </summary>
    private void Search()
    {

    }
    #endregion
}
