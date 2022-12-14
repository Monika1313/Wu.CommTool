using HandyControl.Controls;
using Newtonsoft.Json;
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
using Wu.CommTool.Enums;

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
        /// 源数据Hex
        /// </summary>
        [JsonIgnore]
        public dynamic? OriginValue { get => _OriginValue; set => SetProperty(ref _OriginValue, value); }
        private dynamic? _OriginValue;

        /// <summary>
        /// 值
        /// </summary>
        public dynamic? Value { get => _Value; set => SetProperty(ref _Value, value); }
        private dynamic? _Value;


        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType Type { get => _Type; set => SetProperty(ref _Type, value); }
        private DataType _Type;

        /// <summary>
        /// 数据更新时间
        /// </summary>
        [JsonIgnore]
        public DateTime? UpdateTime { get => _UpdateTime; set => SetProperty(ref _UpdateTime, value); }
        private DateTime? _UpdateTime = null;


        /// <summary>
        /// 倍率
        /// </summary>
        public double Rate { get => _Rate; set => SetProperty(ref _Rate, value); }
        private double _Rate = 1;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get => _Unit; set => SetProperty(ref _Unit, value); }
        private string _Unit = "";

        /// <summary>
        /// 源字节数组
        /// </summary>
        [JsonIgnore]
        public byte[]? OriginBytes
        {
            get => _OriginBytes; 
            set
            {
                SetProperty(ref _OriginBytes, value);
                Value = GetVal(DataBytes, Type, Rate);
            }
        }
        private byte[]? _OriginBytes;

        /// <summary>
        /// 在源字节数组中的位置
        /// </summary>
        public int Location { get => _Location; set => SetProperty(ref _Location, value); }
        private int _Location = 0;

        //当前值的原始字节数组(已转换字节序)
        public byte[]? DataBytes => GetDataBytes(OriginBytes, Location, ModbusByteOrder, Type);

        //高位在前 低位在后
        public string DataHex => DataBytes is null ? "00" : BitConverter.ToString(DataBytes.Reverse().ToArray()).Replace('-', ' ');

        public override string ToString() => DataHex;

        /// <summary>
        /// 字节序
        /// </summary>
        public ModbusByteOrder ModbusByteOrder { get => _ModbusByteOrder; set => SetProperty(ref _ModbusByteOrder, value); }
        private ModbusByteOrder _ModbusByteOrder;


        /// <summary>
        /// 截取原始字节数组
        /// </summary>
        /// <param name="origin">字节数组</param>
        /// <param name="skip">起始位置</param>
        /// <param name="byteOrder">字节序</param>
        /// <param name="dataType">数据类型</param>
        /// <returns></returns>
        public static dynamic? GetDataBytes(byte[]? origin, int skip, ModbusByteOrder byteOrder, DataType dataType)
        {
            if (origin == null)
            {
                return null;
            }
            //从源数组中读取数据
            byte[] arr = origin.Skip(skip).Take(DataTypeLength(dataType)).ToArray();
            //进行字节序转换
            arr = ByteOrder(arr, byteOrder);
            return arr;
        }


        public static dynamic? GetVal(byte[]? databytes, DataType dataType, double rate)
        {
            if (databytes == null)
            {
                return 0;
            }
            switch (dataType)
            {
                case DataType.uShort:
                    return BitConverter.ToUInt16(databytes) * rate;
                case DataType.Short:
                    return BitConverter.ToInt16(databytes) * rate;
                case DataType.uInt:
                    return BitConverter.ToUInt32(databytes) * rate;
                case DataType.Int:
                    return BitConverter.ToInt32(databytes) * rate;
                case DataType.uLong:
                    return BitConverter.ToUInt64(databytes) * rate;
                case DataType.Long:
                    return BitConverter.ToInt64(databytes) * rate;
                case DataType.Float:
                    return Math.Round((BitConverter.ToSingle(databytes) * rate),2);
                case DataType.Double:
                    return Math.Round(BitConverter.ToDouble(databytes) * rate,2);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 数据类型对应的字节数
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static int DataTypeLength(DataType dataType)
        {
            switch (dataType)
            {
                //case DataType.Byte:
                //    return 1;
                //case DataType.Sint:
                //    return 1;
                case DataType.uShort:
                    return 2;
                case DataType.Short:
                    return 2;
                case DataType.uInt:
                    return 4;
                case DataType.Int:
                    return 4;
                case DataType.uLong:
                    return 8;
                case DataType.Long:
                    return 8;
                case DataType.Float:
                    return 4;
                case DataType.Double:
                    return 8;
                //case DataType.Bool:
                //    return 1;
                default:
                    return 1;
            }
        }

        ///// <summary>
        ///// 从大端数组中指定位置读取short数据
        ///// </summary>
        ///// <param name="data"></param>
        ///// <param name="p"></param>
        ///// <returns></returns>
        //public static ushort GetUInt16FromBigEndianBytes(byte[] data, int p)
        //{
        //    return BitConverter.ToUInt16(SmallBigConvert(data, p, 2), 0);
        //}


        /// <summary>
        /// 字节序转换
        /// </summary>
        /// <param name="val"></param>
        /// <param name="byteOrder"></param>
        /// <returns></returns>
        public static byte[] ByteOrder(byte[] val, ModbusByteOrder byteOrder)
        {
            //若为单字节的则直接返回
            if (val.Length<=1)
            {
                return val;
            }
            //TODO 字节序处理
            switch (byteOrder)
            {
                case ModbusByteOrder.ABCD:
                    return val;
                case ModbusByteOrder.BADC:
                    byte[] re = new byte[val.Length];
                    for (int i = 0; i < val.Length; i++)
                    {
                        byte item = val[i];
                        if (i % 2 == 1)
                        {
                            re[i - 1] = item;
                        }
                        else
                        {
                            re[i + 1] = item;
                        }
                    }
                    return re;
                case ModbusByteOrder.CDAB:
                    return val;
                case ModbusByteOrder.DCBA:
                    return val.Reverse().ToArray();
                default:
                    return val;
            }
        }
    }
}
