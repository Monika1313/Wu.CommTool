//namespace Wu.CommTool.Modules.ModbusRtu.Models;

///// <summary>
///// 用于页面展示的消息数据
///// </summary>
//public class MessageData : BindableBase
//{
//    public MessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "")
//    {
//        this.Type = Type;
//        this.Content = Content;
//        this.Time = dateTime;
//        this.Title = Title;
//    }
//    /// <summary>
//    /// 时间
//    /// </summary>
//    public DateTime Time { get => _Time; set => SetProperty(ref _Time, value); }
//    private DateTime _Time;

//    /// <summary>
//    /// 消息类型
//    /// </summary>
//    public Enums.MessageType Type { get => _Type; set => SetProperty(ref _Type, value); }
//    private Enums.MessageType _Type;

//    /// <summary>
//    /// 标题
//    /// </summary>
//    public string Title { get => _Title; set => SetProperty(ref _Title, value); }
//    private string _Title = string.Empty;

//    /// <summary>
//    /// 消息内容
//    /// </summary>
//    public string Content { get => _Content; set => SetProperty(ref _Content, value); }
//    private string _Content = string.Empty;
//}
