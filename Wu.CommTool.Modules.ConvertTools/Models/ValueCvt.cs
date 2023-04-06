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

namespace Wu.CommTool.Modules.ConvertTools.Models
{
    /// <summary>
    /// 值转换
    /// </summary>
    public class ValueCvt : BindableBase
    {
        #region ABCD
        /// <summary>
        /// ABCD 16进制
        /// </summary>
        public string? ABCD_16Jz
        {
            get => _ABCD_16Jz;
            set
            {
                SetProperty(ref _ABCD_16Jz, value);
                //验证是否为16进制字符串
                //
            }
        }
        private string? _ABCD_16Jz;

        /// <summary>
        /// ABCD 16进制无符号整型
        /// </summary>
        public ushort? ABCD_16Uint { get => _ABCD_16Uint; set => SetProperty(ref _ABCD_16Uint, value); }
        private ushort? _ABCD_16Uint;

        /// <summary>
        /// ABCD 16进制有符号整型
        /// </summary>
        public short? ABCD_16Int { get => _ABCD_16Int; set => SetProperty(ref _ABCD_16Int, value); }
        private short? _ABCD_16Int;

        /// <summary>
        /// ABCD 32进制无符号整型
        /// </summary>
        public ushort? ABCD_32Uint { get => _ABCD_32Uint; set => SetProperty(ref _ABCD_32Uint, value); }
        private ushort? _ABCD_32Uint;

        /// <summary>
        /// ABCD 32进制有符号整型
        /// </summary>
        public short? ABCD_32Int { get => _ABCD_32Int; set => SetProperty(ref _ABCD_32Int, value); }
        private short? _ABCD_32Int;

        /// <summary>
        /// ABCD 32位浮点型
        /// </summary>
        public float? ABCD_Float { get => _ABCD_Float; set => SetProperty(ref _ABCD_Float, value); }
        private float? _ABCD_Float;

        /// <summary>
        /// ABCD 64进制无符号整型
        /// </summary>
        public ushort? ABCD_64Uint { get => _ABCD_64Uint; set => SetProperty(ref _ABCD_64Uint, value); }
        private ushort? _ABCD_64Uint;

        /// <summary>
        /// ABCD 64进制有符号整型
        /// </summary>
        public short? ABCD_64Int { get => _ABCD_64Int; set => SetProperty(ref _ABCD_64Int, value); }
        private short? _ABCD_64Int;

        /// <summary>
        /// ABCD 64位浮点型
        /// </summary>
        public double? ABCD_Double { get => _ABCD_Double; set => SetProperty(ref _ABCD_Double, value); }
        private double? _ABCD_Double;
        #endregion

        #region DCBA
        /// <summary>
        /// DCBA 16进制
        /// </summary>
        public string? DCBA_16Jz { get => _DCBA_16Jz; set => SetProperty(ref _DCBA_16Jz, value); }
        private string? _DCBA_16Jz;

        /// <summary>
        /// DCBA 16进制无符号整型
        /// </summary>
        public ushort? DCBA_16Uint { get => _DCBA_16Uint; set => SetProperty(ref _DCBA_16Uint, value); }
        private ushort? _DCBA_16Uint;

        /// <summary>
        /// DCBA 16进制有符号整型
        /// </summary>
        public short? DCBA_16Int { get => _DCBA_16Int; set => SetProperty(ref _DCBA_16Int, value); }
        private short? _DCBA_16Int;

        /// <summary>
        /// DCBA 32进制无符号整型
        /// </summary>
        public ushort? DCBA_32Uint { get => _DCBA_32Uint; set => SetProperty(ref _DCBA_32Uint, value); }
        private ushort? _DCBA_32Uint;

        /// <summary>
        /// DCBA 32进制有符号整型
        /// </summary>
        public short? DCBA_32Int { get => _DCBA_32Int; set => SetProperty(ref _DCBA_32Int, value); }
        private short? _DCBA_32Int;

        /// <summary>
        /// DCBA 32位浮点型
        /// </summary>
        public float? DCBA_Float { get => _DCBA_Float; set => SetProperty(ref _DCBA_Float, value); }
        private float? _DCBA_Float;

        /// <summary>
        /// DCBA 64进制无符号整型
        /// </summary>
        public ushort? DCBA_64Uint { get => _DCBA_64Uint; set => SetProperty(ref _DCBA_64Uint, value); }
        private ushort? _DCBA_64Uint;

        /// <summary>
        /// DCBA 64进制有符号整型
        /// </summary>
        public short? DCBA_64Int { get => _DCBA_64Int; set => SetProperty(ref _DCBA_64Int, value); }
        private short? _DCBA_64Int;

        /// <summary>
        /// DCBA 64位浮点型
        /// </summary>
        public double? DCBA_Double { get => _DCBA_Double; set => SetProperty(ref _DCBA_Double, value); }
        private double? _DCBA_Double;
        #endregion

        #region BADC
        /// <summary>
        /// BADC 16进制
        /// </summary>
        public string? BADC_16Jz { get => _BADC_16Jz; set => SetProperty(ref _BADC_16Jz, value); }
        private string? _BADC_16Jz;

        /// <summary>
        /// BADC 16进制无符号整型
        /// </summary>
        public ushort? BADC_16Uint { get => _BADC_16Uint; set => SetProperty(ref _BADC_16Uint, value); }
        private ushort? _BADC_16Uint;

        /// <summary>
        /// BADC 16进制有符号整型
        /// </summary>
        public short? BADC_16Int { get => _BADC_16Int; set => SetProperty(ref _BADC_16Int, value); }
        private short? _BADC_16Int;

        /// <summary>
        /// BADC 32进制无符号整型
        /// </summary>
        public ushort? BADC_32Uint { get => _BADC_32Uint; set => SetProperty(ref _BADC_32Uint, value); }
        private ushort? _BADC_32Uint;

        /// <summary>
        /// BADC 32进制有符号整型
        /// </summary>
        public short? BADC_32Int { get => _BADC_32Int; set => SetProperty(ref _BADC_32Int, value); }
        private short? _BADC_32Int;

        /// <summary>
        /// BADC 32位浮点型
        /// </summary>
        public float? BADC_Float { get => _BADC_Float; set => SetProperty(ref _BADC_Float, value); }
        private float? _BADC_Float;

        /// <summary>
        /// BADC 64进制无符号整型
        /// </summary>
        public ushort? BADC_64Uint { get => _BADC_64Uint; set => SetProperty(ref _BADC_64Uint, value); }
        private ushort? _BADC_64Uint;

        /// <summary>
        /// BADC 64进制有符号整型
        /// </summary>
        public short? BADC_64Int { get => _BADC_64Int; set => SetProperty(ref _BADC_64Int, value); }
        private short? _BADC_64Int;

        /// <summary>
        /// BADC 64位浮点型
        /// </summary>
        public double? BADC_Double { get => _BADC_Double; set => SetProperty(ref _BADC_Double, value); }
        private double? _BADC_Double;
        #endregion

        #region CDAB
        /// <summary>
        /// CDAB 16进制
        /// </summary>
        public string? CDAB_16Jz { get => _CDAB_16Jz; set => SetProperty(ref _CDAB_16Jz, value); }
        private string? _CDAB_16Jz;

        /// <summary>
        /// CDAB 16进制无符号整型
        /// </summary>
        public ushort? CDAB_16Uint { get => _CDAB_16Uint; set => SetProperty(ref _CDAB_16Uint, value); }
        private ushort? _CDAB_16Uint;

        /// <summary>
        /// CDAB 16进制有符号整型
        /// </summary>
        public short? CDAB_16Int { get => _CDAB_16Int; set => SetProperty(ref _CDAB_16Int, value); }
        private short? _CDAB_16Int;

        /// <summary>
        /// CDAB 32进制无符号整型
        /// </summary>
        public ushort? CDAB_32Uint { get => _CDAB_32Uint; set => SetProperty(ref _CDAB_32Uint, value); }
        private ushort? _CDAB_32Uint;

        /// <summary>
        /// CDAB 32进制有符号整型
        /// </summary>
        public short? CDAB_32Int { get => _CDAB_32Int; set => SetProperty(ref _CDAB_32Int, value); }
        private short? _CDAB_32Int;

        /// <summary>
        /// CDAB 32位浮点型
        /// </summary>
        public float? CDAB_Float { get => _CDAB_Float; set => SetProperty(ref _CDAB_Float, value); }
        private float? _CDAB_Float;

        /// <summary>
        /// CDAB 64进制无符号整型
        /// </summary>
        public ushort? CDAB_64Uint { get => _CDAB_64Uint; set => SetProperty(ref _CDAB_64Uint, value); }
        private ushort? _CDAB_64Uint;

        /// <summary>
        /// CDAB 64进制有符号整型
        /// </summary>
        public short? CDAB_64Int { get => _CDAB_64Int; set => SetProperty(ref _CDAB_64Int, value); }
        private short? _CDAB_64Int;

        /// <summary>
        /// CDAB 64位浮点型
        /// </summary>
        public double? CDAB_Double { get => _CDAB_Double; set => SetProperty(ref _CDAB_Double, value); }
        private double? _CDAB_Double;
        #endregion
    }
}
