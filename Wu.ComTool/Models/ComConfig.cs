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

namespace Wu.ComTool.Models
{
    public class ComConfig : BindableBase
    {
        /// <summary>
        /// 波特率
        /// </summary>
        public BaudRate BaudRate { get => _BaudRate; set => SetProperty(ref _BaudRate, value); }
        private BaudRate _BaudRate;
    }
}
