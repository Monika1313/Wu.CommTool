﻿namespace Wu.CommTool.Modules.ModbusRtu.ModbusRtuModels;

/// <summary>
/// ModbusRtuFrame基类
/// </summary>
public class ModbusRtuFrameCreator : BindableBase
{
    public ModbusRtuFrameCreator()
    {
        ModbusRtuFrameType = ModbusRtuFrameType._0x03请求帧;
        SlaveId = 1;
        Function = 3;
        StartAddr = 0;
        RegisterNum = 1;
    }

    #region **************************************************  属性  **************************************************
    /// <summary>
    /// 从站ID 1字节
    /// </summary>
    public byte SlaveId
    {
        get => _SlaveId;
        set
        {
            SetProperty(ref _SlaveId, value);
            SetProperty(ref _SlaveIdHex, value.ToString("X2"), nameof(SlaveIdHex));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private byte _SlaveId;

    /// <summary>
    /// 功能码 1字节
    /// </summary>
    public byte Function
    {
        get => _Function;
        set
        {
            SetProperty(ref _Function, value);
            SetProperty(ref _FunctionHex, value.ToString("X2"), nameof(FunctionHex));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private byte _Function;

    /// <summary>
    /// 起始地址/输出地址 2字节
    /// </summary>
    public ushort StartAddr
    {
        get => _StartAddr;
        set
        {
            SetProperty(ref _StartAddr, value);
            SetProperty(ref _StartAddrHex, value.ToString("X4"), nameof(StartAddrHex));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private ushort _StartAddr;

    /// <summary>
    /// 0x03寄存器数量/0x01线圈数量(读输出线圈)/0x02输入数量(读离散输入)/0x05的输出值/0x06的寄存器值 2字节 单位word
    /// </summary>
    public ushort RegisterNum
    {
        get => _RegisterNum;
        set
        {
            SetProperty(ref _RegisterNum, value);
            SetProperty(ref _RegisterNumHex, value.ToString("X4"), nameof(RegisterNumHex));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private ushort _RegisterNum;


    /// <summary>
    /// 0x0f的输入数量 0x10的寄存器数量
    /// </summary>
    public ushort WriteNum
    {
        get => _WriteNum;
        set => SetProperty(ref _WriteNum, value);
    }
    private ushort _WriteNum;


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
    public byte[] CrcCode
    {
        get => _CrcCode;
        set => SetProperty(ref _CrcCode, value);
    }
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
    /// 帧类型
    /// </summary>
    public ModbusRtuFrameType ModbusRtuFrameType
    {
        get => _ModbusRtuFrameType;
        set
        {
            SetProperty(ref _ModbusRtuFrameType, value);
            switch (_ModbusRtuFrameType)
            {
                case ModbusRtuFrameType._0x01请求帧:
                case ModbusRtuFrameType._0x01响应帧:
                    Function = 0x01;
                    break;
                case ModbusRtuFrameType._0x81错误帧:
                    Function = 0x81;
                    break;
                case ModbusRtuFrameType._0x02请求帧:
                case ModbusRtuFrameType._0x02响应帧:
                    Function = 0x02;
                    break;
                case ModbusRtuFrameType._0x82错误帧:
                    Function = 0x82;
                    break;
                case ModbusRtuFrameType._0x03请求帧:
                case ModbusRtuFrameType._0x03响应帧:
                    Function = 0x03;
                    break;
                case ModbusRtuFrameType._0x83错误帧:
                    Function = 0x83;
                    break;
                case ModbusRtuFrameType._0x04请求帧:
                case ModbusRtuFrameType._0x04响应帧:
                    Function = 0x04;
                    break;
                case ModbusRtuFrameType._0x84错误帧:
                    Function = 0x84;
                    break;
                case ModbusRtuFrameType._0x05请求帧:
                case ModbusRtuFrameType._0x05响应帧:
                    Function = 0x05;
                    break;
                case ModbusRtuFrameType._0x85错误帧:
                    Function = 0x85;
                    break;
                case ModbusRtuFrameType._0x06请求帧:
                case ModbusRtuFrameType._0x06响应帧:
                    Function = 0x06;
                    break;
                case ModbusRtuFrameType._0x86错误帧:
                    Function = 0x86;
                    break;
                case ModbusRtuFrameType._0x0F请求帧:
                case ModbusRtuFrameType._0x0F响应帧:
                    Function = 0x0F;
                    break;
                case ModbusRtuFrameType._0x8F错误帧:
                    Function = 0x8F;
                    break;
                case ModbusRtuFrameType._0x10请求帧:
                case ModbusRtuFrameType._0x10响应帧:
                    Function = 0x10;
                    break;
                case ModbusRtuFrameType._0x90错误帧:
                    Function = 0x90;
                    break;
                case ModbusRtuFrameType._0x17请求帧:
                case ModbusRtuFrameType._0x17响应帧:
                    Function = 0x17;
                    break;
                case ModbusRtuFrameType._0x97错误帧:
                    Function = 0x97;
                    break;
                default:
                    break;
            }
        }
    }
    private ModbusRtuFrameType _ModbusRtuFrameType;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrMessage { get => _ErrMessage; set => SetProperty(ref _ErrMessage, value); }
    private string _ErrMessage;
    #endregion

    #region 16进制值
    /// <summary>
    /// 从站地址16进制
    /// </summary>
    public string SlaveIdHex
    {
        get => _SlaveIdHex;
        set
        {
            SetProperty(ref _SlaveIdHex, value);
            SetProperty(ref _SlaveId, Convert.ToByte(value!.RemoveSpace(), 16), nameof(SlaveId));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private string _SlaveIdHex;

    /// <summary>
    /// 功能码16进制
    /// </summary>
    public string FunctionHex { get => _FunctionHex; set => SetProperty(ref _FunctionHex, value); }
    private string _FunctionHex;

    /// <summary>
    /// 起始地址16进制
    /// </summary>
    public string StartAddrHex
    {
        get => _StartAddrHex;
        set
        {
            SetProperty(ref _StartAddrHex, value);
            SetProperty(ref _StartAddr, Convert.ToUInt16(value!.RemoveSpace(), 16), nameof(StartAddr));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private string _StartAddrHex;

    /// <summary>
    /// 寄存器数量16进制
    /// </summary>
    public string RegisterNumHex
    {
        get => _RegisterNumHex;
        set
        {
            SetProperty(ref _RegisterNumHex, value);
            SetProperty(ref _RegisterNum, Convert.ToUInt16(value!.RemoveSpace(), 16), nameof(RegisterNum));
            RaisePropertyChanged(nameof(FrameStr));
        }
    }
    private string _RegisterNumHex;
    #endregion


    /// <summary>
    /// 帧
    /// </summary>
    public string FrameStr { get => _FrameStr; }
    private string _FrameStr => GetFrameStr();


    /// <summary>
    /// 获取帧
    /// </summary>
    /// <returns></returns>
    public string GetFrameStr()
    {
        string unCrcFrame = string.Empty;
        switch (ModbusRtuFrameType)
        {
            case ModbusRtuFrameType._0x01请求帧:
            case ModbusRtuFrameType._0x02请求帧:
            case ModbusRtuFrameType._0x03请求帧:
            case ModbusRtuFrameType._0x04请求帧:
            case ModbusRtuFrameType._0x05请求帧:
            case ModbusRtuFrameType._0x06请求帧:
                //TODO 0x05请求帧的输出值只能是0xFF00或0x0000
                unCrcFrame = $"{SlaveId:X2} {Function:X2} {StartAddr:X4} {RegisterNum:X4} ";
                CrcCode = Wu.Utils.Crc.Crc16Modbus(unCrcFrame.GetBytes());
                return Wu.CommTool.Core.Common.ModbusUtils.StrCombineCrcCode(unCrcFrame);


            case ModbusRtuFrameType._0x0F请求帧:
            case ModbusRtuFrameType._0x10请求帧:
                //unCrcFrame = $"{SlaveId:X2} {Function:X2} {StartAddr:X4} {RegisterNum:X4} ";
                //CrcCode = Wu.Utils.Crc.Crc16Modbus(unCrcFrame.GetBytes());
                //return Wu.CommTool.Modules.ModbusRtu.Utils.ModbusUtils.StrCombineCrcCode(unCrcFrame);
                break;


            case ModbusRtuFrameType._0x81错误帧:
            case ModbusRtuFrameType._0x82错误帧:
            case ModbusRtuFrameType._0x83错误帧:
            case ModbusRtuFrameType._0x84错误帧:
            case ModbusRtuFrameType._0x85错误帧:
            case ModbusRtuFrameType._0x86错误帧:
            case ModbusRtuFrameType._0x8F错误帧:
            case ModbusRtuFrameType._0x90错误帧:
            case ModbusRtuFrameType._0x97错误帧:
                break;


            case ModbusRtuFrameType._0x01响应帧:
                break;

            case ModbusRtuFrameType._0x02响应帧:
                break;

            case ModbusRtuFrameType._0x03响应帧:
            case ModbusRtuFrameType._0x04响应帧:
                break;


            case ModbusRtuFrameType._0x05响应帧:
            case ModbusRtuFrameType._0x06响应帧:
                break;


            case ModbusRtuFrameType._0x0F响应帧:
                break;

            case ModbusRtuFrameType._0x10响应帧:
                break;

            case ModbusRtuFrameType._0x17请求帧:
                break;
            case ModbusRtuFrameType._0x17响应帧:
                break;
        }
        return string.Empty;
    }
}
