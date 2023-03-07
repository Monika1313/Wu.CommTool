using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wu.FzWater.Mqtt;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// ModbusRtu数据帧
    /// </summary>
    public class ModbusRtuFrame : BindableBase
    {
        /// <summary>
        /// 从站ID
        /// </summary>
        public byte SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
        private byte _SlaveId;

        /// <summary>
        /// 功能码
        /// </summary>
        public byte Function { get => _Function; set => SetProperty(ref _Function, value); }
        private byte _Function;

        /// <summary>
        /// 起始地址
        /// </summary>
        public int StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
        private int _StartAddr;

        /// <summary>
        /// 数据数量 单位word
        /// </summary>
        public int DataCount { get => _DataCount; set => SetProperty(ref _DataCount, value); }
        private int _DataCount;

        /// <summary>
        /// CRC校验码
        /// </summary>
        public byte[] CrcCode { get => _CrcCode; set => SetProperty(ref _CrcCode, value); }
        private byte[] _CrcCode;
    }
}
