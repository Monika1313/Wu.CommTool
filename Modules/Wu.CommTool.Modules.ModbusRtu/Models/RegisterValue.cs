//using Prism.Commands;
//using Prism.Ioc;
//using Prism.Mvvm;
//using Prism.Regions;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using Wu.CommTool.Modules.ModbusRtu.Enums;
//using System.Threading.Tasks;
//using HandyControl.Controls;

//namespace Wu.CommTool.Modules.ModbusRtu.Models
//{
//    /// <summary>
//    /// 寄存器值
//    /// </summary>
//    public class RegisterValue : BindableBase
//    {
//        /// <summary>
//        /// 名称
//        /// </summary>
//        public string Name { get => _Name; set => SetProperty(ref _Name, value); }
//        private string _Name = string.Empty;

//        /// <summary>
//        /// 数据类型
//        /// </summary>
//        public DataType Type { get => _Type; set => SetProperty(ref _Type, value); }
//        private DataType _Type = DataType.uShort;

//        /// <summary>
//        /// 数据类型的长度 单位=字节
//        /// </summary>
//        public int DataTypeByteLength => GetDataTypeLength(Type);

//        /// <summary>
//        /// 第几个word
//        /// </summary>
//        public int Index { get => _Index; set => SetProperty(ref _Index, value); }
//        private int _Index;

//        /// <summary>
//        /// 值
//        /// </summary>
//        public dynamic? Value { get => _Value; set => SetProperty(ref _Value, value); }
//        private dynamic? _Value;

//        /// <summary>
//        /// 源字节数组
//        /// </summary>
//        public byte[]? OriginBytes
//        {
//            get => _OriginBytes;
//            set
//            {
//                SetProperty(ref _OriginBytes, value);
//                Value = GetVal(DataBytes, Type, 1);
//            }
//        }
//        private byte[]? _OriginBytes;

//        //当前值的原始字节数组(已转换字节序)
//        public byte[]? DataBytes => GetDataBytes(OriginBytes, Location, ModbusByteOrder, Type);






//        /// <summary>
//        /// 根据字节数据 数据类型 获取值
//        /// </summary>
//        /// <param name="databytes"></param>
//        /// <param name="dataType"></param>
//        /// <param name="rate"></param>
//        /// <returns></returns>
//        public static dynamic? GetVal(byte[]? databytes, DataType dataType, double rate)
//        {
//            if (databytes == null)
//            {
//                return 0;
//            }
//            return dataType switch
//            {
//                DataType.uShort => Math.Round(BitConverter.ToUInt16(databytes, 0) * rate, 3),
//                DataType.Short => Math.Round(BitConverter.ToInt16(databytes, 0) * rate, 3),
//                DataType.uInt => Math.Round(BitConverter.ToUInt32(databytes, 0) * rate, 3),
//                DataType.Int => Math.Round(BitConverter.ToInt32(databytes, 0) * rate, 3),
//                DataType.uLong => Math.Round(BitConverter.ToUInt64(databytes, 0) * rate, 3),
//                DataType.Long => Math.Round(BitConverter.ToInt64(databytes, 0) * rate, 3),
//                DataType.Float => Math.Round((BitConverter.ToSingle(databytes, 0) * rate), 2),
//                DataType.Double => Math.Round(BitConverter.ToDouble(databytes, 0) * rate, 2),
//                _ => (dynamic)0,
//            };
//        }


//        /// <summary>
//        /// 字节序转换
//        /// </summary>
//        /// <param name="val"></param>
//        /// <param name="byteOrder"></param>
//        /// <returns></returns>
//        public static byte[] ByteOrder(byte[] val, ModbusByteOrder byteOrder)
//        {
//            //若为单字节的则直接返回
//            if (val.Length <= 1)
//            {
//                return val;
//            }
//            //TODO 字节序处理
//            switch (byteOrder)
//            {
//                case ModbusByteOrder.ABCD:
//                    return val;
//                case ModbusByteOrder.BADC:
//                    byte[] re = new byte[val.Length];
//                    for (int i = 0; i < val.Length; i++)
//                    {
//                        byte item = val[i];
//                        if (i % 2 == 1)
//                        {
//                            re[i - 1] = item;
//                        }
//                        else
//                        {
//                            re[i + 1] = item;
//                        }
//                    }
//                    return re;
//                case ModbusByteOrder.CDAB:
//                    var temp = val.Reverse().ToArray();
//                    byte[] result = new byte[temp.Length];
//                    for (int i = 0; i < temp.Length; i++)
//                    {
//                        byte item = temp[i];
//                        if (i % 2 == 1)
//                        {
//                            result[i - 1] = item;
//                        }
//                        else
//                        {
//                            result[i + 1] = item;
//                        }
//                    }
//                    return val;
//                case ModbusByteOrder.DCBA:
//                    return val.Reverse().ToArray();
//                default:
//                    return val;
//            }
//        }















//        /// <summary>
//        /// 数据类型对应的字节数
//        /// </summary>
//        /// <param name="dataType"></param>
//        /// <returns></returns>
//        public static int GetDataTypeLength(DataType dataType)
//        {
//            return dataType switch
//            {
//                //case DataType.Byte:
//                //    return 1;
//                //case DataType.Sint:
//                //    return 1;
//                DataType.uShort => 2,
//                DataType.Short => 2,
//                DataType.uInt => 4,
//                DataType.Int => 4,
//                DataType.uLong => 8,
//                DataType.Long => 8,
//                DataType.Float => 4,
//                DataType.Double => 8,
//                //case DataType.Bool:
//                //    return 1;
//                _ => 1,
//            };
//        }
//    }
//}
