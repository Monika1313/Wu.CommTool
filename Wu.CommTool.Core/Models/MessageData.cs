namespace Wu.CommTool.Core.Models;

/// <summary>
/// 用于页面展示的消息数据
/// </summary>
public partial class MessageData : ObservableObject
{
    public MessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "")
    {
        this.Type = Type;
        this.Content = Content;
        this.Time = dateTime;
        this.Title = Title;
    }

    /// <summary>
    /// 时间
    /// </summary>
    [ObservableProperty]
    DateTime time;
    

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get => _Type; set => SetProperty(ref _Type, value); }
    private MessageType _Type;

    /// <summary>
    /// 标题
    /// </summary>
    [ObservableProperty]
    string title = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    [ObservableProperty]
    string content = string.Empty;
}
