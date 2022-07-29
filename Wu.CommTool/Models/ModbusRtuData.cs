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
        public dynamic OriginValue { get => _OriginValue; set => SetProperty(ref _OriginValue, value); }
        private dynamic _OriginValue;

        /// <summary>
        /// 值
        /// </summary>
        public dynamic Value { get => _Value; set => SetProperty(ref _Value, value); }
        private dynamic _Value;

        /// <summary>
        /// 数据类型
        /// </summary>
        public int Type { get => _Type; set => SetProperty(ref _Type, value); }
        private int _Type;

    }
}
