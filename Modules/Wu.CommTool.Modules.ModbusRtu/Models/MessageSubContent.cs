namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// Modbus子消息 将帧拆分解析的消息
/// </summary>
public class MessageSubContent : BindableBase
{
    public MessageSubContent(string content, ModbusRtuMessageType type)
    {
        Content = content;
        Type = type;
    }

    public MessageSubContent(IList<byte> value, string content, ModbusRtuMessageType type)
    {
        Content = content;
        Type = type;
    }

    /// <summary>
    /// 子消息内容
    /// </summary>
    public string Content { get => _Content; set => SetProperty(ref _Content, value); }
    private string _Content = string.Empty;

    /// <summary>
    /// 字节值
    /// </summary>
    public IList<byte> Value { get => _Value; set => SetProperty(ref _Value, value); }
    private IList<byte> _Value;

    /// <summary>
    /// 消息类型
    /// </summary>
    public ModbusRtuMessageType Type { get => _Type; set => SetProperty(ref _Type, value); }
    private ModbusRtuMessageType _Type;
}
