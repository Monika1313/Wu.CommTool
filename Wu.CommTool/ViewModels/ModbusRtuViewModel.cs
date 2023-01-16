using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;
using Wu.CommTool.Common;
using Wu.CommTool.Dialogs.Views;
using Wu.CommTool.Extensions;
using Wu.CommTool.Models;
using Wu.CommTool.Views;
using Wu.Extensions;
using Wu.ViewModels;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Wu.CommTool.ViewModels
{
    public class ModbusRtuViewModel : NavigationViewModel
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        private readonly SerialPort SerialPort = new();              //串口
        protected System.Timers.Timer timer = new();        //定时器 定时读取数据
        private Queue<string> PublishFrameQueue = new();    //数据帧发送队列
        private Queue<string> ReceiveFrameQueue = new();    //数据帧处理队列
        //private object locker = new(); //线程锁
        Task publishHandleTask;
        Task receiveHandleTask;
        CancellationTokenSource cts = new();
        private int TaskDelayTime = int.MaxValue;
        #endregion


        #region **************************************** 构造函数 ****************************************
        public ModbusRtuViewModel() { }
        public ModbusRtuViewModel(IContainerProvider provider, IDialogHostService dialogHost)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;
            ExecuteCommand = new(Execute);
            ModbusRtuFunChangedCommand = new DelegateCommand<MenuBar>(ModbusRtuFunChanged);
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);
            BaudRateSelectionChangedCommand = new DelegateCommand<object>(BaudRateSelectionChanged);
            ParitySelectionChangedCommand = new DelegateCommand<object>(ParitySelectionChanged);
            ModburRtuDataWriteCommand = new DelegateCommand<ModbusRtuData>(ModburRtuDataWrite);

            //更新串口列表
            GetComPorts();

            //默认选中9600无校验
            SelectedBaudRates.Add(BaudRate._9600);
            SelectedParitys.Add(Models.Parity.None);

            //数据监控过滤器
            RefreshModbusRtuDataDataView();

            publishHandleTask = new Task(PublishFrame);
            receiveHandleTask = new Task(ReceiveFrame);
            publishHandleTask.Start();
            receiveHandleTask.Start();
        }
        #endregion





        #region 搜索设备 属性
        /// <summary>
        /// 当前搜索的ModbusRtu设备
        /// </summary>
        public ModbusRtuDevice CurrentDevice { get => _CurrentDevice; set => SetProperty(ref _CurrentDevice, value); }
        private ModbusRtuDevice _CurrentDevice = new();

        /// <summary>
        /// 搜索设备的状态 0=未开始搜索 1=搜索中 2=搜索结束/搜索中止
        /// </summary>
        public int SearchDeviceState { get => _SearchDeviceState; set => SetProperty(ref _SearchDeviceState, value); }
        private int _SearchDeviceState = 0;


        /// <summary>
        /// 搜索到的ModbusRtu设备
        /// </summary>
        public ObservableCollection<ModbusRtuDevice> ModbusRtuDevices { get => _ModbusRtuDevices; set => SetProperty(ref _ModbusRtuDevices, value); }
        private ObservableCollection<ModbusRtuDevice> _ModbusRtuDevices = new();

        /// <summary>
        /// 选中的波特率
        /// </summary>
        public IList<BaudRate> SelectedBaudRates { get => _SelectedBaudRates; set => SetProperty(ref _SelectedBaudRates, value); }
        private IList<BaudRate> _SelectedBaudRates = new List<BaudRate>();

        ///// <summary>
        ///// 选中的校验方式
        ///// </summary>
        public IList<Wu.CommTool.Models.Parity> SelectedParitys { get => _SelectedParitys; set => SetProperty(ref _SelectedParitys, value); }
        private IList<Wu.CommTool.Models.Parity> _SelectedParitys = new List<Wu.CommTool.Models.Parity>();

        /// <summary>
        /// definity
        /// </summary>
        public IsDrawersOpen ModbusRtuReadDrawerOpen { get => _ModbusRtuReadDrawerOpen; set => SetProperty(ref _ModbusRtuReadDrawerOpen, value); }
        private IsDrawersOpen _ModbusRtuReadDrawerOpen = new();
        #endregion


        #region **************************************** 属性 ****************************************
        /// <summary>
        /// 抽屉1
        /// </summary>
        public IsDrawersOpen IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private IsDrawersOpen _IsDrawersOpen = new();

        /// <summary>
        /// 抽屉2
        /// </summary>
        public IsDrawersOpen IsDrawersOpen2 { get => _IsDrawersOpen2; set => SetProperty(ref _IsDrawersOpen2, value); }
        private IsDrawersOpen _IsDrawersOpen2 = new();

        /// <summary>
        /// 抽屉3
        /// </summary>
        public IsDrawersOpen IsDrawersOpen3 { get => _IsDrawersOpen3; set => SetProperty(ref _IsDrawersOpen3, value); }
        private IsDrawersOpen _IsDrawersOpen3 = new();

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
        private string _SendMessage = "01 03 0000 0001";

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

        /// <summary>
        /// 暂停界面更新接收的数据
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause = false;

        /// <summary>
        /// 数据监控配置
        /// </summary>
        public DataMonitorConfig DataMonitorConfig { get => _DataMonitorConfig; set => SetProperty(ref _DataMonitorConfig, value); }
        private DataMonitorConfig _DataMonitorConfig = new();

        /// <summary>
        /// ModbusRtu功能菜单
        /// </summary>
        public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
        private ObservableCollection<MenuBar> _MenuBars = new()
            {
                new MenuBar() { Icon = "Number1", Title = "自定义帧", NameSpace = "0" },
                new MenuBar() { Icon = "Number2", Title = "搜索设备", NameSpace = "1" },
                new MenuBar() { Icon = "Number3", Title = "数据监控", NameSpace = "2" },
                //new MenuBar() { Icon = "Number4", Title = "数据读写", NameSpace = "3" },
            };


        /// <summary>
        /// ModbusRtu功能选择Index
        /// </summary>
        public int ModbusRtuFunIndex { get => _ModbusRtuFunIndex; set => SetProperty(ref _ModbusRtuFunIndex, value); }
        private int _ModbusRtuFunIndex = 0;


        ///// <summary>
        ///// 是否过滤没有设置的数据 ModbusRtu数据监控
        ///// </summary>
        //public bool IsFilter { get => _IsFilter; set => SetProperty(ref _IsFilter, value); }
        //private bool _IsFilter = false;

        /// <summary>
        /// ModbusRtuDataDataView
        /// </summary>
        public ListCollectionView ModbusRtuDataDataView { get => _ModbusRtuDataDataView; set => SetProperty(ref _ModbusRtuDataDataView, value); }
        private ListCollectionView _ModbusRtuDataDataView;

        ///// <summary>
        ///// definity
        ///// </summary>
        //public DataView TestDataView { get => _TestDataView; set => SetProperty(ref _TestDataView, value); }
        //private DataView _TestDataView;
        #endregion


        #region **************************************** 命令 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        /// <summary>
        /// ModbusRtu功能选择修改
        /// </summary>
        public DelegateCommand<MenuBar> ModbusRtuFunChangedCommand { get; private set; }

        /// <summary>
        /// 波特率选框选项改变
        /// </summary>
        public DelegateCommand<object> BaudRateSelectionChangedCommand { get; private set; }

        /// <summary>
        /// 校验位选框选项修改
        /// </summary>
        public DelegateCommand<object> ParitySelectionChangedCommand { get; private set; }

        /// <summary>
        /// ModburRtu数据写入
        /// </summary>
        public DelegateCommand<ModbusRtuData> ModburRtuDataWriteCommand { get; private set; }

        #endregion


        #region **************************************** 方法 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="obj"></param>
        public void Execute(string obj)
        {
            try
            {
                switch (obj)
                {
                    case "Add": break;
                    case "Pause": Pause(); break;

                    case "AreaData": AreaData(); break;                                             //周期读取区域数据
                    case "Test": Test(); break;                                             //周期读取区域数据

                    case "SearchDevices": SearchDevices(); break;                                   //搜索ModbusRtu设备
                    case "StopSearchDevices": StopSearchDevices(); break;                           //停止搜索ModbusRtu设备

                    case "Send":
                        //若串口未打开则打开串口
                        if (!ComConfig.IsOpened)
                        {
                            ShowMessage("串口未打开, 尝试打开串口...");
                            OpenCom();
                        }
                        PublishFrameQueue.Enqueue(SendMessage);          //将待发送的消息添加进队列
                        break;                                           //发送数据
                    case "GetComPorts": GetComPorts(); break;                                       //查找Com口
                    case "Clear": Clear(); break;                                                   //清空页面信息
                    case "OpenCom": OpenCom(); break;                                               //打开串口
                    case "CloseCom": CloseCom(); break;                                             //关闭串口
                    case "OperatePort": OperatePort(); break;                                       //操作串口 开启则关闭 关闭则开启

                    case "OperateFilter": OperateFilter(); break;                                   //操作ModbusRtu数据过滤器

                    case "ShowModbusRtuFunSelect": IsDrawersOpen.IsLeftDrawerOpen = true; break;    //打开ModbusRtu功能选择左侧抽屉
                    case "ConfigCom": IsDrawersOpen2.IsLeftDrawerOpen = true; break;                //打开ModbusRtu配置左侧抽屉
                    case "ModbusRtuReadDrawerOpenLeft": ModbusRtuReadDrawerOpen.IsLeftDrawerOpen = true; break;  //打开ModbusRtu配置左侧抽屉
                    case "OpenLeftDrawer3": IsDrawersOpen3.IsLeftDrawerOpen = true; break;          //打开3层抽屉的左侧抽屉
                    case "OpenRightDrawer": IsDrawersOpen.IsRightDrawerOpen = true; break;          //打开1层右侧抽屉

                    case "OpenAutoRead": OpenAutoRead(); break;                                     //打开自动读取
                    case "CloseAutoRead": CloseAutoRead(); break;                                   //关闭自动读取

                    case "ImportConfig": ImportConfig(); break;                                     //导入数据监控配置
                    case "ExportConfig": ExportConfig(); break;                                     //导出数据监控配置
                    case "ViewMessage": IsDrawersOpen3.IsRightDrawerOpen = true; break;             //打开数据监控页面右侧抽屉
                    default: break;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void Test()
        {
            DataMonitorConfig.ModbusRtuDatas[0].Rate += 1;
        }

        /// <summary>
        /// 开关数据过滤器
        /// </summary>
        private void OperateFilter()
        {
            DataMonitorConfig.IsFilter = !DataMonitorConfig.IsFilter;
            RefreshModbusRtuDataDataView();
        }

        /// <summary>
        /// 更新数据监控视图
        /// </summary>
        private void RefreshModbusRtuDataDataView()
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
        /// 关闭自动读取数据
        /// </summary>
        /// ++
        /// +
        private void CloseAutoRead()
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
        /// 打开数据监控
        /// </summary>
        private async void OpenAutoRead()
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

                //数据监控时, 发送队列处理时间延迟, 避免数据采集与数据写入冲突
                TaskDelayTime = 300;

                timer = new()
                {
                    Interval = DataMonitorConfig.Period,   //这里设置的间隔时间单位ms
                    AutoReset = true                    //设置一直执行
                };
                timer.Elapsed += TimerElapsed;
                timer.Start();
                DataMonitorConfig.IsOpened = true;
                ShowMessage("开启数据监控...");
                IsDrawersOpen.IsRightDrawerOpen = false;


                //生成列表
                if (DataMonitorConfig.ModbusRtuDatas.Count != DataMonitorConfig.Quantity)
                {
                    DataMonitorConfig.ModbusRtuDatas.Clear();
                    for (int i = DataMonitorConfig.StartAddr; i < DataMonitorConfig.Quantity + DataMonitorConfig.StartAddr; i++)
                    {
                        DataMonitorConfig.ModbusRtuDatas.Add(new ModbusRtuData()
                        {
                            Addr = i
                        }); ;
                    }
                }
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
        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                string msg = DataMonitorConfig.DataFrame[..^4];
                PublishMessage(msg);
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
        /// 周期读取区域数据
        /// </summary>
        private void AreaData()
        {
            DataMonitorConfig.ModbusRtuDatas.Clear();
            for (int i = 0; i < 100; i++)
            {
                DataMonitorConfig.ModbusRtuDatas.Add(new ModbusRtuData() { Addr = i });
            }
        }

        /// <summary>
        /// 暂停更新接收的数据
        /// </summary>
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

                    #region 开启数据帧处理线程
                    //cts = new CancellationTokenSource();
                    //publishHandleTask = new Task(PublishFrame, cts.Token);
                    //receiveHandleTask = new Task(ReceiveFrame, cts.Token);
                    //publishHandleTask.Start();
                    //receiveHandleTask.Start(); 

                    //修改延时时间
                    TaskDelayTime = 50;
                    //取消上一次的延时
                    cts.Cancel();
                    #endregion

                    ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.Port.Value}  波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}");
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
            ReceiveBytesCount = 0;
            SendBytesCount = 0;
            Messages.Clear();
        }

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

                byte[] msg;
                try
                {
                    msg = message.Replace("-", string.Empty).GetBytes();
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
                        SerialPort.Write(data, 0, data.Length);     //发送数据
                        SendBytesCount += data.Length;                    //统计发送数据总数

                        if (!IsPause)
                            ShowMessage(BitConverter.ToString(data).Replace('-', ' '), MessageType.Send);
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
                    return;

                #region 接收数据
                //接收的数据缓存
                List<byte> list = new();
                if (ComConfig.IsOpened == false)
                    return;
                string msg = string.Empty;
                int times = 0;
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
                        //Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(() => msg.Content += (string.IsNullOrWhiteSpace(msg.Content) ? "" : " ") + BitConverter.ToString(tempBuffer).Replace('-', ' '));//更新界面消息
                        //限制一次接收的最大数量 避免多设备连接时 导致数据收发无法判断帧结束
                        if (list.Count > 300)
                            break;
                    }
                    else
                    {
                        times++;
                        //await Task.Delay(millisecondsDelay: 10);
                        Thread.Sleep(1);
                    }
                } while (times < 30);
                #endregion

                if (!IsPause)
                {
                    msg = msg.Replace('-', ' ');
                    ReceiveFrameQueue.Enqueue(msg);
                }


                //TODO 搜索时将验证通过的添加至搜索到的设备列表
                if (SearchDeviceState == 1)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        CurrentDevice.ReceiveMessage = msg;
                        CurrentDevice.Address = int.Parse(msg[..2], System.Globalization.NumberStyles.HexNumber);
                        ModbusRtuDevices.Add(CurrentDevice);
                    }));
                }

                //计算总接收数据量
                ReceiveBytesCount += list.Count;

                //若暂停更新接收数据 则不显示
                if (IsPause)
                    return;

            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Receive);
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
                //cts.Cancel();                 //停止帧处理线程
                TaskDelayTime = int.MaxValue;
                cts.TryReset();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
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
                        string deviceName = hardInfo.Properties["Name"].Value.ToString()!;
                        //从名称中截取串口
                        List<string> dList = new();
                        foreach (Match mch in Regex.Matches(deviceName, @"COM\d{1,3}").Cast<Match>())
                        {
                            string x = mch.Value.Trim();
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
                    //查找第一个USB设备
                    var usbDevice = ComPorts.FindFirst(x => x.Value.ToLower().Contains("usb"));
                    //搜索结果不为空
                    if (usbDevice.Key != null)
                    {
                        //默认选中项 若含USB设备则指定第一个USB, 若不含USB则指定第一个
                        ComConfig.Port = usbDevice;
                    }
                    else
                    {
                        //没有usb设备则选中第一个
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
        }

        protected void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);
        protected void ShowReceiveMessage(string message) => ShowMessage(message, MessageType.Receive);
        protected void ShowSendMessage(string message) => ShowMessage(message, MessageType.Send);

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            try
            {
                void action()
                {
                    Messages.Add(new MessageData($"{message}", DateTime.Now, type));
                    while (Messages.Count > 100)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(action);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void ShowMessage(MessageData msg)
        {
            try
            {
                void action()
                {
                    Messages.Add(msg);
                    while (Messages.Count > 100)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(action);
            }
            catch (Exception) { }
        }

        #region 已弃用
        ///// <summary>
        ///// 弃用  自动搜索串口设备
        ///// </summary>
        ///// <exception cref="NotImplementedException"></exception>
        //private async void OpenAutoSearchView()
        //{
        //    try
        //    {
        //        //若串口已打开 提示需要关闭串口
        //        if (ComConfig.IsOpened)
        //        {
        //            //弹窗确认 使用该功能需要先关闭串口
        //            var dialogResult = await dialogHost.Question("温馨提示", $"使用自动搜索功能将关闭当前串口, 确认是否关闭 {ComConfig.Port.Key} : {ComConfig.Port.Value}?");
        //            //取消
        //            if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK)
        //                return;
        //            //关闭串口
        //            CloseCom();
        //        }

        //        //打开自动搜索界面
        //        //添加要传递的参数
        //        DialogParameters param = new()
        //        {
        //            { nameof(SerialPort), SerialPort },
        //            { nameof(ComConfig), ComConfig }
        //        };
        //        //弹窗
        //        var dialogResult2 = await dialogHost.ShowDialog(nameof(ModbusRtuAutoSearchDeviceView), param, nameof(ModbusRtuView));
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessage(ex.Message, MessageType.Error);
        //    }
        //} 
        #endregion


        /// <summary>
        /// 导出配置文件
        /// </summary>
        private void ExportConfig()
        {
            try
            {
                //配置文件目录
                string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig");
                Wu.Utils.IOUtil.Exists(dict);
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
                {
                    Title = "请选择导出配置文件...",                                              //对话框标题
                    Filter = "json files(*.jsonDMC)|*.jsonDMC",    //文件格式过滤器
                    FilterIndex = 1,                                                         //默认选中的过滤器
                    FileName = "MqttClientConfig",                                           //默认文件名
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
                Common.Utils.WriteJsonFile(sfd.FileName, content);
                ShowMessage("导出配置完成");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 导入配置文件
        /// </summary>
        private void ImportConfig()
        {
            try
            {
                //配置文件目录
                string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig");
                Wu.Utils.IOUtil.Exists(dict);
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
                var xx = Common.Utils.ReadJsonFile(dlg.FileName);
                DataMonitorConfig = JsonConvert.DeserializeObject<DataMonitorConfig>(xx)!;
                RefreshModbusRtuDataDataView();//更新数据视图
                ShowMessage("导入配置完成");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// ModbusRtu功能选择修改
        /// </summary>
        /// <param name="obj"></param>
        private void ModbusRtuFunChanged(MenuBar obj)
        {
            try
            {
                switch (obj.NameSpace)
                {
                    case "0":
                        ModbusRtuFunIndex = 0;
                        IsDrawersOpen.IsLeftDrawerOpen = false;
                        break;
                    case "1":
                        ModbusRtuFunIndex = 1;
                        IsDrawersOpen.IsLeftDrawerOpen = false;
                        break;
                    case "2":
                        ModbusRtuFunIndex = 2;
                        IsDrawersOpen.IsLeftDrawerOpen = false;
                        break;
                    case "3":
                        ModbusRtuFunIndex = 3;
                        IsDrawersOpen.IsLeftDrawerOpen = false;
                        break;
                }
            }
            catch { }
        }

        /// <summary>
        /// 校验位选框修改时
        /// </summary>
        /// <param name="obj"></param>
        private void ParitySelectionChanged(object obj)
        {
            IList items = (IList)obj;
            var collection = items.Cast<Wu.CommTool.Models.Parity>();
            SelectedParitys = collection.ToList();
        }

        /// <summary>
        /// 波特率选框修改时
        /// </summary>
        /// <param name="obj"></param>
        private void BaudRateSelectionChanged(object obj)
        {
            //获取选中项列表
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<BaudRate>();
            //获取所有选中项
            SelectedBaudRates = collection.ToList();
        }

        /// <summary>
        /// 停止搜索设备
        /// </summary>
        private void StopSearchDevices()
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

        /// <summary>
        /// 自动搜索ModbusRtu设备
        /// </summary>
        private async void SearchDevices()
        {
            try
            {
                //若数据监控功能开启中则关闭
                if (DataMonitorConfig.IsOpened)
                {
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
                    OperatePort();
                }

                SearchDeviceState = 1;//标记状态为搜索设备中
                //清空搜索到的设备列表
                ModbusRtuDevices.Clear();

                //遍历选项

                //Flag:
                foreach (var baud in SelectedBaudRates)
                {
                    foreach (var parity in SelectedParitys)
                    {
                        //搜索
                        ShowMessage($"搜索: {ComConfig.Port.Key}:{ComConfig.Port.Value} 波特率:{(int)baud} 校验方式:{parity} 数据位:{ComConfig.DataBits} 停止位:{ComConfig.StopBits}");

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
                            string msg = $"{i:X2}0300000001";//读取第一个字
                            //串口关闭时或不处于搜索状态
                            if (ComConfig.IsOpened == false || SearchDeviceState != 1)
                                break;
                            PublishMessage(msg);
                            await Task.Delay(100);
                        }
                        if (ComConfig.IsOpened == false)
                            break;
                    }
                    if (ComConfig.IsOpened == false)
                        break;
                }
                if (ComConfig.IsOpened)
                {
                    ShowMessage("搜索完成");
                    OperatePort();
                }
                else
                {
                    ShowMessage("停止搜索");
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                SearchDeviceState = 2;//标记状态为搜索结束
            }
        }

       
        #endregion


        #region **************************************** 数据帧处理 ****************************************
        /// <summary>
        /// 发送数据帧处理线程
        /// </summary>
        private async void PublishFrame()
        {
            while (true)
            {
                try
                {
                    //if (ComConfig.IsOpened == false)
                    //    return;
                    //判断队列是否不空 若为空则等待
                    if (PublishFrameQueue.Count == 0)
                    {
                        try
                        {
                            await Task.Delay(TaskDelayTime, cts.Token);
                        }
                        catch (TaskCanceledException ex)
                        {
                            cts.Dispose();
                            cts = new();
                        }
                        continue;
                    }

                    var frame = PublishFrameQueue.Dequeue();  //出队 数据帧
                    PublishMessage(frame);                    //请求发送数据帧
                                                              //从队列读取的间隔为50ms
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// 接收数据帧处理线程
        /// </summary>
        private async void ReceiveFrame()
        {
            while (true)
            {
                try
                {
                    //若无消息需要处理则不执行
                    if (ReceiveFrameQueue.Count == 0)
                    {
                        try
                        {
                            await Task.Delay(TaskDelayTime, cts.Token);
                        }
                        catch (TaskCanceledException ex)
                        {
                            cts.Dispose();
                            cts = new();
                        }
                        continue;
                    }

                    //从接收消息队列中取出一条消息
                    var frame = ReceiveFrameQueue.Dequeue();
                    if (frame is null)
                    {
                        continue;
                    }

                    // 1 对接收的消息直接进行crc校验
                    var crc = Wu.Utils.Crc.Crc16Modbus(frame.GetBytes());   //校验码
                    //若校验结果不为0000则校验失败
                    if (crc == null || !crc[0].Equals(0) || !crc[1].Equals(0))
                    {
                        ShowReceiveMessage(frame + "\n校验失败...");
                        continue;
                    }
                    else
                    {
                        ShowReceiveMessage(frame);
                    }

                    //CRC校验通过 执行以下内容
                    List<byte> frameList = frame.GetBytes().ToList();//将字符串类型的数据帧转换为字节列表

                    int slaveId = frameList[0]; //从站地址
                    int func = frameList[1];    //功能码


                    //03功能码 字节数量奇数为响应帧 字节数量=8请求帧    错误码0x83
                    if (func == 0x03)
                    {
                        //判断字节数量是否为奇数
                        if (frameList.Count % 2 == 1)
                        {
                            //若自动读取开启则解析接收的数据
                            if (DataMonitorConfig.IsOpened)
                            {
                                //验证数据是否为请求的数据 根据 从站地址 功能码 数据字节数量
                                if (frameList[0] == DataMonitorConfig.SlaveId && frameList[2] == DataMonitorConfig.Quantity * 2)
                                {
                                    Analyse(frameList);
                                }

                                //frame = frame.Replace(" ", "");
                                //if (Convert.ToInt32(frame[..2], 16) == DataMonitorConfig.SlaveId && Convert.ToInt32(frame[2..4], 16) == DataMonitorConfig.Function && Convert.ToInt32(frame[4..6], 16) == DataMonitorConfig.Quantity * 2)
                                //{
                                //    //将数据帧转换为list
                                //    List<byte> list = frame.GetBytes().ToList();
                                //    Analyse(list);
                                //}
                            }
                        }
                    }

                    //0x10功能码 多字节写入  错误码 0x90
                    if (func == 0x10)
                    {
                        //请求帧为奇数字节


                        //响应帧8字节
                        if (frameList.Count == 8)
                        {
                            //todo验证写入是否成功
                            ShowMessage("数据写入成功");
                        }
                    }

                    await Task.Delay(20);
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// ModbusRtu数据写入
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ModburRtuDataWrite(ModbusRtuData obj)
        {
            try
            {
                //TODO编辑写入数据帧
                string addr = DataMonitorConfig.SlaveId.ToString("X2");         //从站地址
                string fun = "10";                                                     //0x10 写入多个寄存器
                string startAddr = obj.Addr.ToString("X4");                     //起始地址
                string jcqSl = (obj.DataTypeByteLength / 2).ToString("X4");        //寄存器数量
                string quantity = (obj.DataTypeByteLength).ToString("X2");  //字节数量

                //Todo 将待写入值根据数据类型反向转换为设备的数据类型
                //string data = obj.WriteValue.ToString($"X{obj.DataTypeByteLength}");   //数据值
                //Todo 数据值编辑

                double wValue = double.Parse(obj.WriteValue) / obj.Rate;//对值的倍率做处理
                string dataStr = "";
                dynamic data = "";
                //根据设定的类型转换值
                switch (obj.Type)
                {
                    case Enums.DataType.uShort:
                        data = (ushort)wValue;
                        break;
                    case Enums.DataType.Short:
                        data = (short)wValue;
                        break;
                    case Enums.DataType.uInt:
                        data = (uint)wValue;
                        break;
                    case Enums.DataType.Int:
                        data = (int)wValue;
                        break;
                    case Enums.DataType.uLong:
                        data = (ulong)wValue;
                        break;
                    case Enums.DataType.Long:
                        data = (long)wValue;
                        break;
                    case Enums.DataType.Float:
                        data = (float)wValue;
                        break;
                    case Enums.DataType.Double:
                        data = (double)wValue;
                        break;
                    default:
                        break;
                }

                dataStr = BitConverter.ToString(ModbusRtuData.ByteOrder(BitConverter.GetBytes(data), obj.ModbusByteOrder)).Replace("-", "");

                string unCrcFrame = addr + fun + startAddr + quantity;       //未校验的数据帧
                var crc = Wu.Utils.Crc.Crc16Modbus(unCrcFrame.GetBytes());   //校验码
                string frame = $"{addr} {fun} {startAddr} {jcqSl} {quantity} {dataStr} {crc[1]:X2}{crc[0]:X2}";

                ShowMessage("数据写入...");
                //请求发送数据帧
                PublishFrameQueue.Enqueue(frame);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
        #endregion
    }
}
