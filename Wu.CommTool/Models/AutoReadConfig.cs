using Newtonsoft.Json;
using Prism.Mvvm;
using System.Text;
using Wu.CommTool.Enums;
using Wu.Extensions;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 自动读取数据
    /// </summary>
    public class AutoReadConfig : BindableBase
    {
        /// <summary>
        /// 从站地址
        /// </summary>
        public int SlaveId
        {
            get => _SlaveId;
            set
            {
                SetProperty(ref _SlaveId, value);
                RaisePropertyChanged(nameof(DataFrame));
            }
        }
        private int _SlaveId = 1;

        /// <summary>
        /// 功能码
        /// </summary>
        public int Function
        {
            get => _Function;
            set
            {
                SetProperty(ref _Function, value);
                RaisePropertyChanged(nameof(DataFrame));
            }
        }
        private int _Function = 03;

        /// <summary>
        /// 起始地址
        /// </summary>
        public int StartAddr
        {
            get => _StartAddr;
            set
            {
                SetProperty(ref _StartAddr, value);
                RaisePropertyChanged(nameof(DataFrame));
            }
        }
        private int _StartAddr = 8192;

        /// <summary>
        /// 读取数量
        /// </summary>
        public int Quantity
        {
            get => _Quantity;
            set
            {
                SetProperty(ref _Quantity, value);
                RaisePropertyChanged(nameof(DataFrame));
            }
        }
        private int _Quantity = 70;

        /// <summary>
        /// 周期
        /// </summary>
        public int Period { get => _Period; set => SetProperty(ref _Period, value); }
        private int _Period = 1000;


        /// <summary>
        /// 字节序设置
        /// </summary>
        public ModbusByteOrder ByteOrder { get => _ByteOrder; set => SetProperty(ref _ByteOrder, value); }
        private ModbusByteOrder _ByteOrder = ModbusByteOrder.DCBA;

        /// <summary>
        /// 数据帧
        /// </summary>
        public string DataFrame { get => _DataFrame; }
        private string _DataFrame => GetDataFrame();

        /// <summary>
        /// 获取数据帧
        /// </summary>
        /// <returns></returns>
        private string GetDataFrame()
        {
            string a = SlaveId.ToString("X2");
            string b = Function.ToString("X2");
            string c = StartAddr.ToString("X4");
            string d = Quantity.ToString("X4");
            string s = a + b + c + d;
            var crc = Wu.Utils.Crc.Crc16Modbus(s.GetBytes());
            return $"{a} {b} {c} {d} {crc[1]:X2}{crc[0]:X2}";
        }


        /// <summary>
        /// 是否开启
        /// </summary>
        [JsonIgnore]
        public bool IsOpened { get => _IsOpened; set => SetProperty(ref _IsOpened, value); }
        private bool _IsOpened = false;
    }
}