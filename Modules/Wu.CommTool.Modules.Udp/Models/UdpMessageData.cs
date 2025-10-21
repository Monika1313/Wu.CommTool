namespace Wu.CommTool.Modules.Udp.Models;

public partial class UdpMessageData : MessageData
{
    public UdpMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
    }

    public UdpMessageData(string Content, DateTime dateTime, string remoteEndPoint, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
        RemoteEndPoint = remoteEndPoint;
    }

    [ObservableProperty] string remoteEndPoint;
}
