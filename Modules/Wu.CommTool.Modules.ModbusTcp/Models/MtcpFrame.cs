namespace Wu.CommTool.Modules.ModbusTcp.Models;

public partial class MtcpFrame : ObservableObject
{
    /// <summary>
    /// 功能码
    /// </summary>
    [ObservableProperty]
    byte functionCode;

    /// <summary>
    /// 从站地址
    /// </summary>
    [ObservableProperty]
    byte slaveAddress;


    byte[] MessageFrame { get; }

    /// <summary>
    /// PDU
    /// </summary>
    [ObservableProperty]
    byte[] protocolDataUnit;

    [ObservableProperty]
    ushort transactionId;

}
