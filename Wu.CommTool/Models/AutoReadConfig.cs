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
        /// 从站地址
        /// </summary>
        public int SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
        private int _SlaveId =1;

        /// <summary>
        /// definity
        /// </summary>
        public int Function { get => _Function; set => SetProperty(ref _Function, value); }
        private int _Function = 03;


    }
}
