using HandyControl.Controls;
using MaterialDesignThemes.Wpf;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using Wu.CommTool.Modules.ModbusRtu.Utils;

namespace Wu.CommTool.Modules.ModbusRtu.Models;

public partial class UartModel : ObservableObject
{
    public UartModel()
    {
        SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);           //串口接收事件
        //数据帧处理子线程
        publishHandleTask = new Task(PublishFrame);
        receiveHandleTask = new Task(ReceiveFrame);
        publishHandleTask.Start();
        receiveHandleTask.Start();


        CustomFrames = [new UartCustomFrame  (this,"01 03 0000 0001 "),
                                                    new (this,"01 04 0000 0001 "),
                                                    new (this,""),];

    }

    private readonly SerialPort SerialPort = new();              //串口
    private readonly Queue<(string, int)> PublishFrameQueue = new();      //数据帧发送队列
    private readonly ConcurrentQueue<string> ReceiveFrameQueue = new();    //数据帧处理队列
    readonly Task publishHandleTask; //发布消息处理线程
    readonly Task receiveHandleTask; //接收消息处理线程
    readonly EventWaitHandle WaitPublishFrameEnqueue = new AutoResetEvent(false); //等待发布消息入队
    readonly EventWaitHandle WaitUartReceived = new AutoResetEvent(false); //接收到串口数据完成标志
    protected System.Timers.Timer timer = new();                 //定时器 定时读取数据
    private readonly string ModbusRtuConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig");                           //ModbusRtu配置文件路径
    public readonly string ModbusRtuAutoResponseConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuAutoResponseConfig");   //ModbusRtu自动应答配置文件路径
    private static readonly ILog log = LogManager.GetLogger(typeof(ModbusRtuModel));

    #region 属性
    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty] ObservableCollection<MessageData> messages = [];

    /// <summary>
    /// 暂停
    /// </summary>
    [ObservableProperty] bool isPause;

    /// <summary>
    /// 串口列表
    /// </summary>
    [ObservableProperty] ObservableCollection<ComPort> comPorts = [];

    /// <summary>
    /// Com口配置
    /// </summary>
    [ObservableProperty] ComConfig comConfig = new();

    /// <summary>
    /// 接收的数据总数
    /// </summary>
    [ObservableProperty] int receiveBytesCount = 0;

    /// <summary>
    /// 发送的数据总数
    /// </summary>
    [ObservableProperty] int sendBytesCount = 0;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty] ModbusByteOrder byteOrder = ModbusByteOrder.DCBA;

    /// <summary>
    /// 串口接收数据格式
    /// </summary>
    [ObservableProperty] UartDataFormat receiveDataFormat = UartDataFormat.Hex;

    /// <summary>
    /// 串口发送数据格式
    /// </summary>
    [ObservableProperty] UartDataFormat sendDataFormat = UartDataFormat.Hex;
    #endregion



    #region ******************************  自定义帧模块 属性  ******************************
    /// <summary>
    /// 输入消息 用于发送
    /// </summary>
    [ObservableProperty] private string inputMessage = "01 03 0000 0001";

    /// <summary>
    /// 自动校验模式选择 Crc校验模式
    /// </summary>
    [ObservableProperty] private CrcMode crcMode = CrcMode.None;

    /// <summary>
    /// 自定义帧的输入框
    /// </summary>
    [ObservableProperty] private ObservableCollection<UartCustomFrame> customFrames;
    #endregion


    #region 串口操作方法
    /// <summary>
    /// 获取串口完整名字（包括驱动名字）
    /// </summary>
    [RelayCommand]
    public void GetComPorts()
    {
        try
        {
            if (ComConfig.IsOpened)
            {
                return;
            }

            //缓存最后一次选择的串口
            var lastComPort = ComConfig.ComPort;

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
                    ComConfig.ComPort = usbDevice;
                }
                //其次保持不变
                else if (lastComPort != null && ComPorts.Any(x => x.Port == lastComPort.Port && x.DeviceName == lastComPort.DeviceName))//保留原选项
                {
                    ComConfig.ComPort = ComPorts.FirstOrDefault(x => x.Port == lastComPort.Port && x.DeviceName == lastComPort.DeviceName);
                }
                //都没有则选中第一个
                else
                {
                    ComConfig.ComPort = ComPorts[0];
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

    /// <summary>
    /// 打开串口
    /// </summary>
    [RelayCommand]
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
            SerialPort.PortName = ComConfig.ComPort.Port;                          //串口
            SerialPort.BaudRate = (int)ComConfig.BaudRate;                         //波特率
            SerialPort.Parity = (System.IO.Ports.Parity)ComConfig.Parity;          //校验
            SerialPort.DataBits = ComConfig.DataBits;                              //数据位
            SerialPort.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;    //停止位
            try
            {
                SerialPort.Open();              //打开串口
                ComConfig.IsOpened = true;      //标记串口已打开
                ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.ComPort.DeviceName}  波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}");
            }
            catch (Exception ex)
            {
                HcGrowlExtensions.Warning($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", nameof(ModbusRtuView));
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
    public async void CloseCom()
    {
        try
        {
            //若串口未开启则返回
            if (!ComConfig.IsOpened)
            {
                return;
            }

            ComConfig.IsOpened = false;          //标记串口已关闭
            //先清空队列再关闭
            PublishFrameQueue.Clear();      //清空发送帧队列

            //清空接收帧队列
            while (!ReceiveFrameQueue.IsEmpty)
            {
                ReceiveFrameQueue.TryDequeue(out var item);
            }

            if (ComConfig.IsSending)
            {
                await Task.Delay(100);
            }
            ShowMessage($"关闭串口{SerialPort.PortName}");
            SerialPort.Close();                   //关闭串口 
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, MessageType.Error);
        }
    }

    /// <summary>
    /// 打开串口 若串口未打开则打开串口 若串口已打开则关闭
    /// </summary>
    private void OperatePort()
    {
        try
        {
            if (SerialPort.IsOpen == false)
            {
                //配置串口
                SerialPort.PortName = ComConfig.ComPort.Port;                              //串口
                SerialPort.BaudRate = (int)ComConfig.BaudRate;                         //波特率
                SerialPort.Parity = (System.IO.Ports.Parity)ComConfig.Parity;          //校验
                SerialPort.DataBits = ComConfig.DataBits;                              //数据位
                SerialPort.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;    //停止位
                try
                {
                    SerialPort.Open();               //打开串口
                    ComConfig.IsOpened = true;      //标记串口已打开
                    ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.ComPort.DeviceName}");
                }
                catch (Exception ex)
                {
                    HcGrowlExtensions.Warning($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", nameof(ModbusRtuView));
                    ShowMessage($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", MessageType.Error);
                    return;
                }
            }
            else
            {
                try
                {
                    CloseCom();
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
    /// 根据选择 对字符串进行crc校验
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public string GetModbusCrcedStr(string msg)
    {
        var msg2 = msg.Replace("-", string.Empty).GetBytes();
        List<byte> crc = [];
        //根据选择ModbusCRC校验
        var code = Wu.Utils.Crc.Crc16Modbus(msg2);
        Array.Reverse(code);
        crc.AddRange(code);
        //合并数组
        List<byte> list = new();
        list.AddRange(msg2);
        list.AddRange(crc);
        var data = BitConverter.ToString(list.ToArray()).Replace("-", "");
        return data;
    }
    #endregion


    #region******************************  页面消息  ******************************
    public void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);

    protected void ShowReceiveMessage(string message)
    {
        try
        {
            void action()
            {
                Messages.Add(new MessageData($"{message}", DateTime.Now, MessageType.Receive));
                log.Info($"接收:{message}");
                while (Messages.Count > 100)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFun(action);
        }
        catch (Exception)
        {
        }
    }

    protected void ShowSendMessage(string message)
    {
        try
        {
            void action()
            {
                Messages.Add(new MessageData($"{message}", DateTime.Now, MessageType.Send));
                log.Info($"发送:{message}");
                while (Messages.Count > 100)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFun(action);
        }
        catch (Exception)
        {
        }
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
        ReceiveBytesCount = 0;
        SendBytesCount = 0;
        Messages.Clear();
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


            if (SendDataFormat == UartDataFormat.Ascii)
            {
                if (SerialPort.IsOpen)
                {
                    try
                    {
                        if (!IsPause)
                            ShowSendMessage(message);

                        byte[] sendBytes = Encoding.ASCII.GetBytes(message.RemoveSpace()); //将字符串转换为字节数组
                        SerialPort.Write(sendBytes, 0, sendBytes.Length);     //发送数据
                        SendBytesCount += sendBytes.Length;                    //统计发送数据总数

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
                    CloseCom();
                }
                return false;
            }

            //若是Hex则执行以下内容

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
                        ShowSendMessage(message.RemoveSpace().InsertFormat(2, " "));

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
                CloseCom();
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
            ComConfig.IsReceiving = true;

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
                if (ComConfig.IsOpened == false)
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
                    #region 根据功能码调整帧至正确的起始位置(由于数据中可能存在类似功能码的数据, 可能会有错误)
                    if (ComConfig.AutoFrame == Enable.启用 && frameCache.Count >= 8 && (times > 1))
                    {
                        //TODO 根据接收数据中功能码位置调整帧至正确的起始位置
                        //获取缓存中所有的功能码位置
                        var funcs = ModbusUtils.GetIndicesOfFunctions(frameCache);
                        //接收缓存至少2字节,且功能码至少1个
                        //将功能码调整至第二字节的位置
                        if (frameCache.Count >= 2 && funcs.Count > 0)
                        {
                            //若前2个功能码是连续的, 则第一个功能码应判定为地址
                            if (funcs.Count >= 2                         //有多个功能码
                                && (funcs[1] - funcs[0] == 1) //前两个功能码是连续的
                                && funcs[0] != 0)                   //第一字节不是地址
                            {
                                frame = frameCache.Take(funcs[0]).ToList();//将这一帧前面的输出
                                msg = BitConverter.ToString(frame.ToArray()).Replace('-', ' ');
                                //输出接收到的数据
                                ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队
                                WaitUartReceived.Set();
                                frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的字节
                                ReceiveBytesCount += frame.Count;              //计算总接收数据量
                                isNot = false;
                                continue;
                            }
                            //前2字节都没有功能码,则将功能码调整至第二字节
                            else if (funcs[0] > 2)
                            {
                                frame = frameCache.Take(funcs[0] - 1).ToList();//功能码前一个字节为地址要保留,所以要-1
                                msg = BitConverter.ToString(frame.ToArray()).Replace('-', ' ');
                                //输出接收到的数据
                                ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队
                                WaitUartReceived.Set();
                                frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的字节
                                ReceiveBytesCount += frame.Count;              //计算总接收数据量
                                isNot = false;
                                continue;
                            }
                        }
                    }
                    #endregion

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
                if (frameCache.Count > ComConfig.MaxLength)
                    break;
                Thread.Sleep(1);//同步等待
            } while (times < ComConfig.TimeOut);
            #endregion

            //上部已经处理完分帧了 已经没有数据了
            if (frameCache.Count.Equals(0))
            {
                return;
            }

            msg = UartUtils.Bytes2String(frameCache);

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
    public string GetCrcedStrWithSelect(string msg)
    {
        if (CrcMode == CrcMode.None)
        {
            return msg;
        }
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
                //若已经是校验过的则不校验
                if (code.All(x => x == 0))
                {
                    break;
                }
                Array.Reverse(code);
                crc.AddRange(code);
                break;
            default:
                break;
        }
        //合并数组
        List<byte> list = new();
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
                if (ComConfig.IsOpened)
                {
                    ComConfig.IsSending = true;
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
                ComConfig.IsSending = false;
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

                var receiveStr = frame.RemoveSpace().GetBytes();

                #region 界面输出接收的消息 若校验成功则根据接收到内容输出不同的格式
                if (!IsPause)
                {
                    //根据编码显示不同的内容
                    switch (ReceiveDataFormat)
                    {
                        case UartDataFormat.Ascii:
                            ShowReceiveMessage(System.Text.Encoding.ASCII.GetString(receiveStr));
                            break;
                        case UartDataFormat.Hex:
                        default:
                            ShowReceiveMessage(frame.InsertFormat(2, " "));
                            //ShowReceiveMessage(frame);
                            break;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
    }

    #endregion


    #region ******************************  自定义帧模块 方法  ******************************
    /// <summary>
    /// 发送自定义帧
    /// </summary>
    [RelayCommand]
    public void SendCustomFrame(UartCustomFrame customFrame)
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

        if (string.IsNullOrWhiteSpace(customFrame.Frame))
        {
            ShowErrorMessage("发送消息不能为空...");
            return;
        }

        try
        {
            PublishFrameEnqueue(GetCrcedStrWithSelect(customFrame.Frame));                  //将待发送的消息添加进队列
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 发送自定义帧
    /// </summary>
    public void SendCustomFrame(CustomFrame customFrame)
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

        if (string.IsNullOrWhiteSpace(customFrame.Frame))
        {
            ShowErrorMessage("发送消息不能为空...");
            return;
        }

        try
        {
            PublishFrameEnqueue(GetCrcedStrWithSelect(customFrame.Frame));                  //将待发送的消息添加进队列
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 自定义帧 新增行
    /// </summary>
    [RelayCommand]
    public void AddNewCustomFrame()
    {
        CustomFrames.Add(new UartCustomFrame(this, ""));
    }

    /// <summary>
    /// 自定义帧 删除行
    /// </summary>
    /// <param name="frame"></param>
    [RelayCommand]
    private void DeleteCustomFrame(UartCustomFrame frame)
    {
        if (CustomFrames.Count > 1)
        {
            CustomFrames.Remove(frame);
        }
        else
        {
            ShowErrorMessage("不能删除最后一行...");
        }
    }
    #endregion

    #region ******************************  自动应答 方法  ******************************
    /// <summary>
    /// 导出自动应答配置文件
    /// </summary>
    public void ExportAutoResponseConfig()
    {
        //try
        //{
        //    //配置文件目录
        //    string dict = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuAutoResponseConfig");
        //    Wu.Utils.IoUtil.Exists(dict);
        //    Microsoft.Win32.SaveFileDialog sfd = new()
        //    {
        //        Title = "请选择导出配置文件...",                                              //对话框标题
        //        Filter = "json files(*.jsonARC)|*.jsonARC",    //文件格式过滤器
        //        FilterIndex = 1,                                                         //默认选中的过滤器
        //        FileName = "Default",                                           //默认文件名
        //        DefaultExt = "jsonARC",                                     //默认扩展名
        //        InitialDirectory = dict,                //指定初始的目录
        //        OverwritePrompt = true,                                                  //文件已存在警告
        //        AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
        //    };
        //    if (sfd.ShowDialog() != true)
        //        return;
        //    //将当前的配置序列化为json字符串
        //    var content = JsonConvert.SerializeObject(MosbusRtuAutoResponseDatas);
        //    //保存文件
        //    Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
        //    HcGrowlExtensions.Success($"自动应答配置\"{Path.GetFileNameWithoutExtension(sfd.FileName)}\"导出成功", ModbusRtuView.ViewName);
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning($"自动应答配置导出失败", ModbusRtuView.ViewName);
        //    ShowErrorMessage(ex.Message);
        //}
    }

    /// <summary>
    /// 导入自动应答配置文件
    /// </summary>
    public void ImportAutoResponseConfig()
    {
        //try
        //{
        //    //配置文件目录
        //    string dict = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuAutoResponseConfig");
        //    Wu.Utils.IoUtil.Exists(dict);
        //    //选中配置文件
        //    OpenFileDialog dlg = new()
        //    {
        //        Title = "请选择导入自动应答配置文件...",                                              //对话框标题
        //        Filter = "json files(*.jsonARC)|*.jsonARC",    //文件格式过滤器
        //        FilterIndex = 1,                                                         //默认选中的过滤器
        //        InitialDirectory = dict
        //    };

        //    if (dlg.ShowDialog() != true)
        //        return;
        //    var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
        //    MosbusRtuAutoResponseDatas = JsonConvert.DeserializeObject<ObservableCollection<ModbusRtuAutoResponseData>>(xx)!;
        //    HcGrowlExtensions.Success($"自动应答配置\"{Path.GetFileNameWithoutExtension(dlg.FileName)}\"导出成功", ModbusRtuView.ViewName);
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning($"自动应答配置导入成功", ModbusRtuView.ViewName);
        //    ShowErrorMessage(ex.Message);
        //}
    }
    #endregion
}
