using ImTools;
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
using System.Windows.Controls;
using Wu.Extensions;
using Wu.FzWater.Mqtt;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// ModbusRtu数据帧
    /// </summary>
    public class ModbusRtuFrame : BindableBase
    {
        public ModbusRtuFrame(byte[] frame)
        {
            Frame = frame;//缓存帧
                          //CRC验证通过后

            SlaveId = frame[0];
            Function = frame[1];

            //03功能码
            if (Function == 0x03)
            {
                //请求帧
                if (frame.Length.Equals(8))
                {
                    StartAddr = frame.Skip(2).Take(2).ToArray();
                    DataCount = frame.Skip(4).Take(2).ToArray();
                    CrcCode = frame.Skip(6).Take(2).ToArray();
                }
                //应答帧
                else
                {
                    DataCount = frame.Skip(2).Take(1).ToArray();
                    Datas = frame.Skip(3).Take(frame.Length - 5).ToArray();
                    CrcCode = frame.Skip(frame.Length - 2).Take(2).ToArray();
                }
            }
            else
            {

            }
        }


        /// <summary>
        /// 从站ID
        /// </summary>
        public byte SlaveId { get => _SlaveId; set => SetProperty(ref _SlaveId, value); }
        private byte _SlaveId;

        /// <summary>
        /// 功能码
        /// </summary>
        public byte Function { get => _Function; set => SetProperty(ref _Function, value); }
        private byte _Function;

        /// <summary>
        /// 起始地址
        /// </summary>
        public byte[] StartAddr { get => _StartAddr; set => SetProperty(ref _StartAddr, value); }
        private byte[] _StartAddr;

        /// <summary>
        /// 数据数量 单位word
        /// </summary>
        public byte[] DataCount { get => _DataCount; set => SetProperty(ref _DataCount, value); }
        private byte[] _DataCount;

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Datas { get => _Datas; set => SetProperty(ref _Datas, value); }
        private byte[] _Datas;

        /// <summary>
        /// CRC校验码
        /// </summary>
        public byte[] CrcCode { get => _CrcCode; set => SetProperty(ref _CrcCode, value); }
        private byte[] _CrcCode;

        /// <summary>
        /// 帧
        /// </summary>
        public byte[] Frame { get => _Frame; set => SetProperty(ref _Frame, value); }
        private byte[] _Frame;


        private string DatasFormat(byte[] input)
        {
            return BitConverter.ToString(input).Replace("-", "").InsertFormat(4, " ");
        }

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //TODO 数据帧功能不同 格式化
            try
            {
                //判断内容为请求帧还是应答帧
                //03功能码 字节数量奇数为应答帧 字节数量=8请求帧    错误码0x83
                if (Function == 0x03)
                {
                    //03功能码请求帧
                    if (Frame.Length == 8)
                    {
                        return $"{SlaveId:X2} {Function:X2} {DatasFormat(StartAddr)} {DatasFormat(DataCount)} {DatasFormat(CrcCode)}";
                    }
                    //判断字节数量是否为奇数
                    if (Frame.Length % 2 == 1)
                    {
                        return $"{SlaveId:X2} {Function:X2} {DatasFormat(DataCount)} {DatasFormat(Datas)} {DatasFormat(CrcCode)}";
                    }
                }
                //TODO 其他功能码
                else
                {

                }
                return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
            }
            catch
            {
                return BitConverter.ToString(Frame).Replace("-", "").InsertFormat(4, " ");
            }
        }
    }
}
