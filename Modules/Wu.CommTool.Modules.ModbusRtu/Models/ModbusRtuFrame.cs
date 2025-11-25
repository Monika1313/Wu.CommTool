namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu数据帧
/// </summary>
public class ModbusRtuFrame : BindableBase
{
    #region 构造函数
    public ModbusRtuFrame()
    {

    }

    public ModbusRtuFrame(byte[] frame)
    {
        Frame = frame;//缓存帧
        AnalyseFrame();
    }

    public ModbusRtuFrame(string frame) : this(frame.GetBytes())
    {
    }
    #endregion

    #region **************************************************  属性  **************************************************
    /// <summary>
    /// 从站ID 1字节
    /// </summary>
    public byte SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
    private byte _SlaveId;

    /// <summary>
    /// 功能码 1字节
    /// </summary>
    public ModbusRtuFunctionCode Function { get => _Function; set => SetProperty(ref _Function, value); }
    private ModbusRtuFunctionCode _Function;

    /// <summary>
    /// 起始地址/输出地址 2字节
    /// </summary>
    public ushort StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
    private ushort _StartAddr;

    /// <summary>
    /// 寄存器数量/线圈数量(读输出线圈)/输入数量(读离散输入) 2字节 单位word
    /// </summary>
    public ushort RegisterNum { get => _RegisterNum; set => SetProperty(ref _RegisterNum, value); }
    private ushort _RegisterNum;

    /// <summary>
    /// 字节数量 1字节
    /// </summary>
    public byte BytesNum { get => _BytesNum; set => SetProperty(ref _BytesNum, value); }
    private byte _BytesNum;

    /// <summary>
    /// 寄存器值 2×N*  / 线圈状态 N
    /// </summary>
    public byte[] RegisterValues { get => _RegisterValues; set => SetProperty(ref _RegisterValues, value); }
    private byte[] _RegisterValues;

    /// <summary>
    /// CRC校验码 2字节
    /// </summary>
    public byte[] CrcCode { get => _CrcCode; set => SetProperty(ref _CrcCode, value); }
    private byte[] _CrcCode;

    /// <summary>
    /// 错误码 1字节
    /// </summary>
    public byte ErrCode { get => _ErrCode; set => SetProperty(ref _ErrCode, value); }
    private byte _ErrCode;

    /// <summary>
    /// 帧
    /// </summary>
    public byte[] Frame { get => _Frame; set => SetProperty(ref _Frame, value); }
    private byte[] _Frame;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrMessage { get => _ErrMessage; set => SetProperty(ref _ErrMessage, value); }
    private string _ErrMessage;

    /// <summary>
    /// 帧类型
    /// </summary>
    public ModbusRtuFrameType Type { get => _Type; set => SetProperty(ref _Type, value); }
    private ModbusRtuFrameType _Type = ModbusRtuFrameType.校验失败;

    /// <summary>
    /// Crc校验结果
    /// </summary>
    public bool IsCrcPassed => ModbusRtuFrame.IsModbusCrcPassed(Frame);

    /// <summary>
    /// 子消息集合
    /// </summary>
    public ObservableCollection<MessageSubContent> MessageSubContents => new ObservableCollection<MessageSubContent>(GetMessage());

    #region 0x20功能码 请求帧属性
    /// <summary>
    /// 参考类型1
    /// </summary>
    private byte ReferenceType1 { get; set; }

    /// <summary>
    /// 文件号1
    /// </summary>
    private ushort FileNumber1 { get; set; }

    /// <summary>
    /// 记录号1
    /// </summary>
    private ushort RecordNumber1 { get; set; }

    /// <summary>
    /// 记录长度1
    /// </summary>
    private ushort RecordLength1 { get; set; }


    ///// <summary>
    ///// 参考类型2
    ///// </summary>
    //private byte ReferenceType2 { get; set; }

    ///// <summary>
    ///// 文件号2
    ///// </summary>
    //private ushort FileNumber2 { get; set; }

    ///// <summary>
    ///// 记录号2
    ///// </summary>
    //private ushort RecordNumber2 { get; set; }

    ///// <summary>
    ///// 记录长度2
    ///// </summary>
    //private ushort RecordLength2 { get; set; }
    #endregion

    #region 0x20功能码 应答帧属性
    /// <summary>
    /// 文件响应长度1
    /// </summary>
    private byte FileResponseLength1 { get; set; }

    /// <summary>
    /// 记录数据1
    /// </summary>
    public byte[] RecordDatas1 { get; set; }

    ///// <summary>
    ///// 文件响应长度2
    ///// </summary>
    //private byte FileResponseLength2 { get; set; }

    ///// <summary>
    ///// 记录数据2
    ///// </summary>
    //public byte[] RecordDatas2 { get; set; }
    #endregion
    #endregion


    #region **************************************************  方法  **************************************************
    /// <summary>
    /// 数据格式化 每间隔几个字符插入1个空格
    /// </summary>
    /// <param name="input"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    private static string DatasFormat(byte[] input, int interval = 4)
    {
        return BitConverter.ToString(input).Replace("-", "").InsertFormat(interval, " ");
    }

    /// <summary>
    /// 判断ModbusCrc校验是否通过
    /// </summary>
    /// <returns></returns>
    public static bool IsModbusCrcPassed(byte[] frame)
    {
        //对接收的消息直接进行crc校验
        var crc = Wu.Utils.Crc.Crc16Modbus(frame);   //校验码 校验通过的为0000
        //若校验结果不为0000则校验失败
        if (crc == null || !crc[0].Equals(0) || !crc[1].Equals(0))
            return false;
        //校验成功
        else
            return true;
    }

    #region 解析帧内容用于UI显示
    public List<MessageSubContent> GetMessage()
    {
        List<MessageSubContent> messages = [];
        try
        {
            switch (Type)
            {
                case ModbusRtuFrameType.校验失败:
                case ModbusRtuFrameType.解析失败:
                    messages.Add(new MessageSubContent(BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " "), ModbusRtuMessageType.ErrMsg));
                    break;

                case ModbusRtuFrameType._0x01请求帧:
                case ModbusRtuFrameType._0x02请求帧:
                case ModbusRtuFrameType._0x03请求帧:
                case ModbusRtuFrameType._0x04请求帧:
                case ModbusRtuFrameType._0x05请求帧:
                case ModbusRtuFrameType._0x05响应帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{StartAddr:X4}", ModbusRtuMessageType.StartAddr));
                    messages.Add(new MessageSubContent($"{RegisterNum:X4}", ModbusRtuMessageType.RegisterNum));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                case ModbusRtuFrameType._0x01响应帧:
                case ModbusRtuFrameType._0x02响应帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{BytesNum:X2}", ModbusRtuMessageType.BytesNum));
                    messages.Add(new MessageSubContent($"{DatasFormat(RegisterValues, 2)}", ModbusRtuMessageType.RegisterValues));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                case ModbusRtuFrameType._0x03响应帧:
                case ModbusRtuFrameType._0x04响应帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{BytesNum:X2}", ModbusRtuMessageType.BytesNum));
                    messages.Add(new MessageSubContent($"{DatasFormat(RegisterValues)}", ModbusRtuMessageType.RegisterValues));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                case ModbusRtuFrameType._0x0F请求帧:
                case ModbusRtuFrameType._0x10请求帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{StartAddr:X4}", ModbusRtuMessageType.StartAddr));
                    messages.Add(new MessageSubContent($"{RegisterNum:X4}", ModbusRtuMessageType.RegisterNum));
                    messages.Add(new MessageSubContent($"{BytesNum:X2}", ModbusRtuMessageType.BytesNum));
                    messages.Add(new MessageSubContent($"{DatasFormat(RegisterValues)}", ModbusRtuMessageType.RegisterValues));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                case ModbusRtuFrameType._0x0F响应帧:
                case ModbusRtuFrameType._0x10响应帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{StartAddr:X4}", ModbusRtuMessageType.StartAddr));
                    messages.Add(new MessageSubContent($"{RegisterNum:X4}", ModbusRtuMessageType.RegisterNum));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                case ModbusRtuFrameType._0x06请求帧:
                case ModbusRtuFrameType._0x06响应帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{StartAddr:X4}", ModbusRtuMessageType.StartAddr));
                    messages.Add(new MessageSubContent($"{DatasFormat(RegisterValues)}", ModbusRtuMessageType.RegisterValues));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                //错误帧格式相同
                case ModbusRtuFrameType._0x81错误帧:
                case ModbusRtuFrameType._0x82错误帧:
                case ModbusRtuFrameType._0x83错误帧:
                case ModbusRtuFrameType._0x84错误帧:
                case ModbusRtuFrameType._0x85错误帧:
                case ModbusRtuFrameType._0x86错误帧:
                case ModbusRtuFrameType._0x8F错误帧:
                case ModbusRtuFrameType._0x90错误帧:
                case ModbusRtuFrameType._0x94错误帧:
                case ModbusRtuFrameType._0x95错误帧:
                case ModbusRtuFrameType._0x96错误帧:
                case ModbusRtuFrameType._0x97错误帧:
                case ModbusRtuFrameType._0xAB错误帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{ErrCode:X2}", ModbusRtuMessageType.ErrCode));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                //TODO 0x14的数据类型有部分未做
                case ModbusRtuFrameType._0x14请求帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{BytesNum:X2}", ModbusRtuMessageType.BytesNum));
                    messages.Add(new MessageSubContent($"{ReferenceType1:X2}", ModbusRtuMessageType.ReferenceType));
                    messages.Add(new MessageSubContent($"{FileNumber1:X4}", ModbusRtuMessageType.FileNumber));
                    messages.Add(new MessageSubContent($"{RecordNumber1:X4}", ModbusRtuMessageType.RecordNumber));
                    messages.Add(new MessageSubContent($"{RecordLength1:X4}", ModbusRtuMessageType.RecordLength));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;
                case ModbusRtuFrameType._0x14响应帧:
                    messages.Add(new MessageSubContent($"{SlaveId:X2}", ModbusRtuMessageType.SlaveId));
                    messages.Add(new MessageSubContent($"{(byte)Function:X2}", ModbusRtuMessageType.Function));
                    messages.Add(new MessageSubContent($"{FileResponseLength1:X2}", ModbusRtuMessageType.FileResponseLength));
                    messages.Add(new MessageSubContent($"{ReferenceType1:X2}", ModbusRtuMessageType.ReferenceType));
                    messages.Add(new MessageSubContent($"{DatasFormat(RecordDatas1)}", ModbusRtuMessageType.RecordDatas));
                    messages.Add(new MessageSubContent($"{DatasFormat(CrcCode)}", ModbusRtuMessageType.CrcCode));
                    break;

                //TODO 未处理的帧
                //case ModbusRtuFrameType._0x15请求帧:
                //    break;
                //case ModbusRtuFrameType._0x15响应帧:
                //    break;
                //case ModbusRtuFrameType._0x16请求帧:
                //    break;
                //case ModbusRtuFrameType._0x16响应帧:
                //    break;
                //case ModbusRtuFrameType._0x17请求帧:
                //    break;
                //case ModbusRtuFrameType._0x17响应帧:
                //    break;
                //case ModbusRtuFrameType._0x2B请求帧:
                //    break;
                //case ModbusRtuFrameType._0x2B响应帧:
                //    break;


                default:
                    messages.Add(new MessageSubContent(BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " "), ModbusRtuMessageType.ErrMsg));
                    break;
            }
        }
        catch
        {
            messages.Add(new MessageSubContent(BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " "), ModbusRtuMessageType.ErrMsg));
        }

        return messages;
    }

    public List<MessageSubContent> GetmessageWithErrMsg()
    {
        List<MessageSubContent> messages = GetMessage();
        if (!string.IsNullOrWhiteSpace(ErrMessage))
        {
            messages.Add(new MessageSubContent($"错误: {ErrMessage}", ModbusRtuMessageType.ErrMsg));
        }
        return messages;
    } 
    #endregion


    #region 旧的方法 弃用
    /// <summary>
    /// 获取格式化的帧字符串
    /// </summary>
    /// <returns></returns>
    public string GetFormatFrame()
    {
        try
        {
            //Todo 目前仅处理帧格式, 不解析帧内容是否符合要求
            switch (Type)
            {
                case ModbusRtuFrameType.校验失败:
                case ModbusRtuFrameType.解析失败:
                    return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");

                case ModbusRtuFrameType._0x01请求帧:
                case ModbusRtuFrameType._0x02请求帧:
                case ModbusRtuFrameType._0x03请求帧:
                case ModbusRtuFrameType._0x04请求帧:
                case ModbusRtuFrameType._0x05请求帧:
                case ModbusRtuFrameType._0x05响应帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {StartAddr:X4} {RegisterNum:X4} {DatasFormat(CrcCode)}";

                case ModbusRtuFrameType._0x01响应帧:
                case ModbusRtuFrameType._0x02响应帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {BytesNum:X2} {DatasFormat(RegisterValues, 2)} {DatasFormat(CrcCode)}";

                case ModbusRtuFrameType._0x03响应帧:
                case ModbusRtuFrameType._0x04响应帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {BytesNum:X2} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";

                case ModbusRtuFrameType._0x0F请求帧:
                case ModbusRtuFrameType._0x10请求帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {StartAddr:X4} {RegisterNum:X4} {BytesNum:X2} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";

                case ModbusRtuFrameType._0x0F响应帧:
                case ModbusRtuFrameType._0x10响应帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {StartAddr:X4} {RegisterNum:X4} {DatasFormat(CrcCode)}";

                case ModbusRtuFrameType._0x81错误帧:
                case ModbusRtuFrameType._0x82错误帧:
                case ModbusRtuFrameType._0x83错误帧:
                case ModbusRtuFrameType._0x84错误帧:
                case ModbusRtuFrameType._0x85错误帧:
                case ModbusRtuFrameType._0x86错误帧:
                case ModbusRtuFrameType._0x8F错误帧:
                case ModbusRtuFrameType._0x90错误帧:
                case ModbusRtuFrameType._0x97错误帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {ErrCode:X2} {DatasFormat(CrcCode)}";

                case ModbusRtuFrameType._0x06请求帧:
                case ModbusRtuFrameType._0x06响应帧:
                    return $"{SlaveId:X2} {(byte)Function:X2} {StartAddr:X4} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";

                default:
                    return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
            }
            //return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
        }
        catch
        {
            return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
        }
    } 

    /// <summary>
    /// 格式化字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(ErrMessage))
        {
            return $"{GetFormatFrame()}    错误: {ErrMessage}";
        }
        return GetFormatFrame();
    }
    #endregion


    /// <summary>
    /// 解析数据帧
    /// </summary>
    private void AnalyseFrame()
    {
        //Crc校验
        if (!IsCrcPassed)
        {
            Type = ModbusRtuFrameType.校验失败;
            ErrMessage = "Crc校验失败...";
            return;
        }

        SlaveId = Frame[0];                             //从站地址
        Function = (ModbusRtuFunctionCode)Frame[1];     //功能码

        //Todo 仅实现了部分功能码
        //todo 根据消息内容判断消息的有效性, 
        switch (Function)
        {
            //0x01读线圈
            case ModbusRtuFunctionCode._0x01:
                //若帧总字节数为8,且第三个字节的值不为3，则大概率为响应帧, 由于无法判断此时是请求帧还是响应帧, 则全视为响应帧。
                //请求帧   从站ID(1) 功能码(1) 起始地址(2) 线圈(2) 校验码(2)
                if (Frame.Length.Equals(8) && !Frame[2].Equals(3))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    //RegisterNum = Frame.Skip(4).Take(2).ToArray();
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x01请求帧;
                }
                //响应帧   从站ID(1) 功能码(1) 字节数(1)  线圈状态(N*) 校验码(2)
                else if (Frame.Length >= 6)
                {
                    BytesNum = Frame[2];
                    RegisterValues = Frame.Skip(3).Take(Frame.Length - 5).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x01响应帧;
                }
                break;
            //0x01读线圈的错误码0x81
            case ModbusRtuFunctionCode._0x81:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或起始地址+寄存器数量不符合。寄存器数量范围应∈[0x0001,0x07D0]";
                            break;
                        case 3:
                            ErrMessage = "线圈数量范围应∈[0x0001,0x07D0]";
                            break;
                        case 4:
                            ErrMessage = "读取离散输出失败";
                            break;
                    }
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x81错误帧;
                }
                break;
            //读取离散输入
            case ModbusRtuFunctionCode._0x02:
                //若帧总字节数为8,且第三个字节的值不为3，则大概率为响应帧, 由于无法判断此时是请求帧还是响应帧, 则全视为响应帧。
                //请求帧   从站ID(1) 功能码(1) 起始地址(2) 输入数量(2) 校验码(2)
                if (Frame.Length.Equals(8) && !Frame[2].Equals(3))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x02请求帧;
                }
                //响应帧   从站ID(1) 功能码(1) 字节数(1)  线圈状态(N*) 校验码(2)
                else if (Frame.Length >= 6)
                {
                    BytesNum = Frame[2];
                    RegisterValues = Frame.Skip(3).Take(Frame.Length - 5).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x02响应帧;
                }
                break;
            case ModbusRtuFunctionCode._0x82:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或起始地址+输入数量不符合。寄存器数量范围应∈[0x0001,0x07D0]";
                            break;
                        case 3:
                            ErrMessage = "输入数量范围应∈[0x0001,0x07D0]";
                            break;
                        case 4:
                            ErrMessage = "读取离散输入失败";
                            break;
                    }
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x81错误帧;
                }
                break;
            //读保持寄存器
            case ModbusRtuFunctionCode._0x03:
                //请求帧8字节   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x03请求帧;
                }
                //响应帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                else if (Frame.Length >= 7 && Frame.Length % 2 == 1)
                {
                    BytesNum = Frame[2];
                    RegisterValues = Frame.Skip(3).Take(Frame.Length - 5).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x03响应帧;
                    if (Frame.Length != 5 + BytesNum)
                    {
                        ErrMessage = "寄存器值 数量不符...";
                    }
                }
                break;
            //0x03的错误帧 0x83
            case ModbusRtuFunctionCode._0x83:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
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
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x83错误帧;
                }
                break;
            //0x04读输入寄存器
            case ModbusRtuFunctionCode._0x04:
                //请求帧8字节   从站ID(1) 功能码(1) 起始地址(2) 输入寄存器数量(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x04请求帧;
                }
                //响应帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                else if (Frame.Length >= 7 && Frame.Length % 2 == 1)
                {
                    BytesNum = Frame[2];
                    RegisterValues = Frame.Skip(3).Take(Frame.Length - 5).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x04响应帧;
                    if (Frame.Length != 5 + BytesNum)
                    {
                        ErrMessage = "寄存器值 数量不符!!!";
                    }
                }
                break;
            case ModbusRtuFunctionCode._0x84:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
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
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x84错误帧;
                }
                break;
            //写单个线圈
            case ModbusRtuFunctionCode._0x05:
                //请求帧和应答帧格式相同 请求帧的输出值只能为0x0000(false)或0xFF00(true)
                //请求帧总8字节   从站ID(1) 功能码(1) 输出地址(2) 输出值(2) 校验码(2)
                if (Frame.Length.Equals(8) && Frame[4] == 0 && (Frame[5] == 0 || Frame[5] == 0xFF) && Frame[6] == 0)
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x05请求帧;
                }
                //响应帧   从站ID(1) 功能码(1) 输出地址(2) 输出值(2) 校验码(2)
                else if (Frame.Length.Equals(8))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x05响应帧;
                }
                break;
            case ModbusRtuFunctionCode._0x85:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "输出地址无效";
                            break;
                        case 3:
                            ErrMessage = "输出值应为0x0000或0xFF00";
                            break;
                        case 4:
                            ErrMessage = "写单个输出失败";
                            break;
                    }
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x83错误帧;
                }
                break;

            //写单个寄存器
            case ModbusRtuFunctionCode._0x06:
                //请求帧与响应帧格式完全相同
                //响应帧/请求帧 8字节   从站ID(1) 功能码(1) 寄存器地址(2) 寄存器值(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterValues = Frame.Skip(4).Take(2).ToArray();
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x06请求帧;
                }
                break;
            case ModbusRtuFunctionCode._0x86:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "寄存器地址无效";
                            break;
                        case 3:
                            ErrMessage = "寄存器地址范围应∈[0x0000,0xFFFF]";
                            break;
                        case 4:
                            ErrMessage = "写单个寄存器失败";
                            break;
                    }
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x83错误帧;
                }
                break;

            case ModbusRtuFunctionCode._0x0F:
                //响应帧   从站ID(1) 功能码(1) 起始地址(2) 输出数量(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x0F响应帧;
                }
                //请求帧   从站ID(1) 功能码(1) 起始地址(2) 输出数量(2) 字节数(1) 输出值(N*) 校验码(2)
                else if (Frame.Length >= 10)
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    BytesNum = Frame[6];
                    RegisterValues = Frame.Skip(7).Take(Frame.Length - 9).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x0F请求帧;
                }
                break;
            case ModbusRtuFunctionCode._0x8F:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或起始地址+输出数量无效";
                            break;
                        case 3:
                            ErrMessage = "输出数量范围应∈[0x0001,0x07B0]";
                            break;
                        case 4:
                            ErrMessage = "写多个输出失败";
                            break;
                    }
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x83错误帧;
                }
                break;

            case ModbusRtuFunctionCode._0x10:
                //响应帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x10响应帧;
                }
                //请求帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 字节数(1)  寄存器值(n) 校验码(2)
                else if (Frame.Length >= 9 && Frame.Length % 2 == 1)
                {
                    StartAddr = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 2);
                    RegisterNum = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    BytesNum = Frame[6];
                    RegisterValues = Frame.Skip(7).Take(Frame.Length - 9).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x10请求帧;
                }
                break;

            case ModbusRtuFunctionCode._0x90:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或寄存器数量不符合。寄存器数量范围应∈[0x0001,0x07B0]";
                            break;
                        case 3:
                            ErrMessage = "寄存器数量范围应∈[0x0001,0x07B0]";
                            break;
                        case 4:
                            ErrMessage = "写多个寄存器失败";
                            break;
                    }
                    Type = ModbusRtuFrameType._0x90错误帧;
                }
                break;

            //TODO 读文件记录功能码需要解析
            case ModbusRtuFunctionCode._0x14:
                if (Frame.Length==12)
                {
                    BytesNum = Frame[2];
                    ReferenceType1 = Frame[3];
                    FileNumber1 = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 4);
                    RecordNumber1 = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 6);
                    RecordLength1 = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(Frame, 8);
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x14请求帧;
                }
                // 0x14响应帧
                else
                {
                    
                    BytesNum = Frame[2];//响应数据长度
                    FileResponseLength1 = Frame[3];//文件响应长度 
                    ReferenceType1 = Frame[4];//参考类型
                    RecordDatas1 = Frame.Skip(5).Take(Frame.Length - 7).ToArray();//记录数据
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x14响应帧;
                }
                break;

            case ModbusRtuFunctionCode._0x94:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"该设备不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "参考类型错误/文件号错误/起始地址错误/起始地址+寄存器数量错误";
                            break;
                        case 3:
                            ErrMessage = "读的字节数应∈[0x07,0xF5]";
                            break;
                        case 4:
                            ErrMessage = "读通用参考失败";
                            break;
                        case 8:
                            ErrMessage = "08异常码";
                            break;
                    }
                    Type = ModbusRtuFrameType._0x94错误帧;
                }
                break;

            case ModbusRtuFunctionCode._0x15:
                if (Frame.Length > 8)
                {
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x15请求帧;
                }
                break;
            case ModbusRtuFunctionCode._0x95:
                if (Frame.Length > 8)
                {
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType._0x95错误帧;
                }
                break;


            case ModbusRtuFunctionCode._0x16:
                break;
            case ModbusRtuFunctionCode._0x96:
                break;

            case ModbusRtuFunctionCode._0x17:
                break;
            case ModbusRtuFunctionCode._0x97:
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = $"不支持{(Function - 0x80).ToString().TrimStart('_')}功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址+读的数量无效 或 写起始地址+写的数量 无效";
                            break;
                        case 3:
                            ErrMessage = "读的数量应∈[0x0001,0x007D] 且 写的数量∈[0x0001,0x0079] 且 字节数=写的数量×2";
                            break;
                        case 4:
                            ErrMessage = "读/写多个寄存器失败";
                            break;
                    }
                    Type = ModbusRtuFrameType._0x90错误帧;
                }
                break;

            //case ModbusRtuFunctionCode._0x2B:
            //    break;
            //case ModbusRtuFunctionCode._0xAB:
            //    break;
            default:
                break;
        }

        if (CrcCode == null)
        {
            ErrMessage = "无法解析...";
        }
    }
    #endregion
}
