namespace Wu.CommTool.Modules.ModbusRtu.Enums;

/// <summary>
/// ModbusRtu消息的子项类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum ModbusRtuMessageType
{
    [Description("从站ID")]
    SlaveId = 0,
    [Description("功能码")]
    Function = 1,
    [Description("起始地址")]
    StartAddr = 2,
    [Description("寄存器数量")]
    RegisterNum = 3,
    [Description("字节数")]
    BytesNum = 4,
    [Description("寄存器值")]
    RegisterValues = 5,
    [Description("CRC校验码")]
    CrcCode = 6,
    [Description("错误码")]
    ErrCode = 7,
    [Description("错误消息")]
    ErrMsg = 10,

    [Description("参考类型")]
    ReferenceType = 20,
    [Description("文件号")]
    FileNumber = 21,
    [Description("记录号")]
    RecordNumber = 22,
    [Description("记录长度")]
    RecordLength = 23,

    [Description("文件响应长度")]
    FileResponseLength = 30,
    [Description("记录数据")]
    RecordDatas = 31,

}
