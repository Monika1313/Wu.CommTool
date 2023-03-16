namespace Wu.CommTool.Enums
{
    /// <summary>
    /// ModbusRtu消息的子项类型
    /// </summary>
    public enum ModbusRtuMessageType
    {
        SlaveId = 0,
        Function = 1,
        StartAddr = 2,
        RegisterNum = 3,
        BytesNum = 4,
        RegisterValues = 5,
        CrcCode = 6,
        ErrCode = 7,
        ErrMsg = 10
    }
}
