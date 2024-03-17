namespace Wu.CommTool.Modules.ModbusTcp.Enums;

/// <summary>
/// ModbusTcp消息的子项类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MtcpMessageType
{
    #region MBAP部分
    [Description("事务处理标识")]
    TransactionId,//2字节

    [Description(description: "协议标识")]
    ProtocolId,//2字节 0=Modbus协议

    [Description(description: "长度")]
    PduLength,//2字节 标识PDU长度

    [Description(description: "单元标识")]
    UnitId,//1字节 用于标识Modbus从设备或服务器的地址
    #endregion

    #region PDU部分 不是所有帧都含以下内容
    [Description("功能码")]
    FunctionCode,
    [Description("起始地址")]
    StartAddr,
    [Description("寄存器数量")]
    RegisterNum,
    [Description("字节数")]
    BytesNum,
    [Description("寄存器值")]
    RegisterValues,
    #endregion

    #region 其他
    [Description("错误码")]
    ErrCode,
    [Description("错误消息")]
    ErrMsg 
    #endregion
}
