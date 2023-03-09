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



        /// <summary>
        /// 解析数据帧
        /// </summary>
        private void AnalyseFrame()
        {
            //Crc校验
            if (!IsCrcPassed)
            {
                Type = ModbusRtuFrameType.校验失败;
                ErrMessage = "Crc校验错误";
                return;
            }

            SlaveId = Frame[0];     //从站地址
            Function = Frame[1];    //功能码

            //0x03功能码
            if (Function == 0x03)
            {
                //请求帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Frame.Skip(2).Take(2).ToArray();
                    RegisterNum = Frame.Skip(4).Take(2).ToArray();
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType.请求帧0x03;
                }
                //应答帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                else if(Frame.Length>=7 && Frame.Length%2==1)
                {
                    BytesNum = Frame[2];
                    RegisterValues = Frame.Skip(3).Take(Frame.Length - 5).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType.应答帧0x03;
                }
            }
            //0x03的差错码0x83
            else if (Function == 0x83)
            {
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = "不支持0x03功能码";
                            break;
                        case 2:
                            ErrMessage = "起始地址或起始地址+寄存器数量不符合。寄存器数量范围应∈[0x0001,0x003D]";
                            break;
                        case 3:
                            ErrMessage = "寄存器数量范围应∈[0x0001,0x003D]";
                            break;
                        case 4:
                            ErrMessage = "读多个寄存器失败";
                            break;
                    }
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType.差错帧0x83;
                }
            }
            //0x10功能码
            else if (Function == 0x10)
            {
                //应答帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 校验码(2)
                if (Frame.Length.Equals(8))
                {
                    StartAddr = Frame.Skip(2).Take(2).ToArray();
                    RegisterNum = Frame.Skip(4).Take(2).ToArray();
                    CrcCode = Frame.Skip(6).Take(2).ToArray();
                    Type = ModbusRtuFrameType.应答帧0x10;
                }
                //请求帧   从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 字节数(1)  寄存器值(n) 校验码(2)
                else if(Frame.Length >=9 && Frame.Length % 2 == 1)
                {
                    StartAddr = Frame.Skip(2).Take(2).ToArray();
                    RegisterNum = Frame.Skip(4).Take(2).ToArray();
                    BytesNum = Frame[6];
                    RegisterValues = Frame.Skip(7).Take(Frame.Length - 9).ToArray();
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    Type = ModbusRtuFrameType.请求帧0x10;
                }
            }
            //0x10的错误码0x90
            else if (Function == 0x90)
            {
                if (Frame.Length.Equals(5))
                {
                    ErrCode = Frame[2];
                    CrcCode = Frame.Skip(Frame.Length - 2).Take(2).ToArray();
                    switch (ErrCode)
                    {
                        case 1:
                            ErrMessage = "不支持0x10功能码";
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
                    Type = ModbusRtuFrameType.差错帧0x90;
                }
            }
        }

        /// <summary>
        /// 从站ID 1字节
        /// </summary>
        public byte SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
        private byte _SlaveId;

        /// <summary>
        /// 功能码 1字节
        /// </summary>
        public byte Function { get => _Function; set => SetProperty(ref _Function, value); }
        private byte _Function;

        /// <summary>
        /// 起始地址
        /// </summary>
        public byte[] StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
        private byte[] _StartAddr;

        /// <summary>
        /// 寄存器数量 2字节 单位word
        /// </summary>
        public byte[] RegisterNum { get => _RegisterNum; set => SetProperty(ref _RegisterNum, value); }
        private byte[] _RegisterNum;

        /// <summary>
        /// 字节数量 1字节
        /// </summary>
        public byte BytesNum { get => _BytesNum; set => SetProperty(ref _BytesNum, value); }
        private byte _BytesNum;

        /// <summary>
        /// 寄存器值
        /// </summary>
        public byte[] RegisterValues { get => _RegisterValues; set => SetProperty(ref _RegisterValues, value); }
        private byte[] _RegisterValues;

        /// <summary>
        /// CRC校验码 2字节
        /// </summary>
        public byte[] CrcCode { get => _CrcCode; set => SetProperty(ref _CrcCode, value); }
        private byte[] _CrcCode;

        /// <summary>
        /// 错误码
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

        private string DatasFormat(byte[] input)
        {
            return BitConverter.ToString(input).Replace("-", "").InsertFormat(4, " ");
        }

        /// <summary>
        /// Crc校验结果
        /// </summary>
        public bool IsCrcPassed => IsModbusCrcPassed(Frame);

        /// <summary>
        /// 判断ModbusCrc校验是否通过
        /// </summary>
        /// <returns></returns>
        public bool IsModbusCrcPassed(byte[] frame)
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
                switch (Type)
                {
                    case ModbusRtuFrameType.校验失败:
                        return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
                    case ModbusRtuFrameType.解析失败:
                        return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
                    case ModbusRtuFrameType.请求帧0x03:
                        return $"{SlaveId:X2} {Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterNum)} {DatasFormat(CrcCode)}";
                    case ModbusRtuFrameType.应答帧0x03:
                        return $"{SlaveId:X2} {Function:X2} {BytesNum:X2} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";
                    case ModbusRtuFrameType.差错帧0x83:
                        return $"{SlaveId:X2} {Function:X2} {ErrCode:X2} {DatasFormat(CrcCode)}";
                    case ModbusRtuFrameType.请求帧0x10:
                        return $"{SlaveId:X2} {Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterNum)} {BytesNum:X2} {DatasFormat(RegisterValues)} {DatasFormat(CrcCode)}";
                    case ModbusRtuFrameType.应答帧0x10:
                        return $"{SlaveId:X2} {Function:X2} {DatasFormat(StartAddr)} {DatasFormat(RegisterNum)} {DatasFormat(CrcCode)}";
                    case ModbusRtuFrameType.差错帧0x90:
                        return $"{SlaveId:X2} {Function:X2} {ErrCode:X2} {DatasFormat(CrcCode)}";
                    default:
                        return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
                }
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
    }
}
