using Wu.CommTool.Core.Extensions;

namespace Wu.CommTool.Modules.ConvertTools.ViewModels;

public class ValueConvertViewModel : NavigationViewModel
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    public ValueConvertViewModel() { }
    public ValueConvertViewModel(IContainerProvider provider) : base(provider)
    {
        this.provider = provider;

        ExecuteCommand = new(Execute);
        SaveCommand = new DelegateCommand(Save);
        CancelCommand = new DelegateCommand(Cancel);
    }

    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// 值转换
    /// </summary>
    public ValueCvt ValueCvt { get => _ValueCvt; set => SetProperty(ref _ValueCvt, value); }
    private ValueCvt _ValueCvt = new();
    #endregion


    #region **************************************** 命令 ****************************************
    public DelegateCommand SaveCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }

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
    private void Save()
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
    #endregion
}
