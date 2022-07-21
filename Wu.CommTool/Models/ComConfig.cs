using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.Comm.Models
{
    /// <summary>
    /// 串口配置
    /// </summary>
    public class ComConfig : BindableBase
    {
        /// <summary>
        /// Com口
        /// </summary>
        public KeyValuePair<string, string> Port { get => _Port; set => SetProperty(ref _Port, value); }
        private KeyValuePair<string, string> _Port;

        /// <summary>
        /// 波特率
        /// </summary>
        public BaudRate BaudRate { get => _BaudRate; set => SetProperty(ref _BaudRate, value); }
        private BaudRate _BaudRate = BaudRate._9600;

        /// <summary>
        /// 校验
        /// </summary>
        public Parity Parity { get => _Parity; set => SetProperty(ref _Parity, value); }
        private Parity _Parity = Parity.None;


        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits { get => _DataBits; set => SetProperty(ref _DataBits, value); }
        private int _DataBits =8;

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get => _StopBits; set => SetProperty(ref _StopBits, value); }
        private StopBits _StopBits = StopBits.One;

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpened { get => _IsOpened; set => SetProperty(ref _IsOpened, value); }
        private bool _IsOpened =false;
    }
}
