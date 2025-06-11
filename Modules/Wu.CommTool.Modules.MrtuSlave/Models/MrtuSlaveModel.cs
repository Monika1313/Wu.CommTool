using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using Parity = Wu.CommTool.Modules.ModbusRtu.Enums.Parity;
using StopBits = Wu.CommTool.Modules.ModbusRtu.Enums.StopBits;

namespace Wu.CommTool.Modules.MrtuSlave.Models;

public partial class MrtuSlaveModel : ObservableObject
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private static readonly ILog log = LogManager.GetLogger(typeof(MrtuSlaveModel));
    private readonly SerialPort SerialPort = new();              //串口
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
    }
    #endregion **************************************** 构造函数 ****************************************


    #region **************************************** 属性 ****************************************

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
    /// 自动搜索设备的间隔 单位ms
    /// </summary>
    [ObservableProperty] int searchInterval = 100;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty] ModbusByteOrder byteOrder = ModbusByteOrder.DCBA;

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty] bool state;
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
                ShowMessage($"打开串口 {SerialPort.PortName} : {ComPort.DeviceName}  波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}");
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
    private void Stop()
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
            //PublishFrameQueue.Clear();      //清空发送帧队列

            ////清空接收帧队列
            //while (!ReceiveFrameQueue.IsEmpty)
            //{
            //    ReceiveFrameQueue.TryDequeue(out var item);
            //}

            //if (ComConfig.IsSending)
            //{
            //    await Task.Delay(100);
            //}
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
}
