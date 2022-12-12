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
using Wu.CommTool.Enums;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// ModbusRtu采集的数据
    /// </summary>
    public class ModbusRtuData : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }
        private string _Name = string.Empty;

        /// <summary>
        /// 数据地址
        /// </summary>
        public int Addr { get => _Addr; set => SetProperty(ref _Addr, value); }
        private int _Addr;

        /// <summary>
        /// 源数据
        /// </summary>
        public dynamic? OriginValue { get => _OriginValue; set => SetProperty(ref _OriginValue, value); }
        private dynamic? _OriginValue;

        /// <summary>
        /// 值
        /// </summary>
        public dynamic? Value { get => _Value; set => SetProperty(ref _Value, value); }
        private dynamic? _Value;

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType Type { get => _Type; set => SetProperty(ref _Type, value); }
        private DataType _Type;


        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime? UpdateTime { get => _UpdateTime; set => SetProperty(ref _UpdateTime, value); }
        private DateTime? _UpdateTime = null;

        /// <summary>
        /// 源字节数组
        /// </summary>
        public byte[]? OriginBytes { get => _OriginBytes; set => SetProperty(ref _OriginBytes, value); }
        private byte[]? _OriginBytes;

        /// <summary>
        /// 在源字节数组中的位置
        /// </summary>
        public int Location { get => _Location; set => SetProperty(ref _Location, value); }
        private int _Location = 0;

    }
}
