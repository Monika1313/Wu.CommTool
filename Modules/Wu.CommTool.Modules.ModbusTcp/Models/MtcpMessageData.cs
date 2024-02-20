namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// 用于页面展示的消息数据
/// </summary>
public partial class MtcpMessageData : MessageData
{
    public MtcpMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
        //mtcpFrame =
    }

    [ObservableProperty]
    MtcpFrame mtcpFrame;
}
