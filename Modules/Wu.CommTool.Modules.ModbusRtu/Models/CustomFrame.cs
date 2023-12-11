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

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    public class CustomFrame : BindableBase
    {
        public CustomFrame(string frame = "")
        {
            Frame = frame;
        }
        /// <summary>
        /// 帧
        /// </summary>
        public string Frame { get => _Frame; set => SetProperty(ref _Frame, value); }
        private string _Frame = "";
    }
}
