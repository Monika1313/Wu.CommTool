using Prism.Mvvm;
using Wu.CommTool.Shared.Enums;

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    /// <summary>
    /// ModbusRtu设备
    /// </summary>
    public class ModbusRtuDevice : BindableBase
    {
        /// <summary>
        /// 从站地址
        /// </summary>
        public int Address { get => _Address; set => SetProperty(ref _Address, value); }
        private int _Address;

        /// <summary>
        /// 波特率
        /// </summary>
        public BaudRate BaudRate { get => _BaudRate; set => SetProperty(ref _BaudRate, value); }
        private BaudRate _BaudRate = BaudRate._9600;

        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Parity { get => _Parity; set => SetProperty(ref _Parity, value); }
        private Parity _Parity = Parity.None;

        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits { get => _DataBits; set => SetProperty(ref _DataBits, value); }
        private int _DataBits = 8;

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get => _StopBits; set => SetProperty(ref _StopBits, value); }
        private StopBits _StopBits = StopBits.One;

        /// <summary>
        /// 接收的消息
        /// </summary>
        public string ReceiveMessage { get => _ReceiveMessage; set => SetProperty(ref _ReceiveMessage, value); }
        private string _ReceiveMessage = string.Empty;
    }
}
