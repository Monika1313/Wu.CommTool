namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Modbus子消息 将帧拆分解析的消息
/// </summary>
public partial class MtcpSubMessageData : ObservableObject
{
    public MtcpSubMessageData(string content, MtcpMessageType type)
    {
        Content = content;
        Type = type;
    }

    public MtcpSubMessageData(IList<byte> value, string content, MtcpMessageType type)
    {
        Content = content;
        Type = type;
    }

    /// <summary>
    /// 子消息内容
    /// </summary>
    [ObservableProperty]
    string content = string.Empty;

    /// <summary>
    /// 字节值
    /// </summary>
    public IList<byte> Value { get => _Value; set => SetProperty(ref _Value, value); }
    private IList<byte> _Value;

    /// <summary>
    /// 消息类型
    /// </summary>
    [ObservableProperty]
    MtcpMessageType type;
}
