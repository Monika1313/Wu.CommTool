namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu界面消息展示
/// </summary>
public partial class ModbusRtuMessageData : MessageData
{
    #region 构造函数

    /// <summary>
    /// modbusRtu帧
    /// </summary>
    /// <param name="Content"></param>
    /// <param name="dateTime"></param>
    /// <param name="Type"></param>
    /// <param name="frame"></param>
    public ModbusRtuMessageData(string Content, DateTime dateTime, MessageType Type, ModbusRtuFrame frame) : base(Content, dateTime, Type, "")
    {
        ModbusRtuFrame = frame;
        MessageSubContents = new ObservableCollection<MessageSubContent>(frame.GetmessageWithErrMsg());
    }
    #endregion

    /// <summary>
    /// Modbus帧
    /// </summary>
    [ObservableProperty]
    ModbusRtuFrame _ModbusRtuFrame;

    /// <summary>
    /// 子消息
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MessageSubContent> messageSubContents = [];
}
