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
    /// 编码模式
    /// </summary>
    public class EncodeMode : BindableBase
    {
        public EncodeMode()
        {

        }


        /// <summary>
        /// Id
        /// </summary>
        public int Id { get => _Id; set => SetProperty(ref _Id, value); }
        private int _Id;


    }
}
