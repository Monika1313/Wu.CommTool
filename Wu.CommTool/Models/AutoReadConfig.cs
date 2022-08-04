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
    public class AutoReadConfig : BindableBase
    {
        /// <summary>
        /// 从站ID
        /// </summary>
        public int SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
        private int _SlaveId = 1;

        /// <summary>
        /// 功能码
        /// </summary>
        public int Function { get => _Function; set => SetProperty(ref _Function, value); }
        private int _Function = 03;

        /// <summary>
        /// 起始地址
        /// </summary>
        public int StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
        private int _StartAddr = 0;

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get => _Quantity; set => SetProperty(ref _Quantity, value); }
        private int _Quantity = 120;

        /// <summary>
        /// 周期
        /// </summary>
        public int Period { get => _Period; set => SetProperty(ref _Period, value); }
        private int _Period = 1000;

        /// <summary>
        /// 数据
        /// </summary>
        public ObservableCollection<ModbusRtuData> ModbusRtuDatas { get => _ModbusRtuDatas; set => SetProperty(ref _ModbusRtuDatas, value); }
        private ObservableCollection<ModbusRtuData> _ModbusRtuDatas = new();

    }
}
