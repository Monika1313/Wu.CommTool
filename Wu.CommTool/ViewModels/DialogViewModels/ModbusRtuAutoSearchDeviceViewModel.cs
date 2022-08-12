using DryIoc.Messages;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Wu.CommTool.Common;
using Wu.CommTool.Dialogs.Views;
using Wu.CommTool.Extensions;
using Wu.CommTool.Models;
using Wu.Extensions;
using Parity = Wu.CommTool.Models.Parity;

namespace Wu.CommTool.ViewModels.DialogViewModels
{
    public class ModbusRtuAutoSearchDeviceViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        private SerialPort SerialPort = new();
        public string DialogHostName { get; set; }
        #endregion

        public ModbusRtuAutoSearchDeviceViewModel() { }
        public ModbusRtuAutoSearchDeviceViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;

            ExecuteCommand = new(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            BaudRateSelectionChangedCommand = new DelegateCommand<object>(BaudRateSelectionChanged);
            ParitySelectionChangedCommand = new DelegateCommand<object>(ParitySelectionChanged);

            GetComPorts();
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);
        }

        private void ParitySelectionChanged(object obj)
        {
            IList items = (IList)obj;
            var collection = items.Cast<Parity>();
            SelectedParitys = collection.ToList();
        }

        private void BaudRateSelectionChanged(object obj)
        {
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<BaudRate>();
            SelectedBaudRates = collection.ToList();

        }

        #region **************************************** 属性 ****************************************
        /// <summary>
        /// CurrentDto
        /// </summary>
        public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
        private object _CurrentDto = new();

        /// <summary>
        /// 选中的波特率
        /// </summary>
        public IList<BaudRate> SelectedBaudRates { get => _SelectedBaudRates; set => SetProperty(ref _SelectedBaudRates, value); }
        private IList<BaudRate> _SelectedBaudRates = new List<BaudRate>();

        /// <summary>
        /// 选中的校验方式
        /// </summary>
        public IList<Parity> SelectedParitys { get => _SelectedParitys; set => SetProperty(ref _SelectedParitys, value); }
        private IList<Parity> _SelectedParitys = new List<Parity>();

        /// <summary>
        /// 串口配置
        /// </summary>
        public ComConfig ComConfig { get => _ComConfig; set => SetProperty(ref _ComConfig, value); }
        private ComConfig _ComConfig = new();

        /// <summary>
        /// 页面消息
        /// </summary>
        public ObservableCollection<MessageData> Messages { get => _Messages; set => SetProperty(ref _Messages, value); }
        private ObservableCollection<MessageData> _Messages = new();

        /// <summary>
        /// 串口列表
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> ComPorts { get => _ComPorts; set => SetProperty(ref _ComPorts, value); }
        private ObservableCollection<KeyValuePair<string, string>> _ComPorts = new();

        /// <summary>
        /// 发送的消息
        /// </summary>
        public string SendMessage { get => _SendMessage; set => SetProperty(ref _SendMessage, value); }
        private string _SendMessage = string.Empty;

        /// <summary>
        /// 搜索标志位
        /// </summary>
        public bool IsSearchStoped { get => _IsSearchStoped; set => SetProperty(ref _IsSearchStoped, value); }
        private bool _IsSearchStoped = true;

        /// <summary>
        /// ModbusRtu设备
        /// </summary>
        public ObservableCollection<ModbusRtuDevice> ModbusRtuDevices { get => _ModbusRtuDevices; set => SetProperty(ref _ModbusRtuDevices, value); }
        private ObservableCollection<ModbusRtuDevice> _ModbusRtuDevices = new();

        /// <summary>
        /// 当前设备
        /// </summary>
        public ModbusRtuDevice CurrentDevice { get => _CurrentDevice; set => SetProperty(ref _CurrentDevice, value); }
        private ModbusRtuDevice _CurrentDevice = new();

        /// <summary>
        /// 当前搜索的地址
        /// </summary>
        public int CurrentSearchDevice { get => _CurrentSearchDevice; set => SetProperty(ref _CurrentSearchDevice, value); }
        private int _CurrentSearchDevice = 1;
        #endregion


        #region **************************************** 命令 ****************************************
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        /// <summary>
        /// 波特率选框选项改变
        /// </summary>
        public DelegateCommand<object> BaudRateSelectionChangedCommand { get; private set; }

        /// <summary>
        /// definity
        /// </summary>
        public DelegateCommand<object> ParitySelectionChangedCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": Search(); break;
                case "AutoSearch": AutoSearch(); break;
                case "OpenDialogView": OpenDialogView(); break;
                default: break;
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
            using ManagementObjectSearcher searcher = new("select * from Win32_PnPEntity where Name like '%(COM[0-999]%'");
            var hardInfos = searcher.Get();
            foreach (var hardInfo in hardInfos)
            {
                if (hardInfo.Properties["Name"].Value != null)
                {
                    string deviceName = hardInfo.Properties["Name"].Value.ToString()!;
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


        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        private void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            try
            {
                //判断是UI线程还是子线程 若是子线程需要用委托
                var UiThreadId = System.Windows.Application.Current.Dispatcher.Thread.ManagedThreadId;       //UI线程ID
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
        /// 自动搜索ModbusRtu设备
        /// </summary>
        private async void AutoSearch()
        {
            try
            {
                //TODO 自动搜索ModbusRtu设备
                //设置串口
                //修改串口设置
                if (SelectedBaudRates.Count.Equals(0))
                {
                    ShowMessage("未选择要搜索的波特率", MessageType.Error);
                    return;
                }
                if (SelectedParitys.Count.Equals(0))
                {
                    ShowMessage("未选择要搜索的校验位", MessageType.Error);
                    return;
                }

                //打开串口
                OperatePort();
                if (ComConfig.IsOpened == false)
                {
                    return;
                }
                IsSearchStoped = false;
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
                IsSearchStoped = true;
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
                    //IsDrawersOpen.IsLeftDrawerOpen = false;
                }
                else
                {
                    try
                    {
                        ComConfig.IsOpened = false;          //标记串口已关闭
                        SerialPort.Close();                   //关闭串口
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
                //ShowMessage(ex.Message, MessageType.Error);
            }
            finally
            {

            }
        }


        /// <summary>
        /// 导航至该页面触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Search();
        }

        /// <summary>
        /// 打开该弹窗时执行
        /// </summary>
        public async void OnDialogOpend(IDialogParameters parameters)
        {
            //if (parameters != null && parameters.ContainsKey("ComConfig") && parameters.ContainsKey("SerialPort"))
            //{
            //    ComConfig = parameters.GetValue<ComConfig>("ComConfig");
            //    SerialPort = parameters.GetValue<SerialPort>("SerialPort");
            //    ComConfig.BaudRate = BaudRate._56000;
            //    SerialPort.BaudRate = (int)(BaudRate)BaudRate._300;
            //}
            //else
            //{
            //    throw new Exception("未提供页面所需的参数");
            //}
        }


        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            //确保串口关闭
            CloseCom();

            if (!DialogHost.IsDialogOpen(DialogHostName))
                return;
            //添加返回的参数
            DialogParameters param = new()
            {
                { "Value", CurrentDto }
            };
            //关闭窗口,并返回参数
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
            //确保串口关闭
            CloseCom();
            //若窗口处于打开状态则关闭
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
        }

        /// <summary>
        /// 弹窗
        /// </summary>
        private async void OpenDialogView()
        {
            try
            {
                //DialogParameters param = new()
                //{
                //    { "Value", CurrentDto }
                //};
                //var dialogResult = await dialogHost.ShowDialog(nameof(AutoSearchModbusRtuDeviceView), param, nameof(AutoSearchModbusRtuDeviceView));

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        private async void Search()
        {
            try
            {
                UpdateLoading(true);

            }
            catch (Exception ex)
            {
                aggregator.SendMessage($"{ex.Message}", "Main");
            }
            finally
            {
                UpdateLoading(false);
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
            Thread.Sleep(20);       //延时读取数据 等待数据接收完成
            int n = SerialPort.BytesToRead;          //获取接收的数据总数
            byte[] buf = new byte[n];
            SerialPort.Read(buf, 0, n);        //从第0个读取n个字节, 写入buf


            //todo 验证接收的数据是否校验正确
            //验证通过的添加至搜索到的设备列表
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ModbusRtuDevices.Add(CurrentDevice);
            }));

            ShowMessage(BitConverter.ToString(buf).Replace('-', ' '), MessageType.Receive);
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

                        //Crc校验
                        var code = Wu.Utils.Crc.Crc16Modbus(msg);
                        Array.Reverse(code);
                        crc.AddRange(code);

                        //合并数组
                        List<byte> list = new List<byte>();
                        list.AddRange(msg);
                        list.AddRange(crc);
                        var data = list.ToArray();
                        //SendBytesCount += data.Length;//统计发送数据总数
                        SerialPort.Write(data, 0, data.Length);//发送数据
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

        #endregion
    }
}
