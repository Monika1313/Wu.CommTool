using System.Collections.Generic;
using Wu.CommTool.Modules.ModbusRtu.Enums;

namespace Wu.CommTool.Modules.MrtuSlave.Models;

/// <summary>
/// Modbus子消息 将帧拆分解析的消息
/// </summary>
public partial class MessageSubContent : ObservableObject
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
    [ObservableProperty]
    string content = string.Empty;

    /// <summary>
    /// 字节值
    /// </summary>
    [ObservableProperty]
    List<byte> value;

    /// <summary>
    /// 消息类型
    /// </summary>
    [ObservableProperty]
    ModbusRtuMessageType type;
}
