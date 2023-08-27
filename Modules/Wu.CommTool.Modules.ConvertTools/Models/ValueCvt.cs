using Prism.Mvvm;
using System;
using Wu.CommTool.Shared.Enums;
using Wu.Extensions;
using Wu.CommTool.Shared.Common;
using System.Windows.Media;

namespace Wu.CommTool.Modules.ConvertTools.Models
{
    /// <summary>
    /// 值转换
    /// </summary>
    public class ValueCvt : BindableBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ValueCvt()
        {
            ABCD_16wHex = "0000";
            ABCD_32wHex = "00000000";
            ABCD_64wHex = "0000000000000000";
        }


        #region 16位方法
        /// <summary>
        /// 将相关的16位值赋值null
        /// </summary>
        private void Set16wNull()
        {
            SetProperty(ref _ABCD_16wHex, null, nameof(ABCD_16wHex));
            SetProperty(ref _ABCD_16Int, null, nameof(ABCD_16Int));
            SetProperty(ref _ABCD_16Uint, null, nameof(ABCD_16Uint));

            SetProperty(ref _DCBA_16wHex, null, nameof(DCBA_16wHex));
            SetProperty(ref _DCBA_16Int, null, nameof(DCBA_16Int));
            SetProperty(ref _DCBA_16Uint, null, nameof(DCBA_16Uint));

            SetProperty(ref _BADC_16wHex, null, nameof(BADC_16wHex));
            SetProperty(ref _BADC_16Int, null, nameof(BADC_16Int));
            SetProperty(ref _BADC_16Uint, null, nameof(BADC_16Uint));

            SetProperty(ref _CDAB_16wHex, null, nameof(CDAB_16wHex));
            SetProperty(ref _CDAB_16Int, null, nameof(CDAB_16Int));
            SetProperty(ref _CDAB_16Uint, null, nameof(CDAB_16Uint));
        }

        /// <summary>
        /// 根据16进制字符串更新相关数值 (需要将相应的16进制字符串先更新再调用该方法)
        /// </summary>
        public void Set16wValue(string val, ModbusByteOrder byteOrder)
        {
            //根据字节序给ABCD赋值 其他值都根据该值转换
            switch (byteOrder)
            {
                case ModbusByteOrder.ABCD:
                    SetProperty(ref _ABCD_16wHex, val.InsertFormat(2, " "), nameof(ABCD_16wHex));
                    break;
                case ModbusByteOrder.BADC:
                    SetProperty(ref _ABCD_16wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.BADC).InsertFormat(2, " "), nameof(ABCD_16wHex));
                    break;
                case ModbusByteOrder.CDAB:
                    SetProperty(ref _ABCD_16wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.CDAB).InsertFormat(2, " "), nameof(ABCD_16wHex));
                    break;
                case ModbusByteOrder.DCBA:
                    SetProperty(ref _ABCD_16wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.DCBA).InsertFormat(2, " "), nameof(ABCD_16wHex));
                    break;
            }
            //先赋值各参数的16进制符号
            //SetProperty(ref _ABCD_16wHex, val, nameof(ABCD_16wHex));
            SetProperty(ref _DCBA_16wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_16wHex!, ModbusByteOrder.DCBA).InsertFormat(2, " "), nameof(DCBA_16wHex));
            SetProperty(ref _BADC_16wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_16wHex!, ModbusByteOrder.BADC).InsertFormat(2, " "), nameof(BADC_16wHex));
            SetProperty(ref _CDAB_16wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_16wHex!, ModbusByteOrder.CDAB).InsertFormat(2, " "), nameof(CDAB_16wHex));

            //在根据16进制值转换
            SetProperty(ref _ABCD_16Int, Convert.ToInt16(_ABCD_16wHex!.RemoveSpace(), 16), nameof(ABCD_16Int));
            SetProperty(ref _ABCD_16Uint, Convert.ToUInt16(_ABCD_16wHex!.RemoveSpace(), 16), nameof(ABCD_16Uint));

            SetProperty(ref _DCBA_16Int, Convert.ToInt16(_DCBA_16wHex!.RemoveSpace(), 16), nameof(DCBA_16Int));
            SetProperty(ref _DCBA_16Uint, Convert.ToUInt16(_DCBA_16wHex!.RemoveSpace(), 16), nameof(DCBA_16Uint));

            SetProperty(ref _BADC_16Int, Convert.ToInt16(_BADC_16wHex!.RemoveSpace(), 16), nameof(BADC_16Int));
            SetProperty(ref _BADC_16Uint, Convert.ToUInt16(_BADC_16wHex!.RemoveSpace(), 16), nameof(BADC_16Uint));

            SetProperty(ref _CDAB_16Int, Convert.ToInt16(_CDAB_16wHex!.RemoveSpace(), 16), nameof(CDAB_16Int));
            SetProperty(ref _CDAB_16Uint, Convert.ToUInt16(_CDAB_16wHex!.RemoveSpace(), 16), nameof(CDAB_16Uint));
        }

        /// <summary>
        /// 检查16位的16进制字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns>检查并转换后的值,字符串是否有效</returns>
        public static Tuple<string, bool> Check16wHex(string? value)
        {
            var val = value?.RemoveSpace().TrimStart('0') ?? string.Empty;//移除空格 移除左侧的0
            //Tuple<string, bool> tuple = new Tuple<string, bool>("", true);

            if (string.IsNullOrEmpty(val))
            {
                val = "0000";
                return Tuple.Create(val!, true);
            }
            //含非合法16进制字符
            else if (!Shared.Common.Utils.IsHexString(val))
            {
                return Tuple.Create(val!, false);//该值是错误的值
            }
            //字符串符合16进制字符
            else
            {
                //若位数不够则高位补0
                if (val.Length < 4)
                    val = val.PadLeft(4, '0');//高位补0
                else if (val.Length > 4)//若位数超过4位则判断值是否超过FFFF
                    val = "FFFF";
                return Tuple.Create(val!, true);
            }
        }

        /// <summary>
        /// 将ushort转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Ushort2Hex(ushort input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }

        /// <summary>
        /// 将short转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Short2Hex(short input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }
        #endregion

        #region 32位方法
        /// <summary>
        /// 将相关的32位值赋值null
        /// </summary>
        private void Set32wNull()
        {
            SetProperty(ref _ABCD_32wHex, null, nameof(ABCD_32wHex));
            SetProperty(ref _ABCD_32Int, null, nameof(ABCD_32Int));
            SetProperty(ref _ABCD_32Uint, null, nameof(ABCD_32Uint));
            SetProperty(ref _ABCD_Float, null, nameof(ABCD_Float));

            SetProperty(ref _DCBA_32wHex, null, nameof(DCBA_32wHex));
            SetProperty(ref _DCBA_32Int, null, nameof(DCBA_32Int));
            SetProperty(ref _DCBA_32Uint, null, nameof(DCBA_32Uint));
            SetProperty(ref _DCBA_Float, null, nameof(DCBA_Float));

            SetProperty(ref _BADC_32wHex, null, nameof(BADC_32wHex));
            SetProperty(ref _BADC_32Int, null, nameof(BADC_32Int));
            SetProperty(ref _BADC_32Uint, null, nameof(BADC_32Uint));
            SetProperty(ref _BADC_Float, null, nameof(BADC_Float));

            SetProperty(ref _CDAB_32wHex, null, nameof(CDAB_32wHex));
            SetProperty(ref _CDAB_32Int, null, nameof(CDAB_32Int));
            SetProperty(ref _CDAB_32Uint, null, nameof(CDAB_32Uint));
            SetProperty(ref _CDAB_Float, null, nameof(CDAB_Float));
        }

        /// <summary>
        /// 根据32进制字符串更新相关数值 (需要将相应的16进制字符串先更新再调用该方法)
        /// </summary>
        public void Set32wValue(string val, ModbusByteOrder byteOrder)
        {
            //根据字节序给ABCD赋值 其他值都根据该值转换
            switch (byteOrder)
            {
                case ModbusByteOrder.ABCD:
                    SetProperty(ref _ABCD_32wHex, val.InsertFormat(2, " "), nameof(ABCD_32wHex));
                    break;
                case ModbusByteOrder.BADC:
                    SetProperty(ref _ABCD_32wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.BADC).InsertFormat(2, " "), nameof(ABCD_32wHex));
                    break;
                case ModbusByteOrder.CDAB:
                    SetProperty(ref _ABCD_32wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.CDAB).InsertFormat(2, " "), nameof(ABCD_32wHex));
                    break;
                case ModbusByteOrder.DCBA:
                    SetProperty(ref _ABCD_32wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.DCBA).InsertFormat(2, " "), nameof(ABCD_32wHex));
                    break;
            }
            //先赋值各参数的16进制符号
            SetProperty(ref _DCBA_32wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_32wHex!, ModbusByteOrder.DCBA).InsertFormat(2, " "), nameof(DCBA_32wHex));
            SetProperty(ref _BADC_32wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_32wHex!, ModbusByteOrder.BADC).InsertFormat(2, " "), nameof(BADC_32wHex));
            SetProperty(ref _CDAB_32wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_32wHex!, ModbusByteOrder.CDAB).InsertFormat(2, " "), nameof(CDAB_32wHex));

            //再根据16进制值转换
            SetProperty(ref _ABCD_32Int, Convert.ToInt32(_ABCD_32wHex!.RemoveSpace(), 16), nameof(ABCD_32Int));
            SetProperty(ref _ABCD_32Uint, Convert.ToUInt32(_ABCD_32wHex!.RemoveSpace(), 16), nameof(ABCD_32Uint));
            SetProperty(ref _ABCD_Float, BitConverter.ToSingle(BitConverter.GetBytes((uint)_ABCD_32Uint!), 0), nameof(ABCD_Float));

            SetProperty(ref _DCBA_32Int, Convert.ToInt32(_DCBA_32wHex!.RemoveSpace(), 16), nameof(DCBA_32Int));
            SetProperty(ref _DCBA_32Uint, Convert.ToUInt32(_DCBA_32wHex!.RemoveSpace(), 16), nameof(DCBA_32Uint));
            SetProperty(ref _DCBA_Float, BitConverter.ToSingle(BitConverter.GetBytes((uint)_DCBA_32Uint!), 0), nameof(DCBA_Float));

            SetProperty(ref _BADC_32Int, Convert.ToInt32(_BADC_32wHex!.RemoveSpace(), 16), nameof(BADC_32Int));
            SetProperty(ref _BADC_32Uint, Convert.ToUInt32(_BADC_32wHex!.RemoveSpace(), 16), nameof(BADC_32Uint));
            SetProperty(ref _BADC_Float, BitConverter.ToSingle(BitConverter.GetBytes((uint)_BADC_32Uint!), 0), nameof(BADC_Float));

            SetProperty(ref _CDAB_32Int, Convert.ToInt32(_CDAB_32wHex!.RemoveSpace(), 16), nameof(CDAB_32Int));
            SetProperty(ref _CDAB_32Uint, Convert.ToUInt32(_CDAB_32wHex!.RemoveSpace(), 16), nameof(CDAB_32Uint));
            SetProperty(ref _CDAB_Float, BitConverter.ToSingle(BitConverter.GetBytes((uint)_CDAB_32Uint!), 0), nameof(CDAB_Float));
        }

        /// <summary>
        /// 检查32位的16进制字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns>检查并转换后的值,字符串是否有效</returns>
        public static Tuple<string, bool> Check32wHex(string? value)
        {
            var val = value?.RemoveSpace().TrimStart('0') ?? string.Empty;//移除空格 移除左侧的0

            if (string.IsNullOrEmpty(val))
            {
                val = "00000000";
                return Tuple.Create(val!, true);
            }
            //含非合法16进制字符
            else if (!Shared.Common.Utils.IsHexString(val))
            {
                return Tuple.Create(val!, false);//该值是错误的值
            }
            //字符串符合16进制字符
            else
            {
                //若位数不够则高位补0
                if (val.Length < 8)
                    val = val.PadLeft(8, '0');//高位补0
                else if (val.Length > 8)//若位数超过4位则判断值是否超过FFFF
                    val = "FFFFFFFF";
                return Tuple.Create(val!, true);
            }
        }


        /// <summary>
        /// 将uint转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Uint2Hex(uint input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }

        /// <summary>
        /// 将int转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Int2Hex(int input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }

        /// <summary>
        /// 将float转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Float2Hex(float input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }
        #endregion

        #region 64位方法
        /// <summary>
        /// 将相关的16位值赋值null
        /// </summary>
        private void Set64wNull()
        {
            SetProperty(ref _ABCD_64wHex, null, nameof(ABCD_64Int));
            SetProperty(ref _ABCD_64Int, null, nameof(ABCD_64Int));
            SetProperty(ref _ABCD_64Uint, null, nameof(ABCD_64Uint));
            SetProperty(ref _ABCD_Double, null, nameof(ABCD_Float));

            SetProperty(ref _DCBA_64wHex, null, nameof(DCBA_64wHex));
            SetProperty(ref _DCBA_64Int, null, nameof(DCBA_64Int));
            SetProperty(ref _DCBA_64Uint, null, nameof(DCBA_64Uint));
            SetProperty(ref _DCBA_Double, null, nameof(DCBA_Float));

            SetProperty(ref _BADC_64wHex, null, nameof(BADC_64wHex));
            SetProperty(ref _BADC_64Int, null, nameof(BADC_64Int));
            SetProperty(ref _BADC_64Uint, null, nameof(BADC_64Uint));
            SetProperty(ref _BADC_Double, null, nameof(BADC_Float));

            SetProperty(ref _CDAB_64wHex, null, nameof(CDAB_64wHex));
            SetProperty(ref _CDAB_64Int, null, nameof(CDAB_64Int));
            SetProperty(ref _CDAB_64Uint, null, nameof(CDAB_64Uint));
            SetProperty(ref _CDAB_Double, null, nameof(CDAB_Float));
        }

        /// <summary>
        /// 检查64位的16进制字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns>检查并转换后的值,字符串是否有效</returns>
        public static Tuple<string, bool> Check64wHex(string? value)
        {
            var val = value?.RemoveSpace().TrimStart('0') ?? string.Empty;//移除空格 移除左侧的0

            if (string.IsNullOrEmpty(val))
            {
                val = "0000000000000000";
                return Tuple.Create(val!, true);
            }
            //含非合法16进制字符
            else if (!Shared.Common.Utils.IsHexString(val))
            {
                return Tuple.Create(val!, false);//该值是错误的值
            }
            //字符串符合16进制字符
            else
            {
                //若位数不够则高位补0
                if (val.Length < 16)
                    val = val.PadLeft(16, '0');//高位补0
                else if (val.Length > 16)//若位数超过4位则判断值是否超过FFFF
                    val = "FFFFFFFFFFFFFFFF";
                return Tuple.Create(val!, true);
            }
        }


        /// <summary>
        /// 根据32进制字符串更新相关数值 (需要将相应的16进制字符串先更新再调用该方法)
        /// </summary>
        public void Set64wValue(string val, ModbusByteOrder byteOrder)
        {
            //根据字节序给ABCD赋值 其他值都根据该值转换
            switch (byteOrder)
            {
                case ModbusByteOrder.ABCD:
                    SetProperty(ref _ABCD_64wHex, val.InsertFormat(2, " "), nameof(ABCD_64wHex));
                    break;
                case ModbusByteOrder.BADC:
                    SetProperty(ref _ABCD_64wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.BADC).InsertFormat(2, " "), nameof(ABCD_64wHex));
                    break;
                case ModbusByteOrder.CDAB:
                    SetProperty(ref _ABCD_64wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.CDAB).InsertFormat(2, " "), nameof(ABCD_64wHex));
                    break;
                case ModbusByteOrder.DCBA:
                    SetProperty(ref _ABCD_64wHex, Shared.Common.Utils.ConvertByteOrder(val, ModbusByteOrder.DCBA).InsertFormat(2, " "), nameof(ABCD_64wHex));
                    break;
            }
            //先赋值各参数的16进制符号
            SetProperty(ref _DCBA_64wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_64wHex!, ModbusByteOrder.DCBA).InsertFormat(2, " "), nameof(DCBA_64wHex));
            SetProperty(ref _BADC_64wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_64wHex!, ModbusByteOrder.BADC).InsertFormat(2, " "), nameof(BADC_64wHex));
            SetProperty(ref _CDAB_64wHex, Shared.Common.Utils.ConvertByteOrder(_ABCD_64wHex!, ModbusByteOrder.CDAB).InsertFormat(2, " "), nameof(CDAB_64wHex));

            //再根据16进制值转换
            SetProperty(ref _ABCD_64Int, Convert.ToInt64(_ABCD_64wHex!.RemoveSpace(), 16), nameof(ABCD_64Int));
            SetProperty(ref _ABCD_64Uint, Convert.ToUInt64(_ABCD_64wHex!.RemoveSpace(), 16), nameof(ABCD_64Uint));
            SetProperty(ref _ABCD_Double, BitConverter.ToDouble(BitConverter.GetBytes((ulong)_ABCD_64Uint!), 0), nameof(ABCD_Double));

            SetProperty(ref _DCBA_64Int, Convert.ToInt64(_DCBA_64wHex!.RemoveSpace(), 16), nameof(DCBA_64Int));
            SetProperty(ref _DCBA_64Uint, Convert.ToUInt64(_DCBA_64wHex!.RemoveSpace(), 16), nameof(DCBA_64Uint));
            SetProperty(ref _DCBA_Double, BitConverter.ToDouble(BitConverter.GetBytes((ulong)_DCBA_64Uint!), 0), nameof(DCBA_Double));

            SetProperty(ref _BADC_64Int, Convert.ToInt64(_BADC_64wHex!.RemoveSpace(), 16), nameof(BADC_64Int));
            SetProperty(ref _BADC_64Uint, Convert.ToUInt64(_BADC_64wHex!.RemoveSpace(), 16), nameof(BADC_64Uint));
            SetProperty(ref _BADC_Double, BitConverter.ToDouble(BitConverter.GetBytes((ulong)_BADC_64Uint!), 0), nameof(BADC_Double));

            SetProperty(ref _CDAB_64Int, Convert.ToInt64(_CDAB_64wHex!.RemoveSpace(), 16), nameof(CDAB_64Int));
            SetProperty(ref _CDAB_64Uint, Convert.ToUInt64(_CDAB_64wHex!.RemoveSpace(), 16), nameof(CDAB_64Uint));
            SetProperty(ref _CDAB_Double, BitConverter.ToDouble(BitConverter.GetBytes((ulong)_CDAB_64Uint!), 0), nameof(CDAB_Double));
        }

        /// <summary>
        /// 将ulong转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Ulong2Hex(ulong input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }

        /// <summary>
        /// 将long转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Long2Hex(long input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }

        /// <summary>
        /// 将double转换为16进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Double2Hex(double input)
        {
            byte[] temp = BitConverter.GetBytes(input);
            Array.Reverse(temp);
            return BitConverter.ToString(temp, 0).Replace("-", "");
        }
        #endregion


        #region ABCD
        /// <summary>
        /// ABCD 16位16进制
        /// </summary>
        public string? ABCD_16wHex
        {
            get => _ABCD_16wHex;
            set
            {
                var result = Check16wHex(value);   //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set16wNull();                                  //将相关的值赋null
                    SetProperty(ref _ABCD_16wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set16wValue(val, ModbusByteOrder.ABCD);//根据16进制字符串更新其他数值
            }
        }
        private string? _ABCD_16wHex;

        /// <summary>
        /// ABCD 32位16进制
        /// </summary>
        public string? ABCD_32wHex
        {
            get => _ABCD_32wHex;
            set
            {
                var result = Check32wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set32wNull();                                  //将相关的值赋null
                    SetProperty(ref _ABCD_32wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set32wValue(val, ModbusByteOrder.ABCD);//根据16进制字符串更新其他数值
            }
        }
        private string? _ABCD_32wHex;

        /// <summary>
        /// ABCD 64位16进制
        /// </summary>
        public string? ABCD_64wHex
        {
            get => _ABCD_64wHex;
            set
            {
                var result = Check64wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set64wNull();                                  //将相关的值赋null
                    SetProperty(ref _ABCD_64wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set64wValue(val, ModbusByteOrder.ABCD);//根据16进制字符串更新其他数值
            }
        }
        private string? _ABCD_64wHex;

        /// <summary>
        /// ABCD 16进制无符号整型
        /// </summary>
        public ushort? ABCD_16Uint
        {
            get => _ABCD_16Uint;
            set
            {
                SetProperty(ref _ABCD_16Uint, value);
                ABCD_16wHex = Ushort2Hex(value ?? 0);
            }
        }
        private ushort? _ABCD_16Uint;

        /// <summary>
        /// ABCD 16进制有符号整型
        /// </summary>
        public short? ABCD_16Int
        {
            get => _ABCD_16Int;
            set
            {
                SetProperty(ref _ABCD_16Int, value);
                ABCD_16wHex = Short2Hex(value ?? 0);
            }
        }
        private short? _ABCD_16Int;

        /// <summary>
        /// ABCD 32进制无符号整型
        /// </summary>
        public uint? ABCD_32Uint
        {
            get => _ABCD_32Uint;
            set
            {
                SetProperty(ref _ABCD_32Uint, value);
                ABCD_32wHex = Uint2Hex(value ?? 0);
            }
        }
        private uint? _ABCD_32Uint;

        /// <summary>
        /// ABCD 32进制有符号整型
        /// </summary>
        public int? ABCD_32Int
        {
            get => _ABCD_32Int;
            set
            {
                SetProperty(ref _ABCD_32Int, value);
                ABCD_32wHex = Int2Hex(value ?? 0);
            }
        }
        private int? _ABCD_32Int;

        /// <summary>
        /// ABCD 32位浮点型
        /// </summary>
        public float? ABCD_Float
        {
            get => _ABCD_Float;
            set
            {
                SetProperty(ref _ABCD_Float, value);
                ABCD_32wHex = Float2Hex(value ?? 0);
            }
        }
        private float? _ABCD_Float;

        /// <summary>
        /// ABCD 64进制无符号整型
        /// </summary>
        public ulong? ABCD_64Uint
        {
            get => _ABCD_64Uint;
            set
            {
                SetProperty(ref _ABCD_64Uint, value);
                ABCD_64wHex = Ulong2Hex(value ?? 0);
            }
        }
        private ulong? _ABCD_64Uint;

        /// <summary>
        /// ABCD 64进制有符号整型
        /// </summary>
        public long? ABCD_64Int
        {
            get => _ABCD_64Int;
            set
            {
                SetProperty(ref _ABCD_64Int, value);
                ABCD_64wHex = Long2Hex(value ?? 0);
            }
        }
        private long? _ABCD_64Int;

        /// <summary>
        /// ABCD 64位浮点型
        /// </summary>
        public double? ABCD_Double
        {
            get => _ABCD_Double;
            set
            {
                SetProperty(ref _ABCD_Double, value);
                ABCD_64wHex = Double2Hex(value ?? 0);
            }
        }
        private double? _ABCD_Double;
        #endregion

        #region DCBA
        /// <summary>
        /// DCBA 16进制
        /// </summary>
        public string? DCBA_16wHex
        {
            get => _DCBA_16wHex;
            set
            {
                var result = Check16wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set16wNull();                                  //将相关的值赋null
                    SetProperty(ref _DCBA_16wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set16wValue(val, ModbusByteOrder.DCBA);//根据16进制字符串更新其他数值
            }
        }
        private string? _DCBA_16wHex;

        /// <summary>
        /// DCBA 16进制无符号整型
        /// </summary>
        public ushort? DCBA_16Uint
        {
            get => _DCBA_16Uint;
            set
            {
                SetProperty(ref _DCBA_16Uint, value);
                DCBA_16wHex = Ushort2Hex(value ?? 0);
            }
        }
        private ushort? _DCBA_16Uint;

        /// <summary>
        /// DCBA 16进制有符号整型
        /// </summary>
        public short? DCBA_16Int
        {
            get => _DCBA_16Int; set
            {
                SetProperty(ref _DCBA_16Int, value);
                DCBA_16wHex = Short2Hex(value ?? 0);
            }
        }
        private short? _DCBA_16Int;


        public string? DCBA_32wHex
        {
            get => _DCBA_32wHex;
            set
            {
                var result = Check32wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set32wNull();                                  //将相关的值赋null
                    SetProperty(ref _DCBA_32wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set32wValue(val, ModbusByteOrder.DCBA);//根据16进制字符串更新其他数值
            }
        }
        private string? _DCBA_32wHex;


        /// <summary>
        /// DCBA 32进制无符号整型
        /// </summary>
        public uint? DCBA_32Uint
        {
            get => _DCBA_32Uint;
            set
            {
                SetProperty(ref _DCBA_32Uint, value);
                DCBA_32wHex = Uint2Hex(value ?? 0);
            }
        }
        private uint? _DCBA_32Uint;

        /// <summary>
        /// DCBA 32进制有符号整型
        /// </summary>
        public int? DCBA_32Int
        {
            get => _DCBA_32Int; set
            {
                SetProperty(ref _DCBA_32Int, value);
                DCBA_32wHex = Int2Hex(value ?? 0);
            }
        }
        private int? _DCBA_32Int;

        /// <summary>
        /// DCBA 32位浮点型
        /// </summary>
        public float? DCBA_Float
        {
            get => _DCBA_Float;
            set
            {
                SetProperty(ref _DCBA_Float, value);
                DCBA_32wHex = Float2Hex(value ?? 0);
            }
        }
        private float? _DCBA_Float;

        /// <summary>
        /// DCBA 64位16进制
        /// </summary>
        public string? DCBA_64wHex
        {
            get => _DCBA_64wHex;
            set
            {
                var result = Check64wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set64wNull();                                  //将相关的值赋null
                    SetProperty(ref _DCBA_64wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set64wValue(val, ModbusByteOrder.DCBA);//根据16进制字符串更新其他数值
            }
        }
        private string? _DCBA_64wHex;

        /// <summary>
        /// DCBA 64进制无符号整型
        /// </summary>
        public ulong? DCBA_64Uint
        {
            get => _DCBA_64Uint;
            set
            {
                SetProperty(ref _DCBA_64Uint, value);
                DCBA_64wHex = Ulong2Hex(value ?? 0);
            }
        }
        private ulong? _DCBA_64Uint;

        /// <summary>
        /// DCBA 64进制有符号整型
        /// </summary>
        public long? DCBA_64Int
        {
            get => _DCBA_64Int;
            set
            {
                SetProperty(ref _DCBA_64Int, value);
                DCBA_64wHex = Long2Hex(value ?? 0);
            }
        }
        private long? _DCBA_64Int;

        /// <summary>
        /// DCBA 64位浮点型
        /// </summary>
        public double? DCBA_Double 
        { 
            get => _DCBA_Double;
            set
            {
                SetProperty(ref _DCBA_Double, value);
                DCBA_64wHex = Double2Hex(value ?? 0);
            }
        }
        private double? _DCBA_Double;
        #endregion

        #region BADC
        /// <summary>
        /// BADC 16进制
        /// </summary>
        public string? BADC_16wHex
        {
            get => _BADC_16wHex;
            set
            {
                var result = Check16wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set16wNull();                                  //将相关的值赋null
                    SetProperty(ref _BADC_16wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set16wValue(val, ModbusByteOrder.BADC);//根据16进制字符串更新其他数值
            }
        }
        private string? _BADC_16wHex;

        /// <summary>
        /// BADC 16进制无符号整型
        /// </summary>
        public ushort? BADC_16Uint
        {
            get => _BADC_16Uint;
            set
            {
                SetProperty(ref _BADC_16Uint, value);
                BADC_16wHex = Ushort2Hex(value ?? 0);
            }
        }
        private ushort? _BADC_16Uint;

        /// <summary>
        /// BADC 16进制有符号整型
        /// </summary>
        public short? BADC_16Int
        {
            get => _BADC_16Int;
            set
            {
                SetProperty(ref _BADC_16Int, value);
                BADC_16wHex = Short2Hex(value ?? 0);
            }
        }
        private short? _BADC_16Int;

        /// <summary>
        /// BADC 16进制
        /// </summary>
        public string? BADC_32wHex
        {
            get => _BADC_32wHex;
            set
            {
                var result = Check32wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set32wNull();                                  //将相关的值赋null
                    SetProperty(ref _ABCD_32wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set32wValue(val, ModbusByteOrder.BADC);//根据16进制字符串更新其他数值
            }
        }
        private string? _BADC_32wHex;

        /// <summary>
        /// BADC 32进制无符号整型
        /// </summary>
        public uint? BADC_32Uint
        {
            get => _BADC_32Uint; 
            set
            {
                SetProperty(ref _BADC_32Uint, value);
                BADC_32wHex = Uint2Hex(value ?? 0);
            }
        }
        private uint? _BADC_32Uint;

        /// <summary>
        /// BADC 32进制有符号整型
        /// </summary>
        public int? BADC_32Int
        {
            get => _BADC_32Int;
            set
            {
                SetProperty(ref _BADC_32Int, value);
                BADC_32wHex = Int2Hex(value ?? 0);
            }
        }
        private int? _BADC_32Int;

        /// <summary>
        /// BADC 32位浮点型
        /// </summary>
        public float? BADC_Float
        {
            get => _BADC_Float;
            set
            {
                SetProperty(ref _BADC_Float, value);
                BADC_32wHex = Float2Hex(value ?? 0);
            }
        }
        private float? _BADC_Float;

        /// <summary>
        /// BADC 64位16进制
        /// </summary>
        public string? BADC_64wHex
        {
            get => _BADC_64wHex;
            set
            {
                var result = Check64wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set64wNull();                                  //将相关的值赋null
                    SetProperty(ref _BADC_64wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set64wValue(val, ModbusByteOrder.BADC);//根据16进制字符串更新其他数值
            }
        }
        private string? _BADC_64wHex;

        /// <summary>
        /// BADC 64进制无符号整型
        /// </summary>
        public ulong? BADC_64Uint
        {
            get => _BADC_64Uint;
            set
            {
                SetProperty(ref _BADC_64Uint, value);
                BADC_64wHex = Ulong2Hex(value ?? 0);
            }
        }
        private ulong? _BADC_64Uint;

        /// <summary>
        /// BADC 64进制有符号整型
        /// </summary>
        public long? BADC_64Int 
        { 
            get => _BADC_64Int;
            set
            {
                SetProperty(ref _BADC_64Int, value);
                BADC_64wHex = Long2Hex(value ?? 0);
            }
        }
        private long? _BADC_64Int;

        /// <summary>
        /// BADC 64位浮点型
        /// </summary>
        public double? BADC_Double 
        { 
            get => _BADC_Double;
            set
            {
                SetProperty(ref _BADC_Double, value);
                BADC_64wHex = Double2Hex(value ?? 0);
            }
        }
        private double? _BADC_Double;
        #endregion

        #region CDAB
        /// <summary>
        /// CDAB 16进制
        /// </summary>
        public string? CDAB_16wHex
        {
            get => _CDAB_16wHex;
            set
            {
                var result = Check16wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set16wNull();                                  //将相关的值赋null
                    SetProperty(ref _CDAB_16wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set16wValue(val, ModbusByteOrder.CDAB);//根据16进制字符串更新其他数值
            }
        }
        private string? _CDAB_16wHex;

        /// <summary>
        /// CDAB 16进制无符号整型
        /// </summary>
        public ushort? CDAB_16Uint
        {
            get => _CDAB_16Uint;
            set
            {
                SetProperty(ref _CDAB_16Uint, value);
                CDAB_16wHex = Ushort2Hex(value ?? 0);
            }
        }
        private ushort? _CDAB_16Uint;

        /// <summary>
        /// CDAB 16进制有符号整型
        /// </summary>
        public short? CDAB_16Int
        {
            get => _CDAB_16Int;
            set
            {
                SetProperty(ref _CDAB_16Int, value);
                CDAB_16wHex = Short2Hex(value ?? 0);
            }
        }
        private short? _CDAB_16Int;

        /// <summary>
        /// CDAB 16进制
        /// </summary>
        public string? CDAB_32wHex
        {
            get => _CDAB_32wHex;
            set
            {
                var result = Check32wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set32wNull();                                  //将相关的值赋null
                    SetProperty(ref _ABCD_32wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set32wValue(val, ModbusByteOrder.CDAB);//根据16进制字符串更新其他数值
            }
        }
        private string? _CDAB_32wHex;

        /// <summary>
        /// CDAB 32进制无符号整型
        /// </summary>
        public uint? CDAB_32Uint
        {
            get => _CDAB_32Uint;
            set
            {
                SetProperty(ref _CDAB_32Uint, value);
                CDAB_32wHex = Uint2Hex(value ?? 0);
            }
        }
        private uint? _CDAB_32Uint;

        /// <summary>
        /// CDAB 32进制有符号整型
        /// </summary>
        public int? CDAB_32Int
        {
            get => _CDAB_32Int; set
            {
                SetProperty(ref _CDAB_32Int, value);
                CDAB_32wHex = Int2Hex(value ?? 0);
            }
        }
        private int? _CDAB_32Int;

        /// <summary>
        /// CDAB 32位浮点型
        /// </summary>
        public float? CDAB_Float
        {
            get => _CDAB_Float;
            set
            {
                SetProperty(ref _CDAB_Float, value);
                CDAB_32wHex = Float2Hex(value ?? 0);
            }
        }
        private float? _CDAB_Float;

        /// <summary>
        /// CDAB 64位16进制
        /// </summary>
        public string? CDAB_64wHex
        {
            get => _CDAB_64wHex;
            set
            {
                var result = Check64wHex(value);    //检查字符串
                var val = result.Item1;                      //获取检查后的结果
                if (result.Item2 == false)                         //字符串非法
                {
                    Set64wNull();                                  //将相关的值赋null
                    SetProperty(ref _CDAB_64wHex, val);            //当前值保留 可供修改
                    return;
                }
                Set64wValue(val, ModbusByteOrder.CDAB);//根据16进制字符串更新其他数值
            }
        }
        private string? _CDAB_64wHex;

        /// <summary>
        /// CDAB 64进制无符号整型
        /// </summary>
        public ulong? CDAB_64Uint 
        { 
            get => _CDAB_64Uint;
            set
            {
                SetProperty(ref _CDAB_64Uint, value);
                CDAB_64wHex = Ulong2Hex(value ?? 0);
            }
        }
        private ulong? _CDAB_64Uint;

        /// <summary>
        /// CDAB 64进制有符号整型
        /// </summary>
        public long? CDAB_64Int 
        { 
            get => _CDAB_64Int;
            set
            {
                SetProperty(ref _CDAB_64Int, value);
                CDAB_64wHex = Long2Hex(value ?? 0);
            }
        }
        private long? _CDAB_64Int;

        /// <summary>
        /// CDAB 64位浮点型
        /// </summary>
        public double? CDAB_Double
        {
            get => _CDAB_Double;
            set
            {
                SetProperty(ref _CDAB_Double, value);
                CDAB_64wHex = Double2Hex(value ?? 0);
            }
        }
        private double? _CDAB_Double;
        #endregion
    }
}
