namespace Wu.CommTool.Modules.Udp.Models;

public partial class UdpMessageData : MessageData
{
    public UdpMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
    }

    public UdpMessageData(string Content, DateTime dateTime, MessageType Type, string remoteEndPoint, string dataType) : base(Content, dateTime, Type)
    {
        RemoteEndPoint = remoteEndPoint;
        DataType = dataType;
    }

    [ObservableProperty] string remoteEndPoint;

    /// <summary>
    /// 消息数据类型
    /// </summary>
    [ObservableProperty] string dataType;

}
