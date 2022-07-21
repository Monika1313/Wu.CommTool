using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using Wu.ComTool.Models;
using System.Management;
using HandyControl.Controls;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;
using Wu.Extensions;

namespace Wu.ComTool.ViewModels
{
    public class ComToolViewModel : BindableBase
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private SerialPort ComDevice = new SerialPort();
        #endregion

        public ComToolViewModel() { }
        public ComToolViewModel(IContainerProvider provider)
        {
            this.provider = provider;
            ExecuteCommand = new(Execute);

            ComDevice.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);

            //更新串口列表
            GetComPorts();
        }



        #region **************************************** 属性 ****************************************
        /// <summary>
        /// 打开抽屉
        /// </summary>
        public IsDrawersOpen IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private IsDrawersOpen _IsDrawersOpen = new();

        /// <summary>
        /// Com口配置
        /// </summary>
        public ComConfig ComConfig { get => _ComConfig; set => SetProperty(ref _ComConfig, value); }
        private ComConfig _ComConfig = new(){ };

        /// <summary>
        /// 串口列表
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> ComPorts { get => _ComPorts; set => SetProperty(ref _ComPorts, value); }
        private ObservableCollection<KeyValuePair<string, string>> _ComPorts = new();

        /// <summary>
        /// 选中的串口
        /// </summary>
        public KeyValuePair<string, string> SelectedCom { get => _SelectedCom; set => SetProperty(ref _SelectedCom, value); }
        private KeyValuePair<string, string> _SelectedCom;

        /// <summary>
        /// 页面消息
        /// </summary>
        public ObservableCollection<MessageData> Messages { get => _Messages; set => SetProperty(ref _Messages, value); }
        private ObservableCollection<MessageData> _Messages = new();

        /// <summary>
        /// 发送的消息
        /// </summary>
        public string SendMessage { get => _SendMessage; set => SetProperty(ref _SendMessage, value); }
        private string _SendMessage = string.Empty;

        /// <summary>
        /// Crc校验模式
        /// </summary>
        public CrcMode CrcMode { get => _CrcMode; set => SetProperty(ref _CrcMode, value); }
        private CrcMode _CrcMode = CrcMode.Modbus;
        #endregion


        #region **************************************** 命令 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": GetDataAsync(); break;
                case "Add": break;
                case "Send": Send(); break;
                case "Clear": Clear(); break;
                case "OpenCom": OperatePort(); break;        //打开串口
                case "ConfigCom": IsDrawersOpen.IsLeftDrawerOpen = true; break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 清空消息
        /// </summary>
        private void Clear()
        {
            Messages.Clear();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private bool Send()
        {
            //TODO 发送数据
            byte[] msg = SendMessage.GetBytes();
            if (ComDevice.IsOpen)
            {
                try
                {
                    List<byte> crc = new();
                    //根据选择进行CRC校验
                    switch (CrcMode)
                    {
                        //无校验
                        case CrcMode.None:
                            break;

                        //Modebus校验
                        case CrcMode.Modbus:
                            var code = Wu.Utils.Crc.Crc16Modbus(msg);
                            Array.Reverse(code);
                            crc.AddRange(code);
                            break;
                        default:
                            break;
                    }

                    //合并数组
                    List<byte> list = new List<byte>();
                    list.AddRange(msg);
                    list.AddRange(crc);
                    var data = list.ToArray();
                    ComDevice.Write(data, 0, data.Length);//发送数据
                    ShowMessage(BitConverter.ToString(data).Replace('-', ' '), MessageType.Send);
                    return true;
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message);
                }
            }
            else
            {
                ShowMessage("串口未打开", MessageType.Error);
            }
            return false;
        }



        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
        {
            //TODO 接收数据
            //数据接收完整性
            //延时读取数据 等待数据接收完成
            Thread.Sleep(100);
            string data = string.Empty;
            //while (ComDevice.BytesToRead > 0)
            //{
            //    data += ComDevice.ReadExisting();  //数据读取,直到读完缓冲区数据
            //    //var XX = ComDevice.ReadByte();
            //}
            //

            int n = ComDevice.BytesToRead;
            byte[] buf = new byte[n];
            ComDevice.Read(buf, 0, n);

            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ShowMessage(BitConverter.ToString(buf).Replace('-', ' '), MessageType.Receive);
            }));
        }


        /// <summary>
        /// 打开串口
        /// </summary>
        private void OperatePort()
        {
            try
            {
                if (ComDevice.IsOpen == false)
                {
                    //配置串口
                    ComDevice.PortName = ComConfig.Port.Key;                     //串口
                    ComDevice.BaudRate = (int)ComConfig.BaudRate;             //波特率
                    ComDevice.Parity = (System.IO.Ports.Parity)ComConfig.Parity;                      //校验
                    ComDevice.DataBits = ComConfig.DataBits;                  //数据位
                    ComDevice.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;                  //停止位
                    try
                    {
                        ComDevice.Open();               //打开串口
                        ComConfig.IsOpened = true;      //标记串口已打开
                        ShowMessage($"开打串口{ComDevice.PortName}");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage(ex.Message, MessageType.Error);
                        return;
                    }
                    IsDrawersOpen.IsLeftDrawerOpen = false;
                }
                else
                {
                    try
                    {
                        ComDevice.Close();                   //关闭串口
                        ComConfig.IsOpened = false;          //标记串口已关闭
                        ShowMessage($"关闭串口{ComDevice.PortName}");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage(ex.Message, MessageType.Error);
                    }
                }

            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
            finally
            {

            }
        }



        /// <summary>
        /// 查询数据
        /// </summary>
        public async void GetDataAsync()
        {
            try
            {

            }
            catch (global::System.Exception ex)
            {

            }
            finally
            {

            }
        }


        /// <summary>
        /// 获取串口完整名字（包括驱动名字）
        /// </summary>
        private void GetComPorts()
        {
            //清空列表
            ComPorts.Clear();
            //查找Com口
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM[0-999]%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        int startIndex = deviceName.IndexOf("(");
                        int endIndex = deviceName.IndexOf(")");
                        string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                        string name = deviceName.Substring(0, startIndex - 1);
                        //添加进列表
                        ComPorts.Add(new KeyValuePair<string, string>(key, name));
                    }
                }
                if(ComPorts.Count != 0)
                {
                    ComConfig.Port = ComPorts[0];
                }
                ShowMessage("获取串口成功");
            }
        }

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        private void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            try
            {
                Messages.Add(new MessageData($"{message}", DateTime.Now, type));
            }
            catch (Exception ex) {}
        }
        #endregion
    }
}
