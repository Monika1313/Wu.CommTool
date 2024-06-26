﻿namespace Wu.CommTool.Modules.Message.ViewModels;

public partial class MsgViewModel : ObservableObject, IDialogHostAware
{
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
