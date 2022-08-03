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
using Prism.Services.Dialogs;
using Wu.CommTool.Dialogs.Views;
using Wu.CommTool.Views;
using System.Windows;

namespace Wu.CommTool.ViewModels
{
    public class ModbusRtuViewModel : NavigationViewModel
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        private SerialPort SerialPort = new();
        #endregion

        public ModbusRtuViewModel() { }
        public ModbusRtuViewModel(IContainerProvider provider, IDialogHostService dialogHost)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;
            ExecuteCommand = new(Execute);

            SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);

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

        /// <summary>
        /// 暂停界面更新接收的数据
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause = false;

        /// <summary>
        /// ModbusRtuDatas
        /// </summary>
        public ObservableCollection<ModbusRtuData> ModbusRtuDatas { get => _ModbusRtuDatas; set => SetProperty(ref _ModbusRtuDatas, value); }
        private ObservableCollection<ModbusRtuData> _ModbusRtuDatas = new();
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
                case "Pause": Pause(); break;
                case "AreaData": AreaData(); break;                                    //周期读取区域数据
                case "AutoSearch": OpenAutoSearchView(); break;
                case "Send": Send(); break;                                          //发送数据
                case "GetComPorts": GetComPorts(); break;                            //查找Com口
                case "Clear": Clear(); break;                                        //清空信息
                case "OpenCom": OpenCom(); break;                                //打开串口
                case "OpenRightDrawer": IsDrawersOpen.IsRightDrawerOpen=true; break;                                //打开串口
                case "OperatePort": OperatePort(); break;                                //打开串口
                case "CloseCom": CloseCom(); break;                                //关闭串口
                case "ConfigCom": IsDrawersOpen.IsLeftDrawerOpen = true; break;      //打开配置抽屉
                default: break;
            }
        }

        /// <summary>
        /// 周期读取区域数据
        /// </summary>
        private void AreaData()
        {
            ModbusRtuDatas.Clear();
            for (int i = 0; i < 100; i++)
            {
                ModbusRtuDatas.Add(new ModbusRtuData() { Addr = i });
            }
        }

        private void Pause()
        {
            IsPause = !IsPause;
            if (IsPause)
            {
                ShowMessage("暂停更新接收的数据");
            }
            else
            {
                ShowMessage("恢复更新接收的数据");
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        private void OpenCom()
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
                    ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.Port.Value}");
                }
                catch (Exception ex)
                {
                    ShowMessage($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", MessageType.Error);
                    return;
                }
                IsDrawersOpen.IsLeftDrawerOpen = false;        //关闭左侧抽屉
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
                Regex regex = new(@"^[0-9 a-f A-F -]*$");
                if (!regex.IsMatch(SendMessage))
                {
                    ShowMessage("数据字符仅限 0123456789 ABCDEF", MessageType.Error);
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

                if (SerialPort.IsOpen)
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
                        SerialPort.Write(data, 0, data.Length);//发送数据
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
            try
            {
                //TODO 接收数据
                Thread.Sleep(100);       //延时读取数据 等待数据接收完成
                if (!SerialPort.IsOpen)
                {
                    return;
                }
                int n = SerialPort.BytesToRead;          //获取接收的数据总数
                byte[] buf = new byte[n];
                SerialPort.Read(buf, 0, n);        //从第0个读取n个字节, 写入buf
                ReceivBytesCount += buf.Length;          //统计发送的数据总数

                //若暂停更新接收数据 则不显示
                if (IsPause)
                    return;

                ShowMessage(BitConverter.ToString(buf).Replace('-', ' '), MessageType.Receive);

            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowMessage(ex.Message, MessageType.Receive);
                });
            }
        }


        /// <summary>
        /// 打开串口
        /// </summary>
        private void OperatePort()
        {
            try
            {
                if (SerialPort.IsOpen == false)
                {
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
                        ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.Port.Value}");
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
                        SerialPort.Close();                   //关闭串口
                        ComConfig.IsOpened = false;          //标记串口已关闭
                        ShowMessage($"关闭串口{SerialPort.PortName}");
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
        /// 关闭串口
        /// </summary>
        private void CloseCom()
        {
            try
            {
                //若串口未开启则返回
                if (!ComConfig.IsOpened)
                {
                    return;
                }
                SerialPort.Close();                   //关闭串口
                ComConfig.IsOpened = false;          //标记串口已关闭
                ShowMessage($"关闭串口{SerialPort.PortName}");
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
                        //获取名称
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        //从名称中截取串口
                        List<String> dList = new List<String>();
                        foreach (Match mch in Regex.Matches(deviceName, @"COM\d{1,3}"))
                        {
                            String x = mch.Value.Trim();
                            dList.Add(x);
                        }

                        int startIndex = deviceName.IndexOf("(");
                        //int endIndex = deviceName.IndexOf(")");
                        //string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                        string key = dList[0];
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

        protected void ShowErrorMessage(string message, MessageType messageType = MessageType.Error) => ShowMessage(message, messageType);
        protected void ShowReceiveMessage(string message, MessageType messageType = MessageType.Receive) => ShowMessage(message, messageType);
        protected void ShowSendMessage(string message, MessageType messageType = MessageType.Send) => ShowMessage(message, messageType);

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            try
            {
                //判断是UI线程还是子线程 若是子线程需要用委托
                var UiThreadId = Application.Current.Dispatcher.Thread.ManagedThreadId;       //UI线程ID
                var currentThreadId = Environment.CurrentManagedThreadId;                     //当前线程
                //当前线程为主线程 直接更新数据
                if (currentThreadId == UiThreadId)
                {
                    Messages.Add(new MessageData($"{message}", DateTime.Now, type));
                }
                else
                {
                    //子线程无法更新在UI线程的内容   委托主线程更新
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(new MessageData($"{message}", DateTime.Now, type));
                    });
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 自动搜索串口设备
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private async void OpenAutoSearchView()
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
                //添加要传递的参数
                DialogParameters param = new()
                {
                    { nameof(SerialPort), SerialPort },
                    { nameof(ComConfig), ComConfig }
                };
                //弹窗
                var dialogResult2 = await dialogHost.ShowDialog(nameof(ModbusRtuAutoSearchDeviceView), param, nameof(ModbusRtuView));


            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }
        #endregion
    }
}
