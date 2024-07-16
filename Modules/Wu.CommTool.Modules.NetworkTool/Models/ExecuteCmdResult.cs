namespace Wu.CommTool.Modules.NetworkTool.Models;

public partial class ExecuteCmdResult : ObservableObject
{
    public ExecuteCmdResult(int exitCode, string message)
    {
        ExitCode = exitCode;
        Message = message;
    }



    /// <summary>
    /// 状态 false失败 true成功
    /// </summary>
    public bool Status { get => ExitCode == 0; }

    /// <summary>
    /// 退出代码 0为成功
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Status))]
    int exitCode;

    /// <summary>
    /// 消息
    /// </summary>
    [ObservableProperty]
    string  message;
}
