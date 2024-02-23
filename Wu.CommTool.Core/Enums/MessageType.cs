namespace Wu.CommTool.Core.Enums;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MessageType
{
    [Description("消息")]
    Info = 0,
    [Description("接收")]
    Receive = 1,
    [Description("发送")]
    Send = 2,
    [Description("错误")]
    Error = 3
}
