using ImTools;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows.Controls;
using Wu.CommTool.Enums;
using Wu.Extensions;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// ModbusRtu数据帧
    /// </summary>
    public class ModbusRtuFrame : BindableBase
    {
        #region 构造函数
        public ModbusRtuFrame(byte[] frame)
        {
            Frame = frame;//缓存帧
            AnalyseFrame();
        }

        public ModbusRtuFrame(string frame)
        {
            Frame = frame.GetBytes();
            AnalyseFrame();
        }
        #endregion





        #region **************************************************  字段  **************************************************

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
        public byte[] StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
        private byte[] _StartAddr;

        /// <summary>
        /// 寄存器数量/线圈数量(读输出线圈)/输入数量(读离散输入) 2字节 单位word
        /// </summary>
        public byte[] RegisterNum { get => _RegisterNum; set => SetProperty(ref _RegisterNum, value); }
        private byte[] _RegisterNum;

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

        /// <summary>
        /// 获取格式化的帧字符串
        /// </summary>
        /// <returns></returns>
        public string GetFormatFrame()
        {
            //TODO 数据帧功能不同 格式化
            try
            {
                //没有解析或无法解析的不格式化
                if (Type.Equals(ModbusRtuFrameType.校验失败) || Type.Equals(ModbusRtuFrameType.解析失败))
                    return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
                //Todo 目前仅处理帧格式, 不解析帧内容是否符合要求
                switch (Type)
                {
                    case ModbusRtuFrameType.校验失败:
                        return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
                    case ModbusRtuFrameType.解析失败:
                        return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");

                    case ModbusRtuFrameType._0x01请求帧:
                    case ModbusRtuFrameType._0x02请求帧:
                    case ModbusRtuFrameType._0x03请求帧:
                    case ModbusRtuFrameType._0x04请求帧:
                    case ModbusRtuFrameType._0x05请求帧:
                    case ModbusRtuFrameType._0x05响应帧:
                        return $"{SlaveId:X2} {(byte)Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterNum)} {DatasFormat(CrcCode)}";

                    case ModbusRtuFrameType._0x01响应帧:
                    case ModbusRtuFrameType._0x02响应帧:
                        return $"{SlaveId:X2} {(byte)Function:X2} {BytesNum:X2} {DatasFormat(RegisterValues, 1)} {DatasFormat(CrcCode)}";

                    case ModbusRtuFrameType._0x03响应帧:
                    case ModbusRtuFrameType._0x04响应帧:
                        return $"{SlaveId:X2} {(byte)Function:X2} {BytesNum:X2} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";


                    case ModbusRtuFrameType._0x10请求帧:
                        return $"{SlaveId:X2} {(byte)Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterNum)} {BytesNum:X2} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";

                    case ModbusRtuFrameType._0x10响应帧:
                        return $"{SlaveId:X2} {(byte)Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterNum)} {DatasFormat(CrcCode)}";


                    case ModbusRtuFrameType._0x81错误帧:
                    case ModbusRtuFrameType._0x82错误帧:
                    case ModbusRtuFrameType._0x83错误帧:
                    case ModbusRtuFrameType._0x84错误帧:
                    case ModbusRtuFrameType._0x85错误帧:
                    case ModbusRtuFrameType._0x90错误帧:
                        return $"{SlaveId:X2} {Function:X2} {ErrCode:X2} {DatasFormat(CrcCode)}";

                    case ModbusRtuFrameType._0x06请求帧:
                    case ModbusRtuFrameType._0x06响应帧:
                        return $"{SlaveId:X2} {(byte)Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";
                    case ModbusRtuFrameType._0x86错误帧:
                        break;
                    default:
                        return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
                }
                return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
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

        /// <summary>
        /// 解析数据帧
        /// </summary>
        private void AnalyseFrame()
        {
            //Crc校验
            if (!IsCrcPassed)
            {
                Type = ModbusRtuFrameType.校验失败;
                ErrMessage = "Crc校验失败";
                return;
            }

            SlaveId = Frame[0];                             //从站地址
            Function = (ModbusRtuFunctionCode)Frame[1];     //功能码

            //Todo 仅实现了部分功能码
            switch (Function)
            {
                //0x01读线圈
                case ModbusRtuFunctionCode._0x01:
                    //若帧总字节数为8,且第三个字节的值不为3，则大概率为响应帧, 由于无法判断此时是请求帧还是响应帧, 则全视为响应帧。
                    //请求帧   从站ID(1) 功能码(1) 起始地址(2) 线圈(2) 校验码(2)
                    if (Frame.Length.Equals(8) && !Frame[2].Equals(3))
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
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
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
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
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                    //请求帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 校验码(2)
                    if (Frame.Length.Equals(8))
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
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
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                    //请求帧   从站ID(1) 功能码(1) 起始地址(2) 输入寄存器数量(2) 校验码(2)
                    if (Frame.Length.Equals(8))
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
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
                    }
                    break;
                case ModbusRtuFunctionCode._0x84:
                    if (Frame.Length.Equals(5))
                    {
                        ErrCode = Frame[2];
                        switch (ErrCode)
                        {
                            case 1:
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                    //请求帧和应答帧格式相同 请求帧的输出值只能为0x0000
                    //请求帧   从站ID(1) 功能码(1) 输出地址(2) 输出值(2) 校验码(2)
                    if (Frame.Length.Equals(8) && Frame[4] == 0 && Frame[5]==0)
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
                        CrcCode = Frame.Skip(6).Take(2).ToArray();
                        Type = ModbusRtuFrameType._0x05请求帧;
                    }
                    //响应帧   从站ID(1) 功能码(1) 输出地址(2) 输出值(2) 校验码(2)
                    else if (Frame.Length.Equals(8))
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
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
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                    //响应帧/请求帧   从站ID(1) 功能码(1) 寄存器地址(2) 寄存器值(2) 校验码(2)
                    if (Frame.Length.Equals(8))
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
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
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                    break;
                case ModbusRtuFunctionCode._0x8F:
                    break;
                case ModbusRtuFunctionCode._0x10:
                    //响应帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 校验码(2)
                    if (Frame.Length.Equals(8))
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
                        CrcCode = Frame.Skip(6).Take(2).ToArray();
                        Type = ModbusRtuFrameType._0x10响应帧;
                    }
                    //请求帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 字节数(1)  寄存器值(n) 校验码(2)
                    else if (Frame.Length >= 9 && Frame.Length % 2 == 1)
                    {
                        StartAddr = Frame.Skip(2).Take(2).ToArray();
                        RegisterNum = Frame.Skip(4).Take(2).ToArray();
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
                                ErrMessage = $"不支持{Function.ToString().TrimStart('_')}功能码";
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
                case ModbusRtuFunctionCode._0x14:
                    break;
                case ModbusRtuFunctionCode._0x94:
                    break;
                case ModbusRtuFunctionCode._0x15:
                    break;
                case ModbusRtuFunctionCode._0x95:
                    break;
                case ModbusRtuFunctionCode._0x16:
                    break;
                case ModbusRtuFunctionCode._0x96:
                    break;
                case ModbusRtuFunctionCode._0x17:
                    break;
                case ModbusRtuFunctionCode._0x97:
                    break;
                case ModbusRtuFunctionCode._0x2B:
                    break;
                case ModbusRtuFunctionCode._0xAB:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
