using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using Wu.CommTool.Models;
using System.Management;
using HandyControl.Controls;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;
using Wu.Extensions;
using System.Text.RegularExpressions;
using MaterialDesignThemes.Wpf;
using Wu.CommTool.Common;
using Wu.CommTool.Extensions;

namespace Wu.CommTool.ViewModels
{
    public class ComToolViewModel : NavigationViewModel
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        private SerialPort ComDevice = new SerialPort();
        #endregion

        public ComToolViewModel() { }
        public ComToolViewModel(IContainerProvider provider, IDialogHostService dialogHost)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;
            ExecuteCommand = new(Execute);

            ComDevice.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);

            //更新串口列表
            GetComPorts();

            //配置串口
            ComDevice.PortName = ComConfig.Port.Key;                              //串口
            ComDevice.BaudRate = (int)ComConfig.BaudRate;                         //波特率
            ComDevice.Parity = (System.IO.Ports.Parity)ComConfig.Parity;          //校验
            ComDevice.DataBits = ComConfig.DataBits;                              //数据位
            ComDevice.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;    //停止位
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
        private ComConfig _ComConfig = new();

        /// <summary>
        /// 串口列表
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> ComPorts { get => _ComPorts; set => SetProperty(ref _ComPorts, value); }
        private ObservableCollection<KeyValuePair<string, string>> _ComPorts = new();

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

        /// <summary>
        /// 接收的数据总数
        /// </summary>
        public int ReceivBytesCount { get => _ReceiveBytesCount; set => SetProperty(ref _ReceiveBytesCount, value); }
        private int _ReceiveBytesCount = 0;

        /// <summary>
        /// 发送的数据总数
        /// </summary>
        public int SendBytesCount { get => _SendBytesCount; set => SetProperty(ref _SendBytesCount, value); }
        private int _SendBytesCount = 0;
        #endregion


        #region **************************************** 命令 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="obj"></param>
        public void Execute(string obj)
        {
            //TODO 执行命令
            switch (obj)
            {
                case "Search": GetDataAsync(); break;
                case "Add": break;
                case "CloseCom": CloseCom(); break;
                case "AutoSearch":
                    AutoSearch();
                    break;
                case "Test1":
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
                    break;
                case "Send": Send(); break;                                          //发送数据
                case "GetComPorts": GetComPorts(); break;                            //查找Com口
                case "Clear": Clear(); break;                                        //清空信息
                case "OpenCom": OperatePort(); break;                                //打开串口
                case "ConfigCom": IsDrawersOpen.IsLeftDrawerOpen = true; break;      //打开配置抽屉
                default:
                    break;
            }
        }

        /// <summary>
        /// 自动搜索串口设备
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private async void AutoSearch()
        {
            try
            {
                

                //若串口已打开 提示需要关闭串口
                if (ComConfig.IsOpened)
                {
                    //弹窗确认 使用该功能需要先关闭串口
                    var dialogResult = await dialogHost.Question("温馨提示", $"使用自动搜索功能将关闭当前串口, 确认是否关闭 {ComConfig.Port.Key} : {ComConfig.Port.Value}?");
                    //取消
                    if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK)
                        return;
                    //关闭串口
                    CloseCom();
                }

                //打开自动搜索界面
                //将当前串口配置作为参数

            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }

        /// <summary>
        /// 清空消息
        /// </summary>
        private void Clear()
        {
            ReceivBytesCount = 0;
            SendBytesCount = 0;
            Messages.Clear();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private bool Send()
        {
            //TODO 发送数据
            try
            {
                //串口未打开
                if (ComConfig.IsOpened == false)
                {
                    ShowMessage("串口未打开", MessageType.Error);
                    return false;
                }
                //发送数据不为空
                if (SendMessage is null || SendMessage.Length.Equals(0))
                {
                    ShowMessage("发送的数据不能为空", MessageType.Error);
                    return false;
                }
                //验证数据字符必须符合16进制
                Regex regex = new Regex(@"^[0-9 a-e A-E -]*$");
                if (!regex.IsMatch(SendMessage))
                {
                    ShowMessage("数据字符仅限 0123456789 ABCDE", MessageType.Error);
                    return false;
                }

                byte[] msg;
                try
                {
                    msg = SendMessage.Replace("-", string.Empty).GetBytes();
                }
                catch (Exception ex)
                {
                    throw new Exception($"数据转换16进制失败，发送数据位数必须为偶数(16进制一个字节2位数)。");
                }

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
                        SendBytesCount += data.Length;//统计发送数据总数
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
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
                return false;
            }
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
            Thread.Sleep(100);       //延时读取数据 等待数据接收完成
            int n = ComDevice.BytesToRead;          //获取接收的数据总数
            byte[] buf = new byte[n];
            ComDevice.Read(buf, 0, n);        //从第0个读取n个字节, 写入buf
            ReceivBytesCount += buf.Length;         //统计发送的数据总数
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
                    ComDevice.PortName = ComConfig.Port.Key;                              //串口
                    ComDevice.BaudRate = (int)ComConfig.BaudRate;                         //波特率
                    ComDevice.Parity = (System.IO.Ports.Parity)ComConfig.Parity;          //校验
                    ComDevice.DataBits = ComConfig.DataBits;                              //数据位
                    ComDevice.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;    //停止位
                    try
                    {
                        ComDevice.Open();               //打开串口
                        ComConfig.IsOpened = true;      //标记串口已打开
                        ShowMessage($"打开串口 {ComDevice.PortName} : {ComConfig.Port.Value}");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", MessageType.Error);
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


        private void CloseCom()
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
                if (ComPorts.Count != 0)
                {
                    ComConfig.Port = ComPorts[0];
                }
                string str = $"获取串口成功, 共{ComPorts.Count}个。";
                foreach (var item in ComPorts)
                {
                    str += $"   {item.Key}: {item.Value};";
                }
                ShowMessage(str);
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
            catch (Exception ex) { }
        }
        #endregion
    }
}
