namespace Wu.CommTool.Modules.Message.ViewModels;

public partial class MsgViewModel : ObservableObject, IDialogHostAware
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get => _Title; set => SetProperty(ref _Title, value); }
    private string _Title;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get => _Content; set => SetProperty(ref _Content, value); }
    private string _Content;


    public string DialogHostName { get; set; } = "Root";

    public void OnDialogOpened(IDialogParameters parameters)
    {
        //接收参数
        if (parameters.ContainsKey(nameof(Title)))
            Title = parameters.GetValue<string>(nameof(Title));
        if (parameters.ContainsKey(nameof(Content)))
            Content = parameters.GetValue<string>(nameof(Content));
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    void Cancel()
    {
        //若窗口处于打开状态则关闭
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
    }

    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        DialogParameters param = new DialogParameters();
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }
}
