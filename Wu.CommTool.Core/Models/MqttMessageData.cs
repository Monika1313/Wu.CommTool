namespace Wu.CommTool.Core.Models;

public class MqttMessageData : MessageData
{
    public MqttMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
    }
    public MqttMessageData(string Content, byte[] Origions, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
        this.Origions = Origions;
    }
}
