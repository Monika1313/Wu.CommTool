namespace Wu.CommTool.Modules.ConvertTools.ViewModels;

public partial class ValueConvertViewModel : NavigationViewModel
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    public string DialogHostName { get; set; }
    #endregion

    public ValueConvertViewModel() { }
    public ValueConvertViewModel(IContainerProvider provider) : base(provider)
    {
        this.provider = provider;
    }

    #region **************************************** 属性 ****************************************
    [ObservableProperty]
    object currentDto = new();

    /// <summary>
    /// 值转换
    /// </summary>
    [ObservableProperty]
    ValueCvt valueCvt = new();
    #endregion



    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
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
    /// 弹窗
    /// </summary>
    private void OpenDialogView()
    {

    }

    #endregion
}
