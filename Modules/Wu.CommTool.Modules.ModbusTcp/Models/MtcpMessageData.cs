namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// 用于页面展示的消息数据
/// </summary>
public partial class MtcpMessageData : MessageData
{
    #region 构造函数
    public MtcpMessageData(string Content, DateTime dateTime, MessageType Type, MtcpFrame frame) : base(Content, dateTime, Type, "")
    {
        mtcpFrame = frame;
    }
    #endregion

    [ObservableProperty]
    MtcpFrame mtcpFrame;
}
