using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Wu.CommTool.Modules.ModbusRtu.Models;
using Wu.CommTool.Modules.ModbusRtu.Enums;
using System.Threading.Tasks;

namespace Wu.CommTool.Modules.ModbusRtu.ModbusRtuModels
{
    /// <summary>
    /// ModbusRtuFrame基类
    /// </summary>
    public class ModbusRtuFrameCreator : BindableBase
    {
        public ModbusRtuFrameCreator()
        {
            ModbusRtuFrameType = ModbusRtuFrameType._0x03请求帧;
        }
        #region **************************************************  属性  **************************************************
        /// <summary>
        /// 从站ID 1字节
        /// </summary>
        public byte SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
        private byte _SlaveId = 1;

        /// <summary>
        /// 功能码 1字节
        /// </summary>
        public ModbusRtuFunctionCode Function { get => _Function; set => SetProperty(ref _Function, value); }
        private ModbusRtuFunctionCode _Function = ModbusRtuFunctionCode._0x03;

        /// <summary>
        /// 起始地址/输出地址 2字节
        /// </summary>
        public short StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
        private short _StartAddr = 0;

        /// <summary>
        /// 寄存器数量/线圈数量(读输出线圈)/输入数量(读离散输入) 2字节 单位word
        /// </summary>
        public short RegisterNum { get => _RegisterNum; set => SetProperty(ref _RegisterNum, value); }
        private short _RegisterNum = 1;


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
        /// 帧类型
        /// </summary>
        public ModbusRtuFrameType ModbusRtuFrameType { get => _ModbusRtuFrameType; set => SetProperty(ref _ModbusRtuFrameType, value); }
        private ModbusRtuFrameType _ModbusRtuFrameType;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrMessage { get => _ErrMessage; set => SetProperty(ref _ErrMessage, value); }
        private string _ErrMessage;

        /// <summary>
        /// Crc校验结果
        /// </summary> 
        public bool IsCrcPassed => ModbusRtuFrame.IsModbusCrcPassed(Frame);
        #endregion



        /// <summary>
        /// 获取帧
        /// </summary>
        /// <returns></returns>
        public byte[] GetFrame()
        {
            switch (ModbusRtuFrameType)
            {
                case ModbusRtuFrameType._0x01请求帧:
                    break;
                case ModbusRtuFrameType._0x01响应帧:
                    break;
                case ModbusRtuFrameType._0x81错误帧:
                    break;
                case ModbusRtuFrameType._0x02请求帧:
                    break;
                case ModbusRtuFrameType._0x02响应帧:
                    break;
                case ModbusRtuFrameType._0x82错误帧:
                    break;
                case ModbusRtuFrameType._0x03请求帧:
                    break;
                case ModbusRtuFrameType._0x03响应帧:
                    break;
                case ModbusRtuFrameType._0x83错误帧:
                    break;
                case ModbusRtuFrameType._0x04请求帧:
                    break;
                case ModbusRtuFrameType._0x04响应帧:
                    break;
                case ModbusRtuFrameType._0x84错误帧:
                    break;
                case ModbusRtuFrameType._0x05请求帧:
                    break;
                case ModbusRtuFrameType._0x05响应帧:
                    break;
                case ModbusRtuFrameType._0x85错误帧:
                    break;
                case ModbusRtuFrameType._0x06请求帧:
                    break;
                case ModbusRtuFrameType._0x06响应帧:
                    break;
                case ModbusRtuFrameType._0x86错误帧:
                    break;
                case ModbusRtuFrameType._0x0F请求帧:
                    break;
                case ModbusRtuFrameType._0x0F响应帧:
                    break;
                case ModbusRtuFrameType._0x8F错误帧:
                    break;
                case ModbusRtuFrameType._0x10请求帧:
                    break;
                case ModbusRtuFrameType._0x10响应帧:
                    break;
                case ModbusRtuFrameType._0x90错误帧:
                    break;
                case ModbusRtuFrameType._0x17请求帧:
                    break;
                case ModbusRtuFrameType._0x17响应帧:
                    break;
                case ModbusRtuFrameType._0x97错误帧:
                    break;
                default:
                    break;
            }
            //TODO 
            return new byte[0];
        }
    }
}
