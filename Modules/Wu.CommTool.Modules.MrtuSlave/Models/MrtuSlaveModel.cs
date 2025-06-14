using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Parity = Wu.CommTool.Modules.MrtuSlave.Enums.Parity;
using StopBits = Wu.CommTool.Modules.MrtuSlave.Enums.StopBits;

namespace Wu.CommTool.Modules.MrtuSlave.Models;

public partial class MrtuSlaveModel : ObservableObject
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private static readonly ILog log = LogManager.GetLogger(typeof(MrtuSlaveModel));
    private readonly SerialPort SerialPort = new();              //串口


    Task publishHandleTask; //发布消息处理线程
    Task receiveHandleTask; //接收消息处理线程
    readonly EventWaitHandle WaitPublishFrameEnqueue = new AutoResetEvent(false);   //等待发布消息入队
    readonly EventWaitHandle WaitUartReceived = new AutoResetEvent(false);          //接收到串口数据完成标志
    private readonly Queue<(string, int)> PublishFrameQueue = new();           //数据帧发送队列
    private readonly ConcurrentQueue<string> ReceiveFrameQueue = new();     //数据帧处理队列
    #endregion **************************************** 字段 ****************************************



    #region **************************************** 构造函数 ****************************************
    public MrtuSlaveModel()
    {
        Initial();
    }

    public MrtuSlaveModel(IContainerProvider provider) : this()
    {
        this.provider = provider;
    }

    private void Initial()
    {
        Task.Run(() =>
        {
            GetComPorts();
        });

        SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);           //串口接收事件
        //数据帧处理子线程
        publishHandleTask = new Task(PublishFrame);
        receiveHandleTask = new Task(ReceiveFrame);
        publishHandleTask.Start();
        receiveHandleTask.Start();
    }
    #endregion **************************************** 构造函数 ****************************************




    #region **************************************** 属性 ****************************************
    /// <summary>
    /// 寄存器集合
    /// </summary>
    [ObservableProperty] private ModbusRegisterCollection _ModbusRegisterCollection = new();

    /// <summary>
    /// 串口是否开启
    /// </summary>
    [ObservableProperty][property: JsonIgnore] bool isOpened;

    [ObservableProperty] bool isPause;

    /// <summary>
    /// 串口列表
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    ObservableCollection<ComPort> comPorts = [];

    /// <summary>
    /// 选择的Com口 
    /// </summary>
    [ObservableProperty] ComPort comPort = new();

    /// <summary>
    /// 波特率
    /// </summary>
    [ObservableProperty] BaudRate baudRate = BaudRate._9600;

    /// <summary>
    /// 校验位
    /// </summary>
    [ObservableProperty] Parity parity = Parity.None;

    /// <summary>
    /// 数据位
    /// </summary>
    [ObservableProperty] int dataBits = 8;

    /// <summary>
    /// 停止位
    /// </summary>
    [ObservableProperty] StopBits stopBits = StopBits.One;

    /// <summary>
    /// 分包超时时间
    /// </summary>
    [ObservableProperty] int timeOut = 50;

    /// <summary>
    /// 分包最大字节
    /// </summary>
    [ObservableProperty] int maxLength = 500;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty] ModbusByteOrder byteOrder = ModbusByteOrder.DCBA;

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty] bool state;

    /// <summary>
    /// 是否处于发送数据状态
    /// </summary>
    [ObservableProperty][property: JsonIgnore] bool isSending = false;

    /// <summary>
    /// 接收数据中
    /// </summary>
    [ObservableProperty][property: JsonIgnore] bool isReceiving = false;

    /// <summary>
    /// 接收的数据总数
    /// </summary>
    [ObservableProperty][property: JsonIgnore] int receiveBytesCount = 0;

    /// <summary>
    /// 发送的数据总数
    /// </summary>
    [ObservableProperty][property: JsonIgnore] int sendBytesCount = 0;

    /// <summary>
    /// 从站ID
    /// </summary>
    [ObservableProperty] byte slaveId = 1;
    #endregion **************************************** 属性 ****************************************

    [RelayCommand]
    [property: JsonIgnore]
    private void Run()
    {
        try
        {
            //判断串口是否已打开
            if (IsOpened)
            {
                ShowMessage("当前串口已打开, 无法重复开启");
                return;
            }

            //配置串口
            SerialPort.PortName = ComPort.Port;                          //串口
            SerialPort.BaudRate = (int)BaudRate;                         //波特率
            SerialPort.Parity = (System.IO.Ports.Parity)Parity;          //校验
            SerialPort.DataBits = DataBits;                              //数据位
            SerialPort.StopBits = (System.IO.Ports.StopBits)StopBits;    //停止位
            try
            {
                SerialPort.Open();              //打开串口
                IsOpened = true;      //标记串口已打开
                HcGrowlExtensions.Success($"打开虚拟从站 从站ID:{SlaveId} 波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}  {SerialPort.PortName} : {ComPort.DeviceName}", nameof(MrtuSlaveView));
                ShowMessage($"打开虚拟从站 从站ID:{SlaveId} 波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}  {SerialPort.PortName} : {ComPort.DeviceName}");
            }
            catch (Exception ex)
            {
                HcGrowlExtensions.Warning($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", nameof(MrtuSlaveView));
                ShowMessage($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", MessageType.Error);
                return;
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, MessageType.Error);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private async void Stop()
    {
        try
        {
            //若串口未开启则返回
            if (!IsOpened)
            {
                return;
            }

            IsOpened = false;          //标记串口已关闭

            //先清空队列再关闭
            PublishFrameQueue.Clear();      //清空发送帧队列

            //清空接收帧队列
            while (!ReceiveFrameQueue.IsEmpty)
            {
                ReceiveFrameQueue.TryDequeue(out var item);
            }

            if (IsSending)
            {
                await Task.Delay(100);
            }
            ShowMessage($"关闭串口{SerialPort.PortName}");
            SerialPort.Close();                   //关闭串口 

            HcGrowlExtensions.Success($"关闭虚拟从站 从站ID:{SlaveId}", nameof(MrtuSlaveView));

        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, MessageType.Error);
        }
    }



    /// <summary>
    /// 打开串口 若串口未打开则打开串口 若串口已打开则关闭
    /// </summary>



    #region******************************  页面消息  ******************************

    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    ObservableCollection<MessageData> messages = [];

    public void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);

    /// <summary>
    /// 页面展示接收数据消息
    /// </summary>
    /// <param name="frame"></param>
    public void ShowReceiveMessage(ModbusRtuFrame frame)
    {
        try
        {
            void action()
            {
                var msg = new ModbusRtuMessageData("", DateTime.Now, MessageType.Receive, frame);
                Messages.Add(msg);
                log.Info($"接收:{frame}");

                //if (Messages.Count > 9999)
                //{
                //    for (int i = 0; i < 100; i++)
                //    {
                //        Messages.RemoveAt(0);
                //    }
                //}

                while (Messages.Count > 200)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
        }
        catch (Exception) { }
    }

    /// <summary>
    /// 页面展示发送数据消息
    /// </summary>
    /// <param name="frame"></param>
    public void ShowSendMessage(ModbusRtuFrame frame)
    {
        try
        {
            void action()
            {
                var msg = new ModbusRtuMessageData("", DateTime.Now, MessageType.Send, frame);
                Messages.Add(msg);
                log.Info($"发送:{frame}");
                while (Messages.Count > 200)
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
    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
        try
        {
            void action()
            {
                Messages.Add(new MessageData($"{message}", DateTime.Now, type));
                log.Info(message);
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
    [RelayCommand]
    private void MessageClear()
    {
        //ReceiveBytesCount = 0;
        //SendBytesCount = 0;
        Wu.Wpf.Utils.ExecuteFun(Messages.Clear);
    }

    /// <summary>
    /// 暂停更新接收的数据
    /// </summary>
    [RelayCommand]
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
    #endregion


    /// <summary>
    /// 获取串口完整名字（包括驱动名字）
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    public void GetComPorts()
    {
        try
        {
            if (IsOpened) return;

            //缓存最后一次选择的串口
            var lastComPort = ComPort;

            List<ComPort> coms = [];//缓存查找到的串口
            //查找Com口
            using System.Management.ManagementObjectSearcher searcher = new("select * from Win32_PnPEntity where Name like '%(COM[0-999]%'");
            var hardInfos = searcher.Get();
            //获取串口设备列表
            foreach (var hardInfo in hardInfos)
            {
                if (hardInfo.Properties["Name"].Value != null)
                {
                    string deviceName = hardInfo.Properties["Name"].Value.ToString()!;         //获取名称
                    List<string> portList = [];
                    //从名称中截取串口编号
                    foreach (Match mch in Regex.Matches(deviceName, @"COM\d{1,3}").Cast<Match>())
                    {
                        string x = mch.Value.Trim();
                        portList.Add(x);
                    }
                    int startIndex = deviceName.IndexOf("(");
                    string port = portList[0];
                    string name = deviceName[..(startIndex - 1)];
                    coms.Add(new ComPort(port, name));
                }
            }
            ComPorts = new ObservableCollection<ComPort>(coms.OrderBy(x => x.ComId));//串口列表
            if (ComPorts.Count != 0)
            {
                //查找第一个USB设备
                var usbDevice = ComPorts.FindFirst(x => x.DeviceName.ToLower().Contains("usb"));

                //若最后一次选择的是usb设备 则保持当前选择
                var lastUsbDevice = ComPorts.FindFirst(x => x.DeviceName.ToLower().Contains("usb") && x.Port.Equals(lastComPort.Port) && x.DeviceName.Equals(lastComPort.DeviceName));
                if (lastUsbDevice != null)
                {
                    usbDevice = lastUsbDevice;
                }

                //有USB设备则选择第一个USB
                if (usbDevice != null)
                {
                    //默认选中项 若含USB设备则指定第一个USB, 若不含USB则指定第一个
                    ComPort = usbDevice;
                }
                //其次保持不变
                else if (lastComPort != null && ComPorts.Any(x => x.Port == lastComPort.Port && x.DeviceName == lastComPort.DeviceName))//保留原选项
                {
                    ComPort = ComPorts.FirstOrDefault(x => x.Port == lastComPort.Port && x.DeviceName == lastComPort.DeviceName);
                }
                //都没有则选中第一个
                else
                {
                    ComPort = ComPorts[0];
                }
            }
            string str = $"获取串口成功, 共{ComPorts.Count}个。";
            foreach (var item in ComPorts)
            {
                str += $"   {item.Port}: {item.DeviceName};";
            }
            ShowMessage(str);
        }
        catch (Exception ex)
        {
            Growl.Error(ex.Message);
        }
    }


    [RelayCommand]
    private void OpenMrtuSlaveLogView()
    {
        try
        {
            #region 以非模态窗口显示
            var content = provider.Resolve<MrtuSlaveLogView>();//从容器中取出实例

            //验证实例的有效性
            #region 验证实例的有效性
            if (!(content is FrameworkElement dialogContent))
                throw new NullReferenceException("A dialog's content must be a FrameworkElement...");

            if (dialogContent is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutoWireViewModel(view) is null)
                ViewModelLocator.SetAutoWireViewModel(view, true);

            if (!(dialogContent.DataContext is IDialogHostAware viewModel))
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogHostService interface");
            #endregion

            DialogParameters parameters = new()
            {
                { "Value", this }
            };

            var window = new System.Windows.Window()
            {
                Content = dialogContent,
                Name = nameof(MrtuSlaveLogView),
                Width = 700,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Show();// 显示窗口
            if (viewModel is IDialogHostAware aware)
            {
                aware.OnDialogOpened(parameters);
            }
            #endregion

        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }




    #region ****************************** 串口数据处理 ******************************
    /// <summary>
    /// 发送数据
    /// </summary>
    private bool PublishMessage(string message)
    {
        try
        {
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
            catch (Exception)
            {
                ShowErrorMessage($"数据转换16进制失败，发送数据位数量必须为偶数(16进制一个字节2位数)。");
                return false;
            }

            if (SerialPort.IsOpen)
            {
                try
                {
                    if (!IsPause)
                        ShowSendMessage(new ModbusRtuFrame(data));
                    SerialPort.Write(data, 0, data.Length);     //发送数据
                    SendBytesCount += data.Length;                    //统计发送数据总数

                    return true;
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
            else
            {
                ShowErrorMessage("串口未打开,发送失败!");
                Stop();
            }
            return false;
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
            return false;
        }
    }

    public bool IsModbusCrcOk(List<byte> frame)
    {
        return IsModbusCrcOk(frame.ToArray());
    }

    /// <summary>
    /// 返回该数组是否Modbus校验通过
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public bool IsModbusCrcOk(byte[] frame)
    {
        var code = Wu.Utils.Crc.Crc16Modbus(frame);

        //校验通过
        if (code.All(x => x == 0))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 接收串口消息 该方法必须必须必须使用同步不能用异步
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            //若串口未开启则返回
            if (SerialPort == null || !SerialPort.IsOpen)
            {
                SerialPort?.DiscardInBuffer();//丢弃接收缓冲区的数据
                return;
            }
            //标记接收中状态
            IsReceiving = true;

            #region 接收数据
            List<byte> frameCache = []; //接收数据二次缓存 串口接收数据先缓存至此
            List<byte> frame = [];      //接收的数据帧
            bool isNot = false;         //前8字节不是一帧标志 不做标记将导致对响应帧多次重复校验
            string msg = string.Empty;  //
            int times = 0;              //计算次数 连续数ms无数据判断为一帧结束
            //bool isDone = false;        //frameCache处理完成
            do
            {
                //若串口已被关闭则退出
                if (IsOpened == false)
                    return;
                times++;//计时
                //串口接收到新的数据时执行
                if (SerialPort.BytesToRead > 0)
                {
                    times = 0;                                       //重置等待时间
                    int dataCount = SerialPort.BytesToRead;          //获取串口缓存中的数据量
                    byte[] tempBuffer = new byte[dataCount];         //声明数组
                    SerialPort.Read(tempBuffer, 0, dataCount); //从串口缓存读取数据 从第0个读取n个字节, 写入tempBuffer 
                    frameCache.AddRange(tempBuffer);                 //添加进接收的数据缓存列表
                    //isDone = false;                                  //标记frameCache未处理完成
                }
                //二级缓存frameCache中还有未处理完的数据
                if (frameCache.Count > 0)
                {
                    #region 防粘包处理 前8字节为请求帧的处理
                    //由于监控串口网络时,请求帧和应答帧时间间隔较短,会照成接收粘包  通过先截取一段数据分析是否为请求帧,为请求帧则先解析
                    //0X01请求帧8字节 0x02请求帧8字节 0x03请求帧8字节 0x04请求帧8字节 0x05请求帧8字节  0x06请求帧8字节 0x0F请求帧数量不定 0x10请求帧数量不定
                    //由于大部分请求帧长度为8字节 故对接收字节前8字节截取校验判断是否为一帧可以解决大部分粘包问题

                    //当二级缓存大于等于8字节时 对其进行crc校验,验证通过则为一帧
                    if (/*!isNot && */frameCache.Count >= 8)
                    {
                        frame = frameCache.Take(8).ToList();   //截取frameCache前8个字节 对其进行crc校验,验证通过则为一帧
                        var crcOk = IsModbusCrcOk(frame);       //先验证前8字节是否能校验成功

                        #region TODO 这部分未完成
                        //TODO 0x03、0x04、0x10粘包问题已处理 其他功能码的未做
                        //若8字节校验未通过,则可能不是上述描述的请求帧,应根据对应帧的具体内容具体解析

                        byte functionCode = frame[1];//从帧中获取到当前的功能码
                        if (!crcOk)
                        {
                            //0x10请求帧 帧长度需要根据帧的实际情况计算  长度=9+N  从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 字节数(1)  寄存器值(n) 校验码(2)
                            if (frame[1] == 0x10 && frameCache.Count >= (frame[6] + 9))
                            {
                                frame = frameCache.Take(frame[6] + 9).ToList();
                            }
                            else if (frame[1] == 0x10 && frameCache.Count < (frame[6] + 9))
                            {
                                //数据量不够则继续接收 不能用continue,否则无法执行程序最后的延时1ms
                            }

                            //0x03响应帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                            else if (frame[1] == 0x03 && frameCache.Count >= (frame[2] + 5))
                            {
                                frame = frameCache.Take(frame[2] + 5).ToList();
                            }
                            else if (frame[1] == 0x03 && frameCache.Count < (frame[2] + 5))
                            {
                                //数据量不够则继续接收 不能用continue,否则无法执行程序最后的延时1ms
                            }
                            //0x04响应帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                            else if (frame[1] == 0x04 && frameCache.Count >= (frame[2] + 5))
                            {
                                frame = frameCache.Take(frame[2] + 5).ToList();
                            }
                            else if (frame[1] == 0x04 && frameCache.Count < (frame[2] + 5))
                            {
                                //数据量不够则继续接收 不能用continue,否则无法执行程序最后的延时1ms
                            }


                            switch (functionCode)
                            {
                                //功能码20 (0x14)
                                case 0x14:
                                    {
                                        //0x14请求帧 帧长度需要根据帧的实际情况计算 长度=5+7N且(N>=1)   从站ID(1) 功能码(1) 字节数(1) [参考类型(1) 文件号(2) 记录号(2) 记录长度(2)]*N   校验码(2)
                                        //TODO 暂时支持读一条文件记录
                                        if (frameCache.Count >= 12)
                                        {
                                            var tempF = frameCache.Take(12).ToList();//截取一段进行校验
                                            if (IsModbusCrcOk(tempF))
                                            {
                                                frame = tempF;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    break;
                            }

                            //解析出可能的帧并校验成功
                            if (frame.Count > 0 && IsModbusCrcOk(frame))
                            {
                                msg = BitConverter.ToString(frame.ToArray()).Replace('-', ' ');
                                //输出接收到的数据
                                ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队
                                WaitUartReceived.Set();

                                //置位数据接收完成标志
                                frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的字节
                                ReceiveBytesCount += frame.Count;              //计算总接收数据量
                                times = 0;                                     //重置计时器
                                continue;
                            }
                        }
                        #endregion

                        //CRC校验通过
                        if (crcOk)
                        {
                            msg = BitConverter.ToString(frame.ToArray()).Replace('-', ' ');
                            //输出接收到的数据
                            ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队
                            WaitUartReceived.Set();                         //置位数据接收完成标志
                            frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的8字节
                            ReceiveBytesCount += frame.Count;              //计算总接收数据量
                            times = 0;                                     //重置计时器
                        }
                        //验证失败,标记并不再重复校验
                        else
                        {
                            isNot = true;
                        }
                    }
                    #endregion
                }

                //ModbusRtu标准协议 一帧最大长度是256字节
                //限制一次接收的最大数量 避免多设备连接时 导致数据收发无法判断帧结束
                if (frameCache.Count > MaxLength)
                    break;
                Thread.Sleep(1);//同步等待
            } while (times < TimeOut);
            #endregion

            //上部已经处理完分帧了 已经没有数据了
            if (frameCache.Count.Equals(0))
            {
                return;
            }

            msg = BitConverter.ToString(frameCache.ToArray()).Replace('-', ' ');

            ReceiveFrameQueue.Enqueue(msg); //接收到的消息入队
            ReceiveBytesCount += frameCache.Count;         //计算总接收数据量
            WaitUartReceived.Set();         //置位数据接收完成标志
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
        finally
        {
            IsReceiving = false;
        }
    }

    /// <summary>
    /// 发送消息帧入队
    /// </summary>
    /// <param name="msg">发送的消息</param>
    /// <param name="delay">发送完成后等待的时间,期间不会发送消息</param>
    private void PublishFrameEnqueue(string msg, int delay = 10)
    {
        if (string.IsNullOrEmpty(msg))
        {
            return;
        }
        PublishFrameQueue.Enqueue((msg, delay));       //发布消息入队
        WaitPublishFrameEnqueue.Set();                 //置位发布消息入队标志
    }

    /// <summary>
    /// 根据选择 对字符串进行crc校验  
    /// 若已经是校验过的帧 调用该方法将不进行校验
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public string GetCrcedStr(string msg)
    {
        string reMsg = msg.Replace("-", string.Empty).Replace(" ", string.Empty);
        if (reMsg.Length % 2 == 1)
        {
            ShowErrorMessage("发送字符数量不符, 应为2的整数倍");
            return null;
        }
        var msg2 = msg.Replace("-", string.Empty).GetBytes();
        List<byte> crc = new();

        var code = Wu.Utils.Crc.Crc16Modbus(msg2);

        //若未校验过的则校验
        if (!code.All(x => x == 0))
        {
            Array.Reverse(code);
            crc.AddRange(code);
        }

        //合并数组
        List<byte> list = [.. msg2, .. crc];
        var data = BitConverter.ToString(list.ToArray()).Replace("-", "");
        return data;
    }

    /// <summary>
    /// 发送数据帧处理线程
    /// </summary>
    private async void PublishFrame()
    {
        while (true)
        {
            try
            {
                //判断队列是否不空 若为空则等待
                if (PublishFrameQueue.Count == 0)
                {
                    WaitPublishFrameEnqueue.WaitOne();
                    continue;//需要再次验证队列是否为空
                }
                //判断串口是否已打开,若已关闭则不执行
                if (IsOpened)
                {
                    IsSending = true;
                    var frame = PublishFrameQueue.Dequeue();  //出队 数据帧
                    PublishMessage(frame.Item1);              //请求发送数据帧
                    await Task.Delay(frame.Item2);            //等待一段时间
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
            finally
            {
                IsSending = false;
            }
        }
    }

    /// <summary>
    /// 接收数据帧处理线程
    /// </summary>
    private void ReceiveFrame()
    {
        while (true)
        {
            try
            {
                //若无消息需要处理则进入等待
                if (ReceiveFrameQueue.IsEmpty)
                {
                    WaitUartReceived.WaitOne(); //等待接收消息
                }

                //从接收消息队列中取出一条消息
                if (!ReceiveFrameQueue.TryDequeue(out var frame))
                {
                    continue;
                }
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
                else
                {
                    ShowReceiveMessage(mFrame);
                }
                #endregion

                List<byte> frameList = frame.GetBytes().ToList();//将字符串类型的数据帧转换为字节列表
                int slaveId = frameList[0];                 //从站地址
                int func = frameList[1];                    //功能码



            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
    }
    #endregion



    #region XXXX
    private byte[] ProcessRequest(byte[] request)
    {
        // 解析功能码
        byte functionCode = request[1];

        try
        {
            switch (functionCode)
            {
                case 0x01: // 读线圈
                    return ProcessReadCoils(request);
                //case 0x02: // 读离散输入
                //    return ProcessReadDiscreteInputs(request);
                //case 0x03: // 读保持寄存器
                //    return ProcessReadHoldingRegisters(request);
                //case 0x04: // 读输入寄存器
                //    return ProcessReadInputRegisters(request);
                //case 0x05: // 写单个线圈
                //    return ProcessWriteSingleCoil(request);
                //case 0x06: // 写单个保持寄存器
                //    return ProcessWriteSingleRegister(request);
                //case 0x0F: // 写多个线圈
                //    return ProcessWriteMultipleCoils(request);
                //case 0x10: // 写多个保持寄存器
                //    return ProcessWriteMultipleRegisters(request);
                default:
                    return CreateErrorResponse(functionCode, 0x01); // 非法功能
            }
        }
        catch
        {
            return CreateErrorResponse(functionCode, 0x04); // 从站设备故障
        }
    }

    private byte[] ProcessReadCoils(byte[] request)
    {
        ushort startAddress = (ushort)((request[2] << 8) | request[3]);
        ushort quantity = (ushort)((request[4] << 8) | request[5]);

        if (quantity < 1 || quantity > 2000)
            return CreateErrorResponse(0x01, 0x03); // 非法数据值

        List<bool> values = new List<bool>();
        for (ushort i = 0; i < quantity; i++)
        {
            if (ModbusRegisterCollection.TryReadCoil((ushort)(startAddress + i), out bool value))
            {
                values.Add(value);
            }
            else
            {
                return CreateErrorResponse(0x01, 0x02); // 非法数据地址
            }
        }

        byte[] response = new byte[3 + (quantity + 7) / 8];
        response[0] = SlaveId;
        response[1] = 0x01;
        response[2] = (byte)((quantity + 7) / 8);

        for (int i = 0; i < quantity; i++)
        {
            if (values[i])
            {
                response[3 + i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return AppendCrc(response);
    }

    // 实现其他功能码的处理方法...

    private byte[] CreateErrorResponse(byte functionCode, byte exceptionCode)
    {
        byte[] response = new byte[5];
        response[0] = SlaveId;
        response[1] = (byte)(functionCode | 0x80);
        response[2] = exceptionCode;
        return AppendCrc(response);
    }



    private byte[] AppendCrc(byte[] data)
    {
        ushort crc = CalculateCrc(data);
        byte[] result = new byte[data.Length + 2];
        Array.Copy(data, result, data.Length);
        result[data.Length] = (byte)(crc & 0xFF);
        result[data.Length + 1] = (byte)((crc >> 8) & 0xFF);
        return result;
    }

    private ushort CalculateCrc(byte[] data)
    {
        //TODO 逻辑未实现
        return 0; // 实现CRC计算逻辑
    }
    #endregion

}
