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
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using Wu.Extensions;

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    /// <summary>
    /// ModbusRtu共享实例
    /// </summary>
    public class ModbusRtuModel : BindableBase
    {

        public ModbusRtuModel()
        {
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);           //串口接收事件

            //数据帧处理子线程
            publishHandleTask = new Task(PublishFrame);
            receiveHandleTask = new Task(ReceiveFrame);
            publishHandleTask.Start();
            receiveHandleTask.Start();
        }

        private readonly SerialPort SerialPort = new();              //串口

        private readonly Queue<(string, int)> PublishFrameQueue = new();      //数据帧发送队列
        private readonly Queue<string> ReceiveFrameQueue = new();    //数据帧处理队列
        readonly Task publishHandleTask; //发布消息处理线程
        readonly Task receiveHandleTask; //接收消息处理线程
        public EventWaitHandle WaitPublishFrameEnqueue = new AutoResetEvent(true); //等待发布消息入队
        public EventWaitHandle WaitUartReceived = new AutoResetEvent(true); //接收到串口数据完成标志


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

        #region ******************************  自定义帧模块 属性  ******************************
        /// <summary>
        /// 输入消息 用于发送
        /// </summary>
        public string InputMessage { get => _InputMessage; set => SetProperty(ref _InputMessage, value); }
        private string _InputMessage = "01 03 0000 0001";

        /// <summary>
        /// 自动校验模式选择 Crc校验模式
        /// </summary>
        public CrcMode CrcMode { get => _CrcMode; set => SetProperty(ref _CrcMode, value); }
        private CrcMode _CrcMode = CrcMode.Modbus;
        #endregion

        #region 搜索设备模块 属性
        /// <summary>
        /// 搜索设备的状态 0=未开始搜索 1=搜索中 2=搜索结束/搜索中止
        /// </summary>
        public int SearchDeviceState { get => _SearchDeviceState; set => SetProperty(ref _SearchDeviceState, value); }
        private int _SearchDeviceState = 0;

        /// <summary>
        /// 当前搜索的ModbusRtu设备
        /// </summary>
        public ModbusRtuDevice CurrentDevice { get => _CurrentDevice; set => SetProperty(ref _CurrentDevice, value); }
        private ModbusRtuDevice _CurrentDevice = new();

        /// <summary>
        /// 搜索到的ModbusRtu设备
        /// </summary>
        public ObservableCollection<ModbusRtuDevice> ModbusRtuDevices { get => _ModbusRtuDevices; set => SetProperty(ref _ModbusRtuDevices, value); }
        private ObservableCollection<ModbusRtuDevice> _ModbusRtuDevices = new();
        #endregion


        #region ******************************  数据监控模块 属性  ******************************
        /// <summary>
        /// 数据监控配置
        /// </summary>
        public DataMonitorConfig DataMonitorConfig { get => _DataMonitorConfig; set => SetProperty(ref _DataMonitorConfig, value); }
        private DataMonitorConfig _DataMonitorConfig = new();
        #endregion

        #region 自动应答模块 属性
        /// <summary>
        /// 是否开启自动应答
        /// </summary>
        public bool IsAutoResponse { get => _IsAutoResponse; set => SetProperty(ref _IsAutoResponse, value); }
        private bool _IsAutoResponse = false;

        /// <summary>
        /// ModbusRtu自动应答
        /// </summary>
        public ObservableCollection<ModbusRtuAutoResponseData> MosbusRtuAutoResponseDatas { get => _MosbusRtuAutoResponseDatas; set => SetProperty(ref _MosbusRtuAutoResponseDatas, value); }
        private ObservableCollection<ModbusRtuAutoResponseData> _MosbusRtuAutoResponseDatas = new();

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
                else if (ComPorts.Any(x=>x.Key==ComConfig.Port.Key && x.Value == ComConfig.Port.Value))//保留原选项
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
                    CloseAutoRead();
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


        #region******************************  页面消息  ******************************
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

        /// <summary>
        /// 暂停更新接收的数据
        /// </summary>
        public void Pause()
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
        #endregion

        #region ****************************** 串口数据处理 ******************************
        /// <summary>
        /// 发送数据
        /// </summary>
        private bool PublishMessage(string message)
        {
            try
            {
                //串口未打开 打开串口
                if (ComConfig.IsOpened == false)
                {
                    ShowErrorMessage("串口未打开");
                    ShowMessage("尝试打开串口...");
                    OpenCom();
                }

                //发送数据不能为空
                if (message is null || message.Length.Equals(0))
                {
                    ShowErrorMessage("发送的数据不能为空");
                    return false;
                }

                //验证数据字符必须符合16进制
                Regex regex = new(@"^[0-9 a-f A-F -]*$");
                if (!regex.IsMatch(message))
                {
                    ShowErrorMessage("数据字符仅限 0123456789 ABCDEF");
                    return false;
                }

                byte[] data;
                try
                {
                    data = message.Replace("-", string.Empty).GetBytes();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"数据转换16进制失败，发送数据位数量必须为偶数(16进制一个字节2位数)。");
                    return false;
                }

                if (SerialPort.IsOpen)
                {
                    try
                    {
                        SerialPort.Write(data, 0, data.Length);     //发送数据
                        SendBytesCount += data.Length;                    //统计发送数据总数

                        if (!IsPause)
                            ShowSendMessage("", new ModbusRtuFrame(data).GetmessageWithErrMsg());
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                }
                else
                {
                    ShowErrorMessage("串口未打开");
                }
                return false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
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
                //若串口未开启则返回
                if (!SerialPort.IsOpen)
                {
                    SerialPort?.DiscardInBuffer();//丢弃接收缓冲区的数据
                    return;
                }

                ComConfig.IsReceiving = true;

                //System.Diagnostics.Stopwatch oTime = new();   //定义一个计时对象  
                //oTime.Start();                         //开始计时 

                #region 接收数据
                //接收的数据缓存
                List<byte> list = new();
                if (ComConfig.IsOpened == false)
                    return;
                string msg = string.Empty;
                int times = 0;//计算次数 连续数ms无数据判断为一帧结束
                do
                {
                    if (ComConfig.IsOpened && SerialPort.BytesToRead > 0)
                    {
                        times = 0;
                        int dataCount = SerialPort.BytesToRead;          //获取数据量
                        byte[] tempBuffer = new byte[dataCount];         //声明数组
                        SerialPort.Read(tempBuffer, 0, dataCount); //从第0个读取n个字节, 写入tempBuffer 
                        list.AddRange(tempBuffer);                       //添加进接收的数据列表
                        msg += BitConverter.ToString(tempBuffer);
                        //限制一次接收的最大数量 避免多设备连接时 导致数据收发无法判断帧结束
                        if (list.Count > ComConfig.MaxLength)
                            break;
                    }
                    else
                    {
                        times++;
                        Thread.Sleep(1);
                    }
                } while (times < ComConfig.TimeOut);
                #endregion


                msg = msg.Replace('-', ' ');
                ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队

                //搜索时将验证通过的添加至搜索到的设备列表
                if (SearchDeviceState == 1)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        CurrentDevice.ReceiveMessage = msg;
                        CurrentDevice.Address = int.Parse(msg[..2], System.Globalization.NumberStyles.HexNumber);
                        ModbusRtuDevices.Add(CurrentDevice);
                    }));
                    //HcGrowlExtensions.Success($"搜索到设备 {CurrentDevice.Address}...", viewName);
                }

                ReceiveBytesCount += list.Count;         //计算总接收数据量
                //若暂停更新接收数据 则不显示
                if (IsPause)
                    return;
                WaitUartReceived.Set();//置位数据接收完成标志
                //oTime.Stop();
                //ShowMessage($"接收数据用时{oTime.Elapsed.TotalMilliseconds} ms");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Receive);
            }
            finally
            {
                ComConfig.IsReceiving = false;
            }
        }

        /// <summary>
        /// 发送消息帧入队
        /// </summary>
        /// <param name="msg">发送的消息</param>
        /// <param name="delay">发送完成后等待的时间,期间不会发送消息</param>
        private void PublishFrameEnqueue(string msg, int delay = 10)
        {
            if (msg == null)
            {
                return;
            }
            PublishFrameQueue.Enqueue((msg, delay));       //发布消息入队
            WaitPublishFrameEnqueue.Set();                 //置位发布消息入队标志
        }

        /// <summary>
        /// 根据选择 对字符串进行crc校验
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string GetCrcedStrWithSelect(string msg)
        {
            string reMsg = msg.Replace("-", string.Empty).Replace(" ", string.Empty);
            if (reMsg.Length % 2 == 1)
            {
                ShowErrorMessage("发送字符数量不符, 应为2的整数倍");
                return null;
            }
            var msg2 = msg.Replace("-", string.Empty).GetBytes();
            List<byte> crc = new();
            //根据选择进行CRC校验
            switch (CrcMode)
            {
                //无校验
                case CrcMode.None:
                    break;

                //Modebus校验
                case CrcMode.Modbus:
                    var code = Wu.Utils.Crc.Crc16Modbus(msg2);
                    Array.Reverse(code);
                    crc.AddRange(code);
                    break;
                default:
                    break;
            }
            //合并数组
            List<byte> list = new List<byte>();
            list.AddRange(msg2);
            list.AddRange(crc);
            var data = BitConverter.ToString(list.ToArray()).Replace("-", "");
            return data;
        }

        /// <summary>
        /// 发送数据帧处理线程
        /// </summary>
        private async void PublishFrame()
        {
            WaitPublishFrameEnqueue.Reset();
            while (true)
            {
                //System.Diagnostics.Stopwatch oTime = new System.Diagnostics.Stopwatch();   //定义一个计时对象  
                //oTime.Start();                         //开始计时 
                try
                {
                    //判断队列是否不空 若为空则等待
                    if (PublishFrameQueue.Count == 0)
                    {
                        WaitPublishFrameEnqueue.WaitOne();
                        continue;//需要再次验证队列是否为空
                    }
                    ComConfig.IsSending = true;
                    var frame = PublishFrameQueue.Dequeue();  //出队 数据帧
                    PublishMessage(frame.Item1);              //请求发送数据帧
                    await Task.Delay(frame.Item2);            //等待一段时间
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                finally
                {
                    ComConfig.IsSending = false;
                }
                //oTime.Stop();                          //结束计时
                //ShowMessage($"{oTime.ElapsedMilliseconds} ms");
            }
        }

        /// <summary>
        /// 接收数据帧处理线程
        /// </summary>
        private void ReceiveFrame()
        {
            WaitUartReceived.Reset();
            while (true)
            {
                try
                {
                    //若无消息需要处理则进入等待
                    if (ReceiveFrameQueue.Count == 0)
                    {
                        WaitUartReceived.WaitOne(); //等待接收消息
                    }

                    //从接收消息队列中取出一条消息
                    var frame = ReceiveFrameQueue.Dequeue();
                    if (string.IsNullOrWhiteSpace(frame))
                    {
                        continue;
                    }
                    //实例化ModbusRtu帧
                    var mFrame = new ModbusRtuFrame(frame.GetBytes());

                    //对接收的消息直接进行crc校验
                    var crc = Wu.Utils.Crc.Crc16Modbus(frame.GetBytes());   //校验码 校验通过的为0000

                    #region 界面输出接收的消息 若校验成功则根据接收到内容输出不同的格式
                    if (IsPause)
                    {
                        //若暂停更新显示则不输出
                    }
                    else if (mFrame.Type.Equals(ModbusRtuFrameType.校验失败))
                    {
                        ShowReceiveMessage(mFrame.ToString(), mFrame.GetmessageWithErrMsg());
                        //todo 临时添加 自由口协议自动应答, 该协议不是modbus 校验不会通过
                        #region 自动应答
                        if (IsAutoResponse)
                        {
                            //验证匹配哪一条规则
                            var xx = MosbusRtuAutoResponseDatas.FindFirst(x => x.MateTemplate.ToLower().Replace(" ", "").Equals(frame.ToLower().Replace(" ", "")));
                            if (xx != null)
                            {
                                ShowMessage($"自动应答匹配: {xx.Name}");
                                PublishFrameEnqueue(xx.ResponseTemplate);      //自动应答
                            }
                        }
                        #endregion
                        continue;
                    }
                    //校验成功
                    else
                    {
                        ShowReceiveMessage(mFrame.ToString(), mFrame.GetmessageWithErrMsg());
                    }
                    #endregion


                    #region 自动应答
                    if (IsAutoResponse)
                    {
                        //验证匹配哪一条规则
                        var xx = MosbusRtuAutoResponseDatas.FindFirst(x => x.MateTemplate.ToLower().Replace(" ", "").Equals(frame.ToLower().Replace(" ", "")));
                        if (xx != null)
                        {
                            ShowMessage($"自动应答匹配: {xx.Name}");
                            PublishFrameEnqueue(xx.ResponseTemplate);      //自动应答
                        }
                    }
                    #endregion

                    List<byte> frameList = frame.GetBytes().ToList();//将字符串类型的数据帧转换为字节列表
                    int slaveId = frameList[0];                 //从站地址
                    int func = frameList[1];                    //功能码

                    #region 对接收的数据分功能码展示

                    //03功能码
                    if (mFrame.Type.Equals(ModbusRtuFrameType._0x03响应帧))
                    {
                        //若自动读取开启则解析接收的数据
                        if (DataMonitorConfig.IsOpened)
                        {
                            //验证数据是否为请求的数据 根据 从站地址 功能码 数据字节数量
                            if (frameList[0] == DataMonitorConfig.SlaveId && frameList[2] == DataMonitorConfig.Quantity * 2)
                            {
                                Analyse(frameList);
                            }
                        }
                    }

                    //0x10功能码
                    else if (mFrame.Type.Equals(ModbusRtuFrameType._0x10响应帧) && DataMonitorConfig.IsOpened)
                    {
                        ShowMessage("数据写入成功");
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// 解析接收的数据
        /// </summary>
        private void Analyse(List<byte> list)
        {
            //TODO 解析数据
            //目前仅支持03功能码

            //判断字节数为奇数还是偶数
            //偶数为主站请求
            if (list.Count % 2 == 0)
                return;
            //奇数为响应

            //验证数据是否为请求的数据 根据 从站地址 功能码 数据字节数量
            if (list[0] != DataMonitorConfig.SlaveId || list[1] != DataMonitorConfig.Function || list[2] != DataMonitorConfig.Quantity * 2)
                return;//非请求的数据

            var byteArr = list.ToArray();
            //将读取的数据写入
            for (int i = 0; i < DataMonitorConfig.Quantity; i++)
            {
                DataMonitorConfig.ModbusRtuDatas[i].Location = i * 2 + 3;         //在源字节数组中的起始位置 源字节数组为完整的数据帧,帧头部分3字节 每个值为1个word2字节
                DataMonitorConfig.ModbusRtuDatas[i].OriginValue = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(byteArr, 3 + 2 * i);
                DataMonitorConfig.ModbusRtuDatas[i].OriginBytes = byteArr;        //源字节数组
                DataMonitorConfig.ModbusRtuDatas[i].ModbusByteOrder = DataMonitorConfig.ByteOrder; //字节序
                DataMonitorConfig.ModbusRtuDatas[i].UpdateTime = DateTime.Now;    //更新时间
            }
        }
        #endregion



        #region ******************************  自定义帧模块 方法  ******************************
        /// <summary>
        /// 发送自定义帧
        /// </summary>
        public void SendCustomFrame()
        {
            //若串口未打开则打开串口
            if (!ComConfig.IsOpened)
            {
                ShowMessage("串口未打开, 尝试打开串口...");
                OpenCom();
                if (!ComConfig.IsOpened)
                {
                    return;
                }
            }

            try
            {
                //var msg = SendMessage.Replace("-", string.Empty).GetBytes();
                //List<byte> crc = new();
                ////根据选择进行CRC校验
                //switch (CrcMode)
                //{
                //    //无校验
                //    case CrcMode.None:
                //        break;

                //    //Modebus校验
                //    case CrcMode.Modbus:
                //        var code = Wu.Utils.Crc.Crc16Modbus(msg);
                //        Array.Reverse(code);
                //        crc.AddRange(code);
                //        break;
                //    default:
                //        break;
                //}
                ////合并数组
                //List<byte> list = new List<byte>();
                //list.AddRange(msg);
                //list.AddRange(crc);
                //var data = BitConverter.ToString(list.ToArray()).Replace("-", "");

                PublishFrameEnqueue(GetCrcedStrWithSelect(InputMessage));                  //将待发送的消息添加进队列
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
        #endregion


        #region ******************************  数据监控模块 方法  ******************************
        /// <summary>
        /// 关闭自动读取数据
        /// </summary>
        private void CloseAutoRead()
        {
            //try
            //{
            //    timer.Stop();
            //    DataMonitorConfig.IsOpened = false;
            //    CloseCom();
            //    ShowMessage("关闭自动读取数据...");
            //}
            //catch (Exception ex)
            //{
            //    ShowErrorMessage(ex.Message);
            //}
        }
        #endregion
    }
}
