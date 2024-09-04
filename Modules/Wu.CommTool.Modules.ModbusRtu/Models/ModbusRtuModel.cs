using System.Collections.Concurrent;
using System.IO.Ports;
namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu共享实例
/// </summary>
public partial class ModbusRtuModel : ObservableObject
{
    public ModbusRtuModel()
    {
        SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);           //串口接收事件
        //数据帧处理子线程
        publishHandleTask = new Task(PublishFrame);
        receiveHandleTask = new Task(ReceiveFrame);
        publishHandleTask.Start();
        receiveHandleTask.Start();

        //默认选中9600无校验
        SelectedBaudRates.Add(BaudRate._9600);
        SelectedParitys.Add(Parity.None);

        //初始化一个10个数据的列表
        DataMonitorConfig.ModbusRtuDatas.AddRange(Enumerable.Range(0, 10).Select(i => new ModbusRtuData()));
        RefreshModbusRtuDataDataView();
    }

    private readonly SerialPort SerialPort = new();              //串口
    private readonly Queue<(string, int)> PublishFrameQueue = new();      //数据帧发送队列
    private readonly ConcurrentQueue<string> ReceiveFrameQueue = new();    //数据帧处理队列
    readonly Task publishHandleTask; //发布消息处理线程
    readonly Task receiveHandleTask; //接收消息处理线程
    readonly EventWaitHandle WaitPublishFrameEnqueue = new AutoResetEvent(true); //等待发布消息入队
    readonly EventWaitHandle WaitUartReceived = new AutoResetEvent(true); //接收到串口数据完成标志
    protected System.Timers.Timer timer = new();                 //定时器 定时读取数据
    private readonly string ModbusRtuConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig");                           //ModbusRtu配置文件路径
    public readonly string ModbusRtuAutoResponseConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuAutoResponseConfig");   //ModbusRtu自动应答配置文件路径
    private static readonly ILog log = LogManager.GetLogger(typeof(ModbusRtuModel));

    #region 属性
    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MessageData> messages = [];

    /// <summary>
    /// 暂停
    /// </summary>
    [ObservableProperty]
    bool isPause;

    /// <summary>
    /// 串口列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<ComPort> comPorts = [];

    /// <summary>
    /// Com口配置
    /// </summary>
    [ObservableProperty]
    ComConfig comConfig = new();

    /// <summary>
    /// 接收的数据总数
    /// </summary>
    [ObservableProperty]
    int receiveBytesCount = 0;

    /// <summary>
    /// 发送的数据总数
    /// </summary>
    [ObservableProperty]
    int sendBytesCount = 0;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty]
    ModbusByteOrder byteOrder = ModbusByteOrder.DCBA;
    #endregion

    #region ******************************  自定义帧模块 属性  ******************************
    /// <summary>
    /// 输入消息 用于发送
    /// </summary>
    [ObservableProperty]
    private string inputMessage = "01 03 0000 0001";

    /// <summary>
    /// 自动校验模式选择 Crc校验模式
    /// </summary>
    [ObservableProperty]
    private CrcMode crcMode = CrcMode.Modbus;

    /// <summary>
    /// 自定义帧的输入框
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CustomFrame> customFrames =
    [
        new ("01 03 0000 0001 "),
        new ("01 04 0000 0001 "),
        new (""),
    ];
    #endregion

    #region 搜索设备模块 属性
    /// <summary>
    /// 搜索设备的状态 0=未开始搜索 1=搜索中 2=搜索结束/搜索中止
    /// </summary>
    [ObservableProperty]
    private int searchDeviceState = 0;

    /// <summary>
    /// 当前搜索的ModbusRtu设备
    /// </summary>
    [ObservableProperty]
    private ModbusRtuDevice currentDevice = new();

    /// <summary>
    /// 搜索到的ModbusRtu设备
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ModbusRtuDevice> modbusRtuDevices = [];

    /// <summary>
    /// 选中的波特率
    /// </summary>
    [ObservableProperty]
    private List<BaudRate> selectedBaudRates = [];

    ///// <summary>
    ///// 选中的校验方式
    ///// </summary>
    [ObservableProperty]
    private List<Parity> selectedParitys = [];


    /// <summary>
    /// 搜索使用的功能码
    /// </summary>
    [ObservableProperty]
    private byte searchFunctionCode = 3;

    /// <summary>
    /// 搜索时读取数据的起始地址
    /// </summary>
    [ObservableProperty]
    private int searchStartAddr = 0000;

    /// <summary>
    /// 搜索时读取的数量
    /// </summary>
    [ObservableProperty]
    private int searchReadNum = 1;

    /// <summary>
    /// 搜索到的设备总数
    /// </summary>
    [ObservableProperty]
    int foundCount = 0;
    #endregion


    #region ******************************  数据监控模块 属性  ******************************
    /// <summary>
    /// 数据监控配置
    /// </summary>
    [ObservableProperty]

    private DataMonitorConfig dataMonitorConfig = new();

    /// <summary>
    /// ModbusRtuDataDataView
    /// </summary>
    [ObservableProperty]
    private ListCollectionView modbusRtuDataDataView;

    /// <summary>
    /// 配置文件列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ConfigFile> configFiles = [];

    #endregion

    #region 自动应答模块 属性
    /// <summary>
    /// 是否开启自动应答
    /// </summary>
    [ObservableProperty]
    private bool isAutoResponse = false;

    /// <summary>
    /// ModbusRtu自动应答
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ModbusRtuAutoResponseData> mosbusRtuAutoResponseDatas = [];
    #endregion


    #region 串口操作方法
    /// <summary>
    /// 获取串口完整名字（包括驱动名字）
    /// </summary>
    [RelayCommand]
    public void GetComPorts()
    {
        if (ComConfig.IsOpened)
        {
            return;
        }

        //缓存最后一次选择的串口
        var lastComPort = ComConfig.ComPort;

        //清空列表
        ComPorts.Clear();

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
                ComPorts.Add(new ComPort(port, name));
            }
        }
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
            //停止自动读取
            if (DataMonitorConfig.IsOpened)
            {
                CloseAutoRead();
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

    #region 旧的数据接收方法 弃用了
    ///// <summary>
    ///// 接收消息
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    ///// <exception cref="NotImplementedException"></exception>
    //private void 旧的ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
    //{
    //    try
    //    {
    //        //若串口未开启则返回
    //        if (SerialPort != null && !SerialPort.IsOpen)
    //        {
    //            SerialPort.DiscardInBuffer();//丢弃接收缓冲区的数据
    //            return;
    //        }

    //        ComConfig.IsReceiving = true;

    //        //System.Diagnostics.Stopwatch oTime = new();   //定义一个计时对象  
    //        //oTime.Start();                         //开始计时 

    //        #region 接收数据

    //        //TODO 由于监控串口网络时,请求帧和应答帧时间间隔较短,会照成接收粘包  通过先截取一段数据分析是否为请求帧,为请求帧则将后面的解析为另一帧
    //        //0X01请求帧8字节 0x02请求帧8字节 0x03请求帧8字节 0x04请求帧8字节 0x05请求帧8字节  0x06请求帧8字节 0x0F请求帧不量不定 0x10请求帧不量不定
    //        //由于大部分请求帧长度为8字节 故对接收字节前8字节截取校验判断是否为一帧可以解决大部分粘包问题



    //        //接收的数据缓存
    //        List<byte> list = new();
    //        if (ComConfig.IsOpened == false)
    //            return;
    //        string msg = string.Empty;//
    //        //string tempMsg = string.Empty;//接收数据二次缓冲 串口接收数据先缓存至此
    //        int times = 0;//计算次数 连续数ms无数据判断为一帧结束
    //        do
    //        {
    //            if (ComConfig.IsOpened && SerialPort.BytesToRead > 0)
    //            {
    //                times = 0;
    //                int dataCount = SerialPort.BytesToRead;          //获取数据量
    //                byte[] tempBuffer = new byte[dataCount];         //声明数组
    //                SerialPort.Read(tempBuffer, 0, dataCount); //从串口缓存读取数据 从第0个读取n个字节, 写入tempBuffer 
    //                list.AddRange(tempBuffer);                       //添加进接收的数据列表
    //                msg += BitConverter.ToString(tempBuffer);
    //                //限制一次接收的最大数量 避免多设备连接时 导致数据收发无法判断帧结束
    //                if (list.Count > ComConfig.MaxLength)
    //                    break;
    //            }
    //            else
    //            {
    //                times++;
    //                Thread.Sleep(1);
    //            }
    //        } while (times < ComConfig.TimeOut);
    //        #endregion


    //        msg = msg.Replace('-', ' ');
    //        ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队

    //        //搜索时将验证通过的添加至搜索到的设备列表
    //        if (SearchDeviceState == 1)
    //        {
    //            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
    //            {
    //                CurrentDevice.ReceiveMessage = msg;
    //                CurrentDevice.Address = int.Parse(msg[..2], System.Globalization.NumberStyles.HexNumber);
    //                ModbusRtuDevices.Add(CurrentDevice);
    //            }));
    //            HcGrowlExtensions.Success($"搜索到设备 {CurrentDevice.Address}...", ModbusRtuView.ViewName);
    //        }

    //        ReceiveBytesCount += list.Count;         //计算总接收数据量
    //        //若暂停更新接收数据 则不显示
    //        if (IsPause)
    //            return;
    //        WaitUartReceived.Set();//置位数据接收完成标志
    //        //oTime.Stop();
    //        //ShowMessage($"接收数据用时{oTime.Elapsed.TotalMilliseconds} ms");
    //    }
    //    catch (Exception ex)
    //    {
    //        ShowMessage(ex.Message, MessageType.Receive);
    //    }
    //    finally
    //    {
    //        ComConfig.IsReceiving = false;
    //    }
    //} 
    #endregion

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
                                //输出接收到的数据
                                ReceiveFrameQueue.Enqueue(BitConverter.ToString(frame.ToArray()).Replace('-', ' '));//接收到的消息入队
                                WaitUartReceived.Set();                                                                           //置位数据接收完成标志
                                DeviceFound(msg);
                                frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的字节
                                ReceiveBytesCount += frame.Count;              //计算总接收数据量
                                isNot = false;
                                continue;
                            }
                            //前2字节都没有功能码,则将功能码调整至第二字节
                            else if (funcs[0] > 2)
                            {
                                frame = frameCache.Take(funcs[0] - 1).ToList();//功能码前一个字节为地址要保留,所以要-1
                                //输出接收到的数据
                                ReceiveFrameQueue.Enqueue(BitConverter.ToString(frame.ToArray()).Replace('-', ' '));//接收到的消息入队
                                WaitUartReceived.Set();                                                                           //置位数据接收完成标志
                                DeviceFound(msg);
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

                            //解析出可能的帧并校验成功
                            if (frame.Count > 0 && IsModbusCrcOk(frame))
                            {
                                msg = BitConverter.ToString(frame.ToArray()).Replace('-', ' ');
                                //输出接收到的数据
                                ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队
                                WaitUartReceived.Set();
                                DeviceFound(msg);

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
                            DeviceFound(msg);
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

            msg = BitConverter.ToString(frameCache.ToArray()).Replace('-', ' ');

            DeviceFound(msg);//搜索到设备
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
    /// 搜索到设备了
    /// </summary>
    private void DeviceFound(string msg)
    {
        //搜索时将验证通过的添加至搜索到的设备列表
        if (SearchDeviceState == 1 && msg.Length >= 2)
        {
            var device = CurrentDevice;//缓存当前设备。否则搜索速度快时,会有异步问题
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                device.ReceiveMessage = msg;
                device.Address = int.Parse(msg[..2], System.Globalization.NumberStyles.HexNumber);
                ModbusRtuDevices.Add(CurrentDevice);
                FoundCount++;
            }));
            //HcGrowlExtensions.Success($"搜索到设备 {device.Address}...", ModbusRtuView.ViewName);
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
                if (ReceiveFrameQueue.IsEmpty)
                {
                    WaitUartReceived.WaitOne(); //等待接收消息
                }

                //从接收消息队列中取出一条消息
                if(!ReceiveFrameQueue.TryDequeue(out var frame))
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
                else if (mFrame.Type.Equals(ModbusRtuFrameType.校验失败))
                {
                    ShowReceiveMessage(mFrame);

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
                    ShowReceiveMessage(mFrame);
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

                //03功能码或04功能码
                if (mFrame.Type.Equals(ModbusRtuFrameType._0x03响应帧) || mFrame.Type.Equals(ModbusRtuFrameType._0x04响应帧))
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
        //目前仅支持03功能码和04功能码

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
            PublishFrameEnqueue(GetCrcedStrWithSelect(InputMessage));                  //将待发送的消息添加进队列
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
    #endregion

    #region ******************************  搜索设备模块 方法  ******************************
    /// <summary>
    /// 波特率选框修改时
    /// </summary>
    /// <param name="obj"></param>
    public void BaudRateSelectionChanged(object obj)
    {
        //获取选中项列表
        System.Collections.IList items = (System.Collections.IList)obj;
        var collection = items.Cast<BaudRate>();
        //获取所有选中项
        SelectedBaudRates = collection.ToList();
    }

    /// <summary>
    /// 校验位选框修改时
    /// </summary>
    /// <param name="obj"></param>
    public void ParitySelectionChanged(object obj)
    {
        IList items = (IList)obj;
        var collection = items.Cast<Parity>();
        SelectedParitys = collection.ToList();
    }

    /// <summary>
    /// 自动搜索ModbusRtu设备
    /// </summary>
    public async void SearchDevices()
    {
        try
        {
            //若数据监控功能开启中则关闭
            if (DataMonitorConfig.IsOpened)
            {
                HcGrowlExtensions.Warning("数据监控功能关闭...", ModbusRtuView.ViewName);
                CloseAutoRead();
            }

            //设置串口
            //修改串口设置
            if (SelectedBaudRates.Count.Equals(0))
            {
                ShowErrorMessage("未选择要搜索的波特率");
                return;
            }
            if (SelectedParitys.Count.Equals(0))
            {
                ShowErrorMessage("未选择要搜索的校验位");
                return;
            }

            //打开串口
            if (ComConfig.IsOpened == false)
            {
                OpenCom();
            }

            //HcGrowlExtensions.Info("开始搜索...", ModbusRtuView.ViewName);

            ComConfig.TimeOut = 20;//搜索时设置帧超时时间为20
            SearchDeviceState = 1;//标记状态为搜索设备中
            ModbusRtuDevices.Clear();//清空搜索到的设备列表
            FoundCount = 0;

            //遍历选项
            //Flag:
            foreach (var baud in SelectedBaudRates)
            {
                foreach (var parity in SelectedParitys)
                {
                    //搜索
                    ShowMessage($"搜索: {ComConfig.ComPort.Port}:{ComConfig.ComPort.DeviceName} 波特率:{(int)baud} 校验方式:{parity} 数据位:{ComConfig.DataBits} 停止位:{ComConfig.StopBits}");
                    for (int i = 0; i <= 255; i++)
                    {
                        //当前搜索的设备
                        CurrentDevice = new()
                        {
                            Address = i,
                            BaudRate = baud,
                            Parity = parity,
                            DataBits = ComConfig.DataBits,
                            StopBits = ComConfig.StopBits
                        };

                        //修改设置
                        SerialPort.BaudRate = (int)baud;
                        SerialPort.Parity = (System.IO.Ports.Parity)parity;
                        //string unCrcMsg = $"{i:X2}0300000001";//读取第一个字           
                        string unCrcMsg = $"{i:X2}{SearchFunctionCode:X2}{SearchStartAddr:X4}{SearchReadNum:X4}";//根据设置读取数据
                        //串口关闭时或不处于搜索状态
                        if (ComConfig.IsOpened == false || SearchDeviceState != 1)
                            break;
                        PublishFrameEnqueue(GetModbusCrcedStr(unCrcMsg), ComConfig.SearchInterval);//发送消息入队
                        await Task.Delay(ComConfig.SearchInterval);           //间隔数ms后再请求下一个
                    }
                    if (ComConfig.IsOpened == false)
                        break;
                }
                if (ComConfig.IsOpened == false)
                    break;
            }
            if (ComConfig.IsOpened)
            {
                while (PublishFrameQueue.Count != 0)
                {
                    await Task.Delay(100);
                }
                ShowMessage("搜索完成");
                CloseCom();
            }
            else
            {
                ShowMessage("停止搜索");
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
        finally
        {
            SearchDeviceState = 2;//标记状态为搜索结束
        }
    }

    /// <summary>
    /// 停止搜索设备
    /// </summary>
    public void StopSearchDevices()
    {
        try
        {
            //若串口已打开则关闭
            if (ComConfig.IsOpened == true)
            {
                OperatePort();
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }
    #endregion

    #region ******************************  数据监控模块 方法  ******************************
    /// <summary>
    /// 打开数据监控
    /// </summary>
    public void OpenAutoRead()
    {
        try
        {
            //若搜索设备则先停止搜索
            if (SearchDeviceState == 1)
            {
                SearchDeviceState = 2;
            }

            //若串口未打开 则开启串口
            if (!ComConfig.IsOpened)
                OpenCom();
            //若串口开启失败则返回
            if (!ComConfig.IsOpened)
                return;

            timer = new()
            {
                Interval = DataMonitorConfig.Period,   //这里设置的间隔时间单位ms
                AutoReset = true                    //设置一直执行
            };
            timer.Elapsed += TimerElapsed;
            timer.Start();
            DataMonitorConfig.IsOpened = true;
            ShowMessage("开启数据监控...");

            //生成列表
            if (DataMonitorConfig.ModbusRtuDatas.Count != DataMonitorConfig.Quantity)
            {
                //关闭过滤器
                DataMonitorConfig.IsFilter = false;
                RefreshModbusRtuDataDataView();

                //生成列表
                DataMonitorConfig.ModbusRtuDatas.Clear();
                for (int i = DataMonitorConfig.StartAddr; i < DataMonitorConfig.Quantity + DataMonitorConfig.StartAddr; i++)
                {
                    DataMonitorConfig.ModbusRtuDatas.Add(new ModbusRtuData()
                    {
                        Addr = i
                    });
                }
            }
            //数量相同时 但是测点地址不同 更新地址
            else if(DataMonitorConfig.ModbusRtuDatas.FirstOrDefault().Addr != DataMonitorConfig.StartAddr)
            {
                int addr = DataMonitorConfig.StartAddr;
                foreach (var item in DataMonitorConfig.ModbusRtuDatas)
                {
                    item.Addr = addr;
                    addr++;
                }
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 关闭自动读取数据
    /// </summary>
    public void CloseAutoRead()
    {
        try
        {
            timer.Stop();
            DataMonitorConfig.IsOpened = false;
            CloseCom();
            ShowMessage("关闭自动读取数据...");
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 数据监控 定时器事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            timer.Stop();
            string msg = DataMonitorConfig.DataFrame[..^4];
            PublishMessage(GetModbusCrcedStr(msg));
            ////PublishMessage(DataMonitorConfig.DataFrame);
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
        finally
        {
            timer.Start();
        }
    }

    /// <summary>
    /// 更新数据监控视图
    /// </summary>
    public void RefreshModbusRtuDataDataView()
    {
        ModbusRtuDataDataView = (ListCollectionView)CollectionViewSource.GetDefaultView(DataMonitorConfig.ModbusRtuDatas);
        //数据监控过滤器
        if (DataMonitorConfig.IsFilter)
        {
            ModbusRtuDataDataView.Filter = new Predicate<object>(x => !string.IsNullOrWhiteSpace(((ModbusRtuData)x).Name));
        }
        else
        {
            ModbusRtuDataDataView.Filter = new Predicate<object>(x => true);
        }
        //TestDataView = (DataView)CollectionViewSource.GetDefaultView(DataMonitorConfig.ModbusRtuDatas);
        //var cview = CollectionViewSource.GetDefaultView(DataMonitorConfig.ModbusRtuDatas);
        //cview.Filter = new Predicate<object>(x => true);
        //TestDataView = (DataView)cview;
    }

    /// <summary>
    /// ModbusRtu数据写入
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void ModburRtuDataWrite(ModbusRtuData obj)
    {
        try
        {
            if (!ComConfig.IsOpened)
            {
                ShowErrorMessage("未打开串口,无法写入...");
                return;
            }
            //todo 使用正则验证值为数值
            if (obj.WriteValue == null)
            {
                ShowErrorMessage("写入值不能为空...");
                return;
            }

            double wValue = double.Parse(obj.WriteValue) / obj.Rate;              //对值的倍率做处理
            string unCrcFrame = "";
            dynamic data = "";
            //根据设定的类型转换值
            switch (obj.Type)
            {
                case DataType.uShort:
                    data = (ushort)wValue;
                    break;
                case DataType.Short:
                    data = (short)wValue;
                    break;
                case DataType.uInt:
                    data = (uint)wValue;
                    break;
                case DataType.Int:
                    data = (int)wValue;
                    break;
                case DataType.uLong:
                    data = (ulong)wValue;
                    break;
                case DataType.Long:
                    data = (long)wValue;
                    break;
                case DataType.Float:
                    data = (float)wValue;
                    break;
                case DataType.Double:
                    data = (double)wValue;
                    break;
                case DataType.Hex:
                    data = $"{obj.WriteValue:X4}";
                    break;
                default:
                    break;
            }

            //若是uShort或short则使用0x06功能码(有些设备并不支持0x10功能码) 其他使用0x10功能码
            if (obj.Type.Equals(DataType.uShort) || obj.Type.Equals(DataType.Short) || obj.Type.Equals(DataType.Hex))
            {
                //0x06写入帧
                unCrcFrame = $"{DataMonitorConfig.SlaveId:X2} 06 {obj.Addr:X4} {data:X4}";//未校验的数据帧
            }
            else
            {
                string jcqSl = (obj.DataTypeByteLength / 2).ToString("X4");     //寄存器数量
                string quantity = (obj.DataTypeByteLength).ToString("X2");      //字节数量
                //0x10写入帧
                string dataStr = BitConverter.ToString(ModbusRtuData.ByteOrder(BitConverter.GetBytes(data), obj.ModbusByteOrder)).Replace("-", "");
                unCrcFrame = $"{DataMonitorConfig.SlaveId:X2} 10 {obj.Addr:X4} {jcqSl} {quantity} {dataStr}";//未校验的数据帧
            }

            ShowMessage("数据写入...");
            PublishFrameEnqueue(GetModbusCrcedStr(unCrcFrame), 1000);
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 开关数据过滤器
    /// </summary>
    public void OperateFilter()
    {
        try
        {
            //验证过滤后是否有值 没有值则提示无法过滤
            if (!DataMonitorConfig.IsFilter)
            {
                var y = DataMonitorConfig.ModbusRtuDatas.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name));
                if (y == null)
                {
                    HcGrowlExtensions.Warning("请配置数据名称再开启过滤器...", ModbusRtuView.ViewName);
                    return;
                }
            }

            DataMonitorConfig.IsFilter = !DataMonitorConfig.IsFilter;
            RefreshModbusRtuDataDataView();
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 导出配置文件
    /// </summary>
    public void ExportConfig()
    {
        try
        {
            //配置文件目录
            string dict = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig");
            Wu.Utils.IoUtil.Exists(dict);
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "请选择导出配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonDMC)|*.jsonDMC",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                FileName = "Default",                                           //默认文件名
                DefaultExt = "jsonDMC",                                     //默认扩展名
                InitialDirectory = dict,                //指定初始的目录
                OverwritePrompt = true,                                                  //文件已存在警告
                AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(DataMonitorConfig);
            //保存文件
            Wu.CommTool.Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
            HcGrowlExtensions.Success("导出数据监控配置完成", nameof(ModbusRtuView));
            RefreshQuickImportList();//更新列表
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning("导出数据监控配置失败", nameof(ModbusRtuView));
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 导入配置文件
    /// </summary>
    public void ImportConfig()
    {
        try
        {
            //配置文件目录
            string dict = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig");
            Wu.Utils.IoUtil.Exists(dict);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonDMC)|*.jsonDMC",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                InitialDirectory = dict
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            DataMonitorConfig = JsonConvert.DeserializeObject<DataMonitorConfig>(xx)!;
            RefreshModbusRtuDataDataView();//更新数据视图
            HcGrowlExtensions.Success($"配置\"{Path.GetFileNameWithoutExtension(dlg.FileName)}\"导入完成", nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning("自动应答配置导入失败", nameof(ModbusRtuView));
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 快速导入配置文件
    /// </summary>
    /// <param name="obj"></param>
    public void QuickImportConfig(ConfigFile obj)
    {
        try
        {
            var xx = Core.Common.Utils.ReadJsonFile(obj.FullName);
            DataMonitorConfig = JsonConvert.DeserializeObject<DataMonitorConfig>(xx)!;
            if (DataMonitorConfig == null)
            {
                ShowErrorMessage("读取配置文件失败");
                return;
            }
            RefreshModbusRtuDataDataView();//更新数据视图
            HcGrowlExtensions.Success($"配置\"{Path.GetFileNameWithoutExtension(obj.FullName)}\"导入完成", nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning("自动应答配置导入失败", nameof(ModbusRtuView));
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 更新快速导入配置列表
    /// </summary>
    public void RefreshQuickImportList()
    {
        try
        {
            DirectoryInfo Folder = new DirectoryInfo(ModbusRtuConfigDict);
            //var a = Folder.GetFiles().Where(x => x.Extension.ToLower().Equals(".jsondmc"));
            var a = Folder.GetFiles().Select(item => new ConfigFile(item));
            ConfigFiles.Clear();
            foreach (var item in a)
            {
                ConfigFiles.Add(item);
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("读取配置文件夹异常: " + ex.Message);
        }
    }
    #endregion

    #region ******************************  自动应答 方法  ******************************
    /// <summary>
    /// 打开自动响应编辑界面
    /// </summary>
    /// <param name="obj"></param>
    public void OpenMosbusRtuAutoResponseDataEditView(ModbusRtuAutoResponseData obj)
    {
        //try
        //{
        //    if (obj == null)
        //        return;
        //    //添加参数
        //    DialogParameters param = new();
        //    if (obj != null)
        //        param.Add("Value", obj);
        //    var dialogResult = await dialogHost.ShowDialog(nameof(ModbusRtuAutoResponseDataEditView), param, nameof(ModbusRtuView));

        //    //TODO 将修改后的内容写入
        //    if (dialogResult.Result == ButtonResult.OK)
        //    {
        //        try
        //        {
        //            //UpdateLoading(true);
        //            //从结果中获取数据
        //            var resultDto = dialogResult.Parameters.GetValue<ModbusRtuAutoResponseData>("Value");
        //            if (resultDto == null)
        //            {
        //                return;
        //            }
        //            obj.Name = resultDto.Name;
        //            obj.MateTemplate = resultDto.MateTemplate;
        //            obj.ResponseTemplate = resultDto.ResponseTemplate;

        //            //aggregator.SendMessage($"{updateResult.Result.InformationNum}已修改完成");
        //        }
        //        catch (Exception ex)
        //        {
        //            aggregator.SendMessage($"{ex.Message}");
        //        }
        //        finally
        //        {
        //            //UpdateLoading(false);
        //        }
        //    }
        //    else if (dialogResult.Result == ButtonResult.Abort)
        //    {
        //        //Delete(model);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    aggregator.SendMessage(ex.Message);
        //}
    }

    /// <summary>
    /// 启用自动应答
    /// </summary>
    public void AutoResponseOn()
    {
        try
        {
            IsAutoResponse = true;
            ShowMessage("启用自动应答...");
            HcGrowlExtensions.Success("启用自动应答...", nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 关闭自动应答
    /// </summary>
    public void AutoResponseOff()
    {
        try
        {
            IsAutoResponse = false;
            ShowMessage("关闭自动应答...");
            HcGrowlExtensions.Warning("关闭自动应答...", nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 添加新的响应
    /// </summary>
    public void AddMosbusRtuAutoResponseData()
    {
        try
        {
            MosbusRtuAutoResponseDatas.Add(new());
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 导出自动应答配置文件
    /// </summary>
    public void ExportAutoResponseConfig()
    {
        try
        {
            //配置文件目录
            string dict = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuAutoResponseConfig");
            Wu.Utils.IoUtil.Exists(dict);
            Microsoft.Win32.SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonARC)|*.jsonARC",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                FileName = "Default",                                           //默认文件名
                DefaultExt = "jsonARC",                                     //默认扩展名
                InitialDirectory = dict,                //指定初始的目录
                OverwritePrompt = true,                                                  //文件已存在警告
                AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(MosbusRtuAutoResponseDatas);
            //保存文件
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
            HcGrowlExtensions.Success($"自动应答配置\"{Path.GetFileNameWithoutExtension(sfd.FileName)}\"导出成功", ModbusRtuView.ViewName);
            RefreshQuickImportList();//更新列表
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"自动应答配置导出失败", ModbusRtuView.ViewName);
            ShowErrorMessage(ex.Message);
        }
    }

    /// <summary>
    /// 导入自动应答配置文件
    /// </summary>
    public void ImportAutoResponseConfig()
    {
        try
        {
            //配置文件目录
            string dict = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuAutoResponseConfig");
            Wu.Utils.IoUtil.Exists(dict);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入自动应答配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonARC)|*.jsonARC",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                InitialDirectory = dict
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            MosbusRtuAutoResponseDatas = JsonConvert.DeserializeObject<ObservableCollection<ModbusRtuAutoResponseData>>(xx)!;
            RefreshModbusRtuDataDataView();//更新数据视图
            HcGrowlExtensions.Success($"自动应答配置\"{Path.GetFileNameWithoutExtension(dlg.FileName)}\"导出成功", ModbusRtuView.ViewName);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"自动应答配置导入成功", ModbusRtuView.ViewName);
            ShowErrorMessage(ex.Message);
        }
    }
    #endregion
}
