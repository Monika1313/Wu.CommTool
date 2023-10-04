using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ImTools;
using Wu.CommTool.Modules.ModbusRtu.Enums;
using Wu.Wpf.Models;

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    /// <summary>
    /// ModbusRtu共享实例
    /// </summary>
    public class ModbusRtuModel : BindableBase
    {

        private readonly SerialPort SerialPort = new();              //串口
        private readonly Queue<(string, int)> PublishFrameQueue = new();      //数据帧发送队列
        private readonly Queue<string> ReceiveFrameQueue = new();    //数据帧处理队列



        #region 属性
        /// <summary>
        /// 页面消息
        /// </summary>
        public ObservableCollection<ModbusRtuMessageData> Messages { get => _Messages; set => SetProperty(ref _Messages, value); }
        private ObservableCollection<ModbusRtuMessageData> _Messages = new();

        /// <summary>
        /// 暂停
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause;

        /// <summary>
        /// 串口列表
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> ComPorts { get => _ComPorts; set => SetProperty(ref _ComPorts, value); }
        private ObservableCollection<KeyValuePair<string, string>> _ComPorts = new();

        /// <summary>
        /// Com口配置
        /// </summary>
        public ComConfig ComConfig { get => _ComConfig; set => SetProperty(ref _ComConfig, value); }
        private ComConfig _ComConfig = new();

        /// <summary>
        /// 输入消息 用于发送
        /// </summary>
        public string InputMessage { get => _InputMessage; set => SetProperty(ref _InputMessage, value); }
        private string _InputMessage = "01 03 0000 0001";

        /// <summary>
        /// Crc校验模式
        /// </summary>
        public CrcMode CrcMode { get => _CrcMode; set => SetProperty(ref _CrcMode, value); }
        private CrcMode _CrcMode = CrcMode.Modbus;

        /// <summary>
        /// 接收的数据总数
        /// </summary>
        public int ReceiveBytesCount { get => _ReceiveBytesCount; set => SetProperty(ref _ReceiveBytesCount, value); }
        private int _ReceiveBytesCount = 0;

        /// <summary>
        /// 发送的数据总数
        /// </summary>
        public int SendBytesCount { get => _SendBytesCount; set => SetProperty(ref _SendBytesCount, value); }
        private int _SendBytesCount = 0;
        #endregion

        #region 数据监控功能
        /// <summary>
        /// 数据监控配置
        /// </summary>
        public DataMonitorConfig DataMonitorConfig { get => _DataMonitorConfig; set => SetProperty(ref _DataMonitorConfig, value); }
        private DataMonitorConfig _DataMonitorConfig = new();
        #endregion


        #region 串口操作方法
        /// <summary>
        /// 获取串口完整名字（包括驱动名字）
        /// </summary>
        public void GetComPorts()
        {
            //清空列表
            ComPorts.Clear();
            //查找Com口
            using System.Management.ManagementObjectSearcher searcher = new("select * from Win32_PnPEntity where Name like '%(COM[0-999]%'");
            var hardInfos = searcher.Get();
            foreach (var hardInfo in hardInfos)
            {
                if (hardInfo.Properties["Name"].Value != null)
                {
                    string deviceName = hardInfo.Properties["Name"].Value.ToString()!;         //获取名称
                    List<string> dList = new();                                                 //从名称中截取串口
                    foreach (Match mch in Regex.Matches(deviceName, @"COM\d{1,3}").Cast<Match>())
                    {
                        string x = mch.Value.Trim();
                        dList.Add(x);
                    }
                    int startIndex = deviceName.IndexOf("(");
                    //int endIndex = deviceName.IndexOf(")");
                    //string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                    string key = dList[0];
                    string name = deviceName[..(startIndex - 1)];
                    ComPorts.Add(new KeyValuePair<string, string>(key, name));       //添加进列表
                }
            }
            if (ComPorts.Count != 0)
            {
                //查找第一个USB设备
                var usbDevice = ComPorts.FindFirst(x => x.Value.ToLower().Contains("usb"));
                //有USB设备优先选择USB
                if (usbDevice.Key != null)
                {
                    //默认选中项 若含USB设备则指定第一个USB, 若不含USB则指定第一个
                    ComConfig.Port = usbDevice;
                }
                //其次保持不变
                else if (ComPorts.FirstOrDefault(x=>x.Key==ComConfig.Port.Key && x.Value == ComConfig.Port.Value).Key != null)//保留原选项
                {
                    ComConfig.Port = ComPorts.FirstOrDefault(x => x.Key == ComConfig.Port.Key && x.Value == ComConfig.Port.Value);
                }
                //都没有则选中第一个
                else
                {
                    ComConfig.Port = ComPorts[0];
                }
            }
            string str = $"获取串口成功, 共{ComPorts.Count}个。";
            foreach (var item in ComPorts)
            {
                str += $"   {item.Key}: {item.Value};";
            }
            ShowMessage(str);
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public void OpenCom()
        {
            try
            {
                //判断串口是否已打开
                if (ComConfig.IsOpened)
                {
                    ShowMessage("当前串口已打开, 无法重复开启");
                    return;
                }

                //配置串口
                SerialPort.PortName = ComConfig.Port.Key;                              //串口
                SerialPort.BaudRate = (int)ComConfig.BaudRate;                         //波特率
                SerialPort.Parity = (System.IO.Ports.Parity)ComConfig.Parity;          //校验
                SerialPort.DataBits = ComConfig.DataBits;                              //数据位
                SerialPort.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;    //停止位
                try
                {
                    SerialPort.Open();               //打开串口
                    ComConfig.IsOpened = true;      //标记串口已打开
                    ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.Port.Value}  波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}");
                }
                catch (Exception ex)
                {
                    ShowMessage($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", MessageType.Error);
                    return;
                } 
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void CloseCom()
        {
            try
            {
                //若串口未开启则返回
                if (!ComConfig.IsOpened)
                {
                    return;
                }
                //停止自动读取
                if (DataMonitorConfig.IsOpened)
                {
                    //CloseAutoRead();
                }

                ComConfig.IsOpened = false;          //标记串口已关闭
                                                     //SerialPort.DataReceived -= ReceiveMessage;
                SerialPort.Close();                   //关闭串口 
                ShowMessage($"关闭串口{SerialPort.PortName}");

                PublishFrameQueue.Clear();      //清空发送帧队列
                ReceiveFrameQueue.Clear();      //清空接收帧队列
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }
        #endregion


        #region 页面消息
        protected void ShowErrorMessage(string message) => ShowMessage(message, Enums.MessageType.Error);
        protected void ShowReceiveMessage(string message, List<MessageSubContent> messageSubContents)
        {
            try
            {
                void action()
                {
                    var msg = new ModbusRtuMessageData("", DateTime.Now, Enums.MessageType.Receive);
                    foreach (var item in messageSubContents)
                    {
                        msg.MessageSubContents.Add(item);
                    }
                    Messages.Add(msg);
                    while (Messages.Count > 500)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
            }
            catch (Exception) { }
        }

        protected void ShowSendMessage(string message, List<MessageSubContent> messageSubContents)
        {
            //ShowMessage(message, MessageType.Send);
            try
            {
                void action()
                {
                    var msg = new ModbusRtuMessageData("", System.DateTime.Now, Enums.MessageType.Send);
                    foreach (var item in messageSubContents)
                    {
                        msg.MessageSubContents.Add(item);
                    }
                    Messages.Add(msg);
                    while (Messages.Count > 260)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void ShowMessage(string message, Enums.MessageType type = Enums.MessageType.Info)
        {
            try
            {
                void action()
                {
                    Messages.Add(new ModbusRtuMessageData($"{message}", DateTime.Now, type));
                    while (Messages.Count > 260)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void ShowMessage(ModbusRtuMessageData msg)
        {
            try
            {
                void action()
                {
                    Messages.Add(msg);
                    while (Messages.Count > 260)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 清空消息
        /// </summary>
        public void MessageClear()
        {
            ReceiveBytesCount = 0;
            SendBytesCount = 0;
            Messages.Clear();
        }
        #endregion
    }
}
