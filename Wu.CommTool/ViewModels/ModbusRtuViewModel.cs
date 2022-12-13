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
using System.Timers;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Prism.Regions;
using ImTools;
using System.Collections;
using System.Threading.Tasks;

namespace Wu.CommTool.ViewModels
{
    public class ModbusRtuViewModel : NavigationViewModel
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        private SerialPort SerialPort = new();                 //串口
        protected System.Timers.Timer timer = new();  //定时器 定时读取数据
        //private object locker = new(); //线程锁
        #endregion

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


            //更新串口列表
            GetComPorts();

            //默认选中9600无校验
            SelectedBaudRates.Add(BaudRate._9600);
            SelectedParitys.Add(Models.Parity.None);
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
                        IsDrawersOpen2.IsLeftDrawerOpen = false;
                        break;
                    case "1":
                        ModbusRtuFunIndex = 1;
                        IsDrawersOpen2.IsLeftDrawerOpen = false;
                        break;
                    case "2":
                        ModbusRtuFunIndex = 2;
                        IsDrawersOpen2.IsLeftDrawerOpen = false;
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
        /// 自动搜索ModbusRtu设备
        /// </summary>
        private async void SearchDevices()
        {
            try
            {
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
                OperatePort();
                if (ComConfig.IsOpened == false)
                {
                    return;
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
                            SendMessage = $"{i:X2}0300000001";//读取第一个字
                            if (ComConfig.IsOpened == false)
                                break;
                            Send();
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
        /// ModbusRtuDatas
        /// </summary>
        public ObservableCollection<ModbusRtuData> ModbusRtuDatas { get => _ModbusRtuDatas; set => SetProperty(ref _ModbusRtuDatas, value); }
        private ObservableCollection<ModbusRtuData> _ModbusRtuDatas = new();

        /// <summary>
        /// 自动读取配置
        /// </summary>
        public AutoReadConfig AutoReadConfig { get => _AutoReadConfig; set => SetProperty(ref _AutoReadConfig, value); }
        private AutoReadConfig _AutoReadConfig = new();

        /// <summary>
        /// ModbusRtu功能菜单
        /// </summary>
        public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
        private ObservableCollection<MenuBar> _MenuBars = new()
            {
                new MenuBar() { Icon = "Number1", Title = "自定义数据帧", NameSpace = "0" },
                new MenuBar() { Icon = "Number2", Title = "搜索设备", NameSpace = "1" },
                new MenuBar() { Icon = "Number3", Title = "数据监控", NameSpace = "2" },
            };


        /// <summary>
        /// ModbusRtu功能选择Index
        /// </summary>
        public int ModbusRtuFunIndex { get => _ModbusRtuFunIndex; set => SetProperty(ref _ModbusRtuFunIndex, value); }
        private int _ModbusRtuFunIndex = 0;
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
        #endregion


        #region **************************************** 方法 ****************************************

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="obj"></param>
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": GetDataAsync(); break;
                case "Add": break;
                case "Pause": Pause(); break;
                case "AreaData": AreaData(); break;                                             //周期读取区域数据
                case "AutoSearch": OpenAutoSearchView(); break;                                 //打开搜索页面 该功能已启用
                case "SearchDevices": SearchDevices(); break;                                   //搜索ModbusRtu设备
                case "Send": Send(); break;                                                     //发送数据
                case "GetComPorts": GetComPorts(); break;                                       //查找Com口
                case "Clear": Clear(); break;                                                   //清空信息
                case "OpenCom": OpenCom(); break;                                               //打开串口
                case "OperatePort": OperatePort(); break;                                       //操作串口
                case "CloseCom": CloseCom(); break;                                             //关闭串口
                case "ShowModbusRtuFunSelect": IsDrawersOpen.IsLeftDrawerOpen = true; break;    //打开ModbusRtu功能选择左侧抽屉
                case "ConfigCom": IsDrawersOpen2.IsLeftDrawerOpen = true; break;                //打开ModbusRtu配置左侧抽屉
                case "OpenLeftDrawer3": IsDrawersOpen3.IsLeftDrawerOpen = true; break;          //打开3层抽屉的左侧抽屉
                case "OpenRightDrawer": IsDrawersOpen.IsRightDrawerOpen = true; break;         //打开1层右侧抽屉
                case "OpenAutoRead": OpenAutoRead(); break;                                     //打开自动读取
                case "CloseAutoRead": CloseAutoRead(); break;                                   //关闭自动读取
                case "ImportConfig": ImportConfig(); break;
                case "ExportConfig": ExportConfig(); break;
                case "ViewMessage": IsDrawersOpen3.IsRightDrawerOpen = true; break;             //打开数据监控页面右侧抽屉
                default: break;
            }
        }



        /// <summary>
        /// 关闭自动读取数据
        /// </summary>
        private void CloseAutoRead()
        {
            try
            {
                timer.Stop();
                AutoReadConfig.IsOpened = false;
                ShowMessage("关闭自动读取数据...");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 打开自动读取数据
        /// </summary>
        private async void OpenAutoRead()
        {
            try
            {
                //若串口未打开 则开启串口
                if (!ComConfig.IsOpened)
                    OpenCom();
                //若串口开启失败则返回
                if (!ComConfig.IsOpened)
                    return;

                timer = new()
                {
                    Interval = AutoReadConfig.Period,   //这里设置的间隔时间单位ms
                    AutoReset = true                    //设置一直执行
                };
                timer.Elapsed += TimerElapsed;
                timer.Start();
                AutoReadConfig.IsOpened = true;
                ShowMessage("开启数据监控...");
                IsDrawersOpen.IsRightDrawerOpen = false;

                //生成列表
                ModbusRtuDatas.Clear();
                for (int i = AutoReadConfig.StartAddr; i < AutoReadConfig.Quantity + AutoReadConfig.StartAddr; i++)
                {
                    ModbusRtuDatas.Add(new ModbusRtuData()
                    {
                        Addr = i
                    }); ;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 定时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                SendMessage = AutoReadConfig.DataFrame.Substring(0, AutoReadConfig.DataFrame.Length - 4);
                Send();
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
            ModbusRtuDatas.Clear();
            for (int i = 0; i < 100; i++)
            {
                ModbusRtuDatas.Add(new ModbusRtuData() { Addr = i });
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
        private bool Send()
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
                if (SendMessage is null || SendMessage.Length.Equals(0))
                {
                    ShowErrorMessage("发送的数据不能为空");
                    return false;
                }

                //验证数据字符必须符合16进制
                Regex regex = new(@"^[0-9 a-f A-F -]*$");
                if (!regex.IsMatch(SendMessage))
                {
                    ShowErrorMessage("数据字符仅限 0123456789 ABCDEF");
                    return false;
                }

                byte[] msg;
                try
                {
                    msg = SendMessage.Replace("-", string.Empty).GetBytes();
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
                        SendBytesCount += data.Length;//统计发送数据总数
                        SerialPort.Write(data, 0, data.Length);//发送数据
                        SendBytesCount += data.Length;//计算发送的数据量
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
        private async void ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                #region OLD 1 该方法需要等待一定的时间接收完数据, 由于长数据与短数据接收时间不同,会导致多条断数据数据被合并为一条或一条长数据被分成多条, 该方法并不实用
                //Thread.Sleep(60);       //延时读取数据 等待数据接收完成
                //if (!SerialPort.IsOpen)
                //{
                //    return;
                //}

                //int n = SerialPort.BytesToRead;          //获取接收的数据总数
                //byte[] buf = new byte[n];
                //SerialPort.Read(buf, 0, n);        //从第0个读取n个字节, 写入buf
                //ReceivBytesCount += buf.Length;          //统计发送的数据总数 
                #endregion


                //若串口未开启则返回
                if (!SerialPort.IsOpen)
                    return;

                #region 接收数据
                //接收的数据缓存
                List<byte> list = new();
                if (ComConfig.IsOpened == false)
                    return;
                //界面展示接收消息, 并缓存该条消息, 接收期间持续添加接收的数据
                var msg = new MessageData("", DateTime.Now, MessageType.Receive);
                if (!IsPause)
                    ShowMessage(msg);
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
                        if (!IsPause)
                            Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(() => msg.Content += (string.IsNullOrWhiteSpace(msg.Content) ? "" : " ") + BitConverter.ToString(tempBuffer).Replace('-', ' '));//更新界面消息
                        //限制一次接收的最大数量 避免多设备连接时 导致数据收发无法判断结束
                        if (list.Count > 300)
                            break;
                    }
                    else
                    {
                        times++;
                        Thread.Sleep(1);
                    }
                } while (times < 30);

                #region MyRegion
                //if (SerialPort.BytesToRead == 0)
                //{
                //    times++;
                //    Thread.Sleep(1);
                //}
                //else
                //    times = 0;
                //int dataCount = SerialPort.BytesToRead;          //获取数据量
                //byte[] tempBuffer = new byte[dataCount];         //声明数组
                //SerialPort.Read(tempBuffer, 0, dataCount); //从第0个读取n个字节, 写入tempBuffer 
                //list.AddRange(tempBuffer);                       //添加进接收的数据列表
                //if (!IsPause)
                //    Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(() => msg.Content += BitConverter.ToString(tempBuffer).Replace('-', ' '));//更新界面消息

                ////限制一次接收的最大数量
                //if (list.Count > 300)
                //    break; 
                #endregion

                //030300000078
                //判断接收缓存区是否有数据 有数据则读取 直接读取完接收缓存
                //while (ComConfig.IsOpened && SerialPort.BytesToRead > 0)
                //{
                //    #region 修改为一次读取多个, 这样可以适当增加休眠的等待时间, 避免由于设备响应速度慢导致一条数据变为多条数据
                //    int dataCount = SerialPort.BytesToRead;          //获取数据量
                //    byte[] tempBuffer = new byte[dataCount];         //声明数组
                //    SerialPort.Read(tempBuffer, 0, dataCount); //从第0个读取n个字节, 写入tempBuffer 
                //    list.AddRange(tempBuffer);                       //添加进接收的数据列表
                //    if (!IsPause)
                //        Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(() => msg.Content += BitConverter.ToString(tempBuffer).Replace('-', ' '));//更新界面消息

                //    Thread.Sleep(35);                 //等待数毫秒后确认是否读取完成

                //    #endregion
                //}
                #endregion

                #region old 2 该方法每读一个字节都延时一段时间, 会导致延时较高, 若调低延时则接收数据可能会分成多条
                //list.Add((byte)SerialPort.ReadByte());
                //Thread.Sleep(2);
                #endregion

                //TODO 搜索时将验证通过的添加至搜索到的设备列表
                if (SearchDeviceState != 2)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        ModbusRtuDevices.Add(CurrentDevice);
                    }));
                }
                

                //若自动读取开启则解析接收的数据
                if (AutoReadConfig.IsOpened)
                {
                    Analyse(list);
                }
                //计算总接收数据量
                ReceiveBytesCount += list.Count;

                //若暂停更新接收数据 则不显示
                if (IsPause)
                    return;

                //ShowMessage(BitConverter.ToString(list.ToArray()).Replace('-', ' '), MessageType.Receive);
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
            //对接收的数据进行CRC校验 若校验失败则直接返回
            //目前仅支持03功能码

            //判断字节数为奇数还是偶数
            //偶数为主站请求
            if (list.Count % 2 == 0)
                return;
            //奇数为响应
            //验证校验码
            var crc = Wu.Utils.Crc.Crc16Modbus(list.ToArray()); //带校验码校验 结果应为 00 00
            //校验失败则返回
            if (crc[0] != 0 || crc[1] != 0)
                return;
            //crc校验成功
            //验证数据是否为请求的数据
            if (list[0] != AutoReadConfig.SlaveId || list[1] != AutoReadConfig.Function && list[2] != AutoReadConfig.Quantity)
                return;//非请求的数据
            var byteArr = list.ToArray();

            //Todo解析数据
            //将读取的数据写入
            for (int i = 0; i < AutoReadConfig.Quantity; i++)
            {
                ModbusRtuDatas[i].OriginValue = Wu.Utils.ConvertUtil.GetUInt16FromBigEndianBytes(byteArr, 3 + 2 * i);
                ModbusRtuDatas[i].OriginBytes = byteArr;        //源字节数组
                ModbusRtuDatas[i].ModbusByteOrder = AutoReadConfig.ByteOrder; //字节序

                ModbusRtuDatas[i].Location = i*2 +3;            //在源字节数组中的起始位置 源字节数组为完整的数据帧,帧头部分3字节 每个值为1个word2字节
                ModbusRtuDatas[i].UpdateTime = DateTime.Now;    //更新时间
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
                //停止自动读取
                if (AutoReadConfig.IsOpened)
                {
                    CloseAutoRead();
                }

                ComConfig.IsOpened = false;          //标记串口已关闭
                                                     //SerialPort.DataReceived -= ReceiveMessage;
                SerialPort.Close();                   //关闭串口 

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
                    Filter = "json files(*.jsonModbusRtuConfig)|*.jsonModbusRtuConfig",    //文件格式过滤器
                    FilterIndex = 1,                                                         //默认选中的过滤器
                    FileName = "MqttClientConfig",                                           //默认文件名
                    DefaultExt = "jsonModbusRtuConfig",                                     //默认扩展名
                    InitialDirectory = dict,                //指定初始的目录
                    OverwritePrompt = true,                                                  //文件已存在警告
                    AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
                };
                if (sfd.ShowDialog() != true)
                    return;
                //将当前的配置序列化为json字符串
                var content = JsonConvert.SerializeObject(AutoReadConfig);
                //保存文件
                Common.Utils.WriteJsonFile(sfd.FileName, content);
                System.Windows.MessageBox.Show("导出完成");
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
                    Filter = "json files(*.jsonModbusRtuConfig)|*.jsonModbusRtuConfig",    //文件格式过滤器
                    FilterIndex = 1,                                                         //默认选中的过滤器
                    InitialDirectory = dict
                };

                if (dlg.ShowDialog() != true)
                    return;
                var xx = Common.Utils.ReadJsonFile(dlg.FileName);
                AutoReadConfig = JsonConvert.DeserializeObject<AutoReadConfig>(xx)!;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
        #endregion
    }
}
