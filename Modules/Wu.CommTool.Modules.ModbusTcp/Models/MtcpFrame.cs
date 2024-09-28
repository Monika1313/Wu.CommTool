namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// ModbusTCP帧
/// </summary>
public partial class MtcpFrame : ObservableObject
{
    #region ctor
    public MtcpFrame(byte[] frame)
    {
        ByteFrame = frame;
        Time = DateTime.Now;
        AnalyzeFrame();
    }

    public MtcpFrame(string frame) : this(frame.GetBytes()) { }
    #endregion

    /// <summary>
    /// 解析帧内容
    /// </summary>
    private void AnalyzeFrame()
    {
        if (ByteFrame.Length < 8)
        {
            MtcpFrameType = MtcpFrameType.解析失败;
        }

        MBAP = ByteFrame.Take(7).ToArray();
        PDU = ByteFrame.Skip(7).ToArray();

        //MBAP部分
        TransactionId = ConvertUtil.GetUInt16FromBigEndianBytes(MBAP, 0);
        ProtocolId = ConvertUtil.GetUInt16FromBigEndianBytes(MBAP, 2);
        PduLength = ConvertUtil.GetUInt16FromBigEndianBytes(MBAP, 4);
        UnitId = MBAP[6];
        //TransactionId = BitConverter.ToUInt16(ByteFrame.Skip(0).Take(2).Reverse().ToArray(), 0);
        //ProtocolId = BitConverter.ToUInt16(ByteFrame.Skip(2).Take(2).Reverse().ToArray(), 0);
        //PduLength = BitConverter.ToUInt16(ByteFrame.Skip(4).Take(2).Reverse().ToArray(), 0);
        //UnitId = ByteFrame[6];

        //PDU部分
        FunctionCode = (MtcpFunctionCode)PDU[0];

        //TODO 解析帧类型
        switch (FunctionCode)
        {
            case MtcpFunctionCode._0x01:
                break;
            case MtcpFunctionCode._0x81:
                break;
            case MtcpFunctionCode._0x02:
                break;
            case MtcpFunctionCode._0x82:
                break;
            case MtcpFunctionCode._0x03:
                //请求帧
                if (PDU.Length.Equals(5))
                {
                    MtcpFrameType = MtcpFrameType._0x03请求帧;
                    StartAddr = ConvertUtil.GetUInt16FromBigEndianBytes(PDU, 1);
                    RegisterNum = ConvertUtil.GetUInt16FromBigEndianBytes(PDU, 3);
                }
                //响应帧  功能码(1) 字节数(1)  寄存器值(N *×2) 校验码(2)
                else if (PDU.Length >= 4 && PDU.Length % 2 == 0)
                {
                    MtcpFrameType = MtcpFrameType._0x03响应帧;
                    BytesNum = PDU[1];
                    RegisterValues = PDU.Skip(2).ToArray();

                    if (PDU.Length != 2 + BytesNum)
                    {
                        ErrMessage = "寄存器值 数量不符...";
                    }
                }
                break;
            case MtcpFunctionCode._0x83:
                if (PDU.Length.Equals(2))
                {
                    MtcpFrameType = MtcpFrameType._0x83错误帧;
                    ErrorCode = PDU[1];
                    switch (ErrorCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(FunctionCode - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或起始地址+寄存器数量不符合。寄存器数量范围应∈[0x0001,0x007D]";
                            break;
                        case 3:
                            ErrMessage = "寄存器数量范围应∈[0x0001,0x007D]";
                            break;
                        case 4:
                            ErrMessage = "读多个寄存器失败";
                            break;
                    }
                }
                break;
            case MtcpFunctionCode._0x04:
                if (ByteFrame.Length.Equals(7 + 5))
                {
                    MtcpFrameType = MtcpFrameType._0x04请求帧;
                    StartAddr = BitConverter.ToUInt16(ByteFrame.Skip(8).Take(2).Reverse().ToArray(), 0);
                    RegisterNum = BitConverter.ToUInt16(ByteFrame.Skip(10).Take(2).Reverse().ToArray(), 0);
                }
                break;
            case MtcpFunctionCode._0x84:
                if (PDU.Length.Equals(2))
                {
                    MtcpFrameType = MtcpFrameType._0x84错误帧;
                    ErrorCode = PDU[1];
                    switch (ErrorCode)
                    {
                        case 1:
                            ErrMessage = $"该设备不支持{(FunctionCode - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或起始地址+寄存器数量不符合。寄存器数量范围应∈[0x0001,0x007D]";
                            break;
                        case 3:
                            ErrMessage = "寄存器数量范围应∈[0x0001,0x007D]";
                            break;
                        case 4:
                            ErrMessage = "读输入寄存器失败";
                            break;
                    }
                }
                break;
            case MtcpFunctionCode._0x05:
                break;
            case MtcpFunctionCode._0x85:
                break;
            case MtcpFunctionCode._0x06:
                break;
            case MtcpFunctionCode._0x86:
                break;
            case MtcpFunctionCode._0x0F:
                break;
            case MtcpFunctionCode._0x8F:
                break;
            case MtcpFunctionCode._0x10:
                break;
            case MtcpFunctionCode._0x90:
                break;
            case MtcpFunctionCode._0x14:
                break;
            case MtcpFunctionCode._0x94:
                break;
            case MtcpFunctionCode._0x15:
                break;
            case MtcpFunctionCode._0x95:
                break;
            case MtcpFunctionCode._0x16:
                break;
            case MtcpFunctionCode._0x96:
                break;
            case MtcpFunctionCode._0x17:
                break;
            case MtcpFunctionCode._0x97:
                break;
            case MtcpFunctionCode._0x2B:
                break;
            case MtcpFunctionCode._0xAB:
                break;
            default:
                break;
        }

        MtcpSubMessageDatas = new ObservableCollection<MtcpSubMessageData>(GetMessageWithErrMsg());
    }

    /// <summary>
    /// 获取用于UI绑定的子消息
    /// </summary>
    /// <returns></returns>
    public List<MtcpSubMessageData> GetMessage()
    {
        List<MtcpSubMessageData> messages = [];
        if (MtcpFrameType == MtcpFrameType.解析失败)
        {
            messages.Add(new MtcpSubMessageData(BitConverter.ToString(ByteFrame).Replace("-", "").InsertFormat(4, " "), MtcpMessageType.ErrorMessage));
            return messages;
        }


        //MBAP
        messages.Add(new MtcpSubMessageData($"{TransactionId:X4}", MtcpMessageType.TransactionId));
        messages.Add(new MtcpSubMessageData($"{ProtocolId:X4}", MtcpMessageType.ProtocolId));
        messages.Add(new MtcpSubMessageData($"{PduLength:X4}", MtcpMessageType.PduLength));
        messages.Add(new MtcpSubMessageData($"{UnitId:X2}", MtcpMessageType.UnitId));
        messages.Add(new MtcpSubMessageData($"{(byte)FunctionCode:X2}", MtcpMessageType.FunctionCode));

        switch (MtcpFrameType)
        {

            case MtcpFrameType._0x03请求帧:
                messages.Add(new MtcpSubMessageData($"{StartAddr:X4}", MtcpMessageType.StartAddr));
                messages.Add(new MtcpSubMessageData($"{RegisterNum:X4}", MtcpMessageType.RegisterNum));
                break;
            case MtcpFrameType._0x03响应帧:
                messages.Add(new MtcpSubMessageData($"{BytesNum:X2}", MtcpMessageType.BytesNum));
                messages.Add(new MtcpSubMessageData($"{RegisterValues.DataFormat(2)}", MtcpMessageType.RegisterValues));

                break;

            case MtcpFrameType._0x04请求帧:
                messages.Add(new MtcpSubMessageData($"{StartAddr:X4}", MtcpMessageType.StartAddr));
                messages.Add(new MtcpSubMessageData($"{RegisterNum:X4}", MtcpMessageType.RegisterNum));
                break;


            case MtcpFrameType._0x81错误帧:
            case MtcpFrameType._0x82错误帧:
            case MtcpFrameType._0x83错误帧:
            case MtcpFrameType._0x84错误帧:
            case MtcpFrameType._0x85错误帧:
            case MtcpFrameType._0x86错误帧:
            case MtcpFrameType._0x8F错误帧:
            case MtcpFrameType._0x90错误帧:
            case MtcpFrameType._0x97错误帧:
                messages.Add(new MtcpSubMessageData($"{ErrorCode:X2}", MtcpMessageType.ErrorCode));
                break;
            default:
                messages.Add(new MtcpSubMessageData(BitConverter.ToString(ByteFrame).Replace("-", "").InsertFormat(4, " "), MtcpMessageType.ErrorMessage));
                break;
                //case MtcpFrameType._0x01请求帧:
                //    break;
                //case MtcpFrameType._0x01响应帧:
                //    break;
                //case MtcpFrameType._0x02请求帧:
                //    break;
                //case MtcpFrameType._0x02响应帧:
                //    break;
                //case MtcpFrameType._0x04响应帧:
                //    break;
                //case MtcpFrameType._0x05请求帧:
                //    break;
                //case MtcpFrameType._0x05响应帧:
                //    break;
                //case MtcpFrameType._0x06请求帧:
                //    break;
                //case MtcpFrameType._0x06响应帧:
                //    break;
                //case MtcpFrameType._0x0F请求帧:
                //    break;
                //case MtcpFrameType._0x0F响应帧:
                //    break;
                //case MtcpFrameType._0x10请求帧:
                //    break;
                //case MtcpFrameType._0x10响应帧:
                //    break;
                //case MtcpFrameType._0x17请求帧:
                //    break;
                //case MtcpFrameType._0x17响应帧:
                //    break;
        }


        //messages.Add(new MtcpSubMessageData($"{DatasFormat(RegisterNum)}", ModbusRtuMessageType.RegisterNum));

        return messages;
    }

    public List<MtcpSubMessageData> GetMessageWithErrMsg()
    {
        List<MtcpSubMessageData> messages = GetMessage();
        if (!string.IsNullOrWhiteSpace(ErrMessage))
        {
            messages.Add(new MtcpSubMessageData($"错误: {ErrMessage}", MtcpMessageType.ErrorMessage));
        }
        return messages;
    }




    #region 属性
    /// <summary>
    /// 帧类型
    /// </summary>
    [ObservableProperty]
    MtcpFrameType mtcpFrameType = MtcpFrameType.解析失败;

    /// <summary>
    /// 帧 字节数组 
    /// </summary>
    [ObservableProperty]
    byte[] byteFrame;

    /// <summary>
    /// MBAP
    /// </summary>
    [ObservableProperty]
    byte[] mBAP;

    /// <summary>
    /// PDU
    /// </summary>
    [ObservableProperty]
    byte[] pDU;

    /// <summary>
    /// 错误消息
    /// </summary>
    [ObservableProperty]
    string errMessage;

    /// <summary>
    /// 子消息
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MtcpSubMessageData> mtcpSubMessageDatas = [];
    #endregion

    #region MBAP（报文头）：MBAP的长度为7个字节
    /// <summary>
    /// Transaction Identifier（事务标识符）：表示Modbus TCP事务的唯一标识符，用于将请求和响应绑定在一起。当Modbus TCP服务器返回响应时，该标识符必须与请求匹配
    /// </summary>
    [ObservableProperty]
    ushort transactionId;

    /// <summary>
    /// Protocol Identifier（协议标识符）：表示Modbus TCP协议的标识符，其值固定为0x0000。
    /// </summary>
    [ObservableProperty]
    ushort protocolId = 0x0000;

    /// <summary>
    /// Length（长度）：表示数据段的长度，以字节为单位。
    /// </summary>
    [ObservableProperty]
    ushort pduLength;

    /// <summary>
    /// Unit Identifier（单元标识符）(从站ID)：标识实际执行数据交换的设备或软件。
    /// </summary>
    [ObservableProperty]
    byte unitId;
    #endregion

    #region PDU（协议数据单元）：PDU由功能码和数据组成
    /// <summary>
    /// Function Code（功能码）：表示Modbus TCP的功能码，用于指定读/写操作或访问特定设备或软件的其它功能。功能码的长度为1字节。
    /// </summary>
    [ObservableProperty]
    MtcpFunctionCode functionCode;

    /// <summary>
    /// 起始地址
    /// </summary>
    [ObservableProperty]
    ushort startAddr;

    /// <summary>
    /// 寄存器数量
    /// </summary>
    [ObservableProperty]
    ushort registerNum;

    [ObservableProperty]
    byte bytesNum;

    byte[] RegisterValues;

    [ObservableProperty]
    byte errorCode;
    #endregion

    /// <summary>
    /// 发送或接收的时间
    /// </summary>
    public DateTime Time { get; set; }


    public override string ToString()
    {
        return BitConverter.ToString(ByteFrame).Replace("-", "");
    }
}
