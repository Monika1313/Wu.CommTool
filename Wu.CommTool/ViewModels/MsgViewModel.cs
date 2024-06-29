using MaterialDesignThemes.Wpf;
using Prism.Services.Dialogs;

namespace Wu.CommTool.ViewModels;
public partial class MsgViewModel : ObservableObject, IDialogHostAware
{
    public MsgViewModel()
    {
    }

    /// <summary>
    /// 标题
    /// </summary>
    [ObservableProperty]
    string title;

    /// <summary>
    /// 内容
    /// </summary>
    [ObservableProperty]
    string content;

    public string DialogHostName { get; set; } = "Root";

    public void OnDialogOpened(IDialogParameters parameters)
    {
        //接收参数
        if (parameters.ContainsKey(nameof(Title)))
            Title = parameters.GetValue<string>(nameof(Title));
        if (parameters.ContainsKey(nameof(Content)))
            Content = parameters.GetValue<string>(nameof(Content));
    }

    [RelayCommand]
    void Cancel()
    {
        //若窗口处于打开状态则关闭
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
    }

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
