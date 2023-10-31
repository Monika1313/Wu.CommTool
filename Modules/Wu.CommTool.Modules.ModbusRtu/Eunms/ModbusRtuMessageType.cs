namespace Wu.CommTool.Modules.ModbusRtu.Enums
{
    /// <summary>
    /// ModbusRtu消息的子项类型
    /// </summary>
    public enum ModbusRtuMessageType
    {
        SlaveId = 0,        //从站ID
        Function = 1,       //功能码
        StartAddr = 2,      //起始地址
        RegisterNum = 3,    //寄存器数量
        BytesNum = 4,       //字节数
        RegisterValues = 5, //寄存器值
        CrcCode = 6,        //crc校验码
        ErrCode = 7,        //错误码
        ErrMsg = 10         //错误消息
    }
}
