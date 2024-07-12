namespace Wu.CommTool.Core.Models;

public class MqttMessageData : MessageData
{
    public MqttMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
    }
}
