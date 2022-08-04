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
using Wu.Extensions;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 自动读取数据
    /// </summary>
    public class AutoReadConfig : BindableBase
    {
        /// <summary>
        /// </summary>
        private int _SlaveId = 1;

        /// <summary>
        /// 功能码
        /// </summary>
        private int _Function = 03;

        /// <summary>
        /// 起始地址
        /// </summary>
        private int _StartAddr = 0;

        /// <summary>
        /// </summary>
        private int _Quantity = 120;

        /// <summary>
        /// 周期
        /// </summary>
        public int Period { get => _Period; set => SetProperty(ref _Period, value); }
        private int _Period = 1000;

        /// <summary>
        /// </summary>

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsOpened { get => _IsOpened; set => SetProperty(ref _IsOpened, value); }
        private bool _IsOpened = false;
    }
}
