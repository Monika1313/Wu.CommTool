namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// 用于页面展示的消息数据
/// </summary>
public partial class MtcpMessageData : MessageData
{
    #region 构造函数
    public MtcpMessageData(string Content, DateTime dateTime, MessageType Type, MtcpFrame frame) : base(Content, dateTime, Type, "")
    {
        MtcpFrame = frame;
        MtcpSubMessageData = new ObservableCollection<MtcpSubMessageData>(frame.GetMessageWithErrMsg());
    }
    #endregion

    [ObservableProperty] MtcpFrame mtcpFrame;

    /// <summary>
    /// 子消息
    /// </summary>
    [ObservableProperty] ObservableCollection<MtcpSubMessageData> mtcpSubMessageData = [];
}
