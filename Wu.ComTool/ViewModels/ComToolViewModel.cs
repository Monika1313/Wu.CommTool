using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using Wu.ComTool.Models;
using System.Management;
using HandyControl.Controls;
using System.IO;
using Wu.FzWater.Mqtt;
using System.Text;

namespace Wu.ComTool.ViewModels
{
    public class ComToolViewModel : BindableBase
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private SerialPort ComDevice = new SerialPort();
        #endregion

        public ComToolViewModel() { }
        public ComToolViewModel(IContainerProvider provider)
        {
            this.provider = provider;
            ExecuteCommand = new(Execute);

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
        /// 选中的串口
        /// </summary>
        public KeyValuePair<string, string> SelectedCom { get => _SelectedCom; set => SetProperty(ref _SelectedCom, value); }
        private KeyValuePair<string, string> _SelectedCom;

        /// <summary>
        /// 页面消息
        /// </summary>
        public ObservableCollection<MessageData> Messages { get => _Messages; set => SetProperty(ref _Messages, value); }
        private ObservableCollection<MessageData> _Messages = new();

        /// <summary>
        /// 串口是否已打开
        /// </summary>
        public bool IsComOpened { get => _IsComOpened; set => SetProperty(ref _IsComOpened, value); }
        private bool _IsComOpened = false;

        /// <summary>
        /// 发送的消息
        /// </summary>
        public string SendMessage { get => _SendMessage; set => SetProperty(ref _SendMessage, value); }
        private string _SendMessage = string.Empty;
        #endregion


        #region **************************************** 命令 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": GetDataAsync(); break;
                case "Add": break;
                case "Send": Send(); break;
                case "Clear": Clear(); break;
                case "Test": Test(); break;
                case "OpenCom": OperatePort(); break;        //打开串口
                case "ConfigCom": IsDrawersOpen.IsLeftDrawerOpen = true; break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 清空消息
        /// </summary>
        private void Clear()
        {
            Messages.Clear();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private bool Send()
        {
            //FileInfo fi = new FileInfo("D:\\Desktop\\XX.png");
            //ShowMessage("创建时间：" + fi.CreationTime.ToString() + "写入文件的时间" + fi.LastWriteTime + "访问的时间" + fi.LastAccessTime);
            byte[] data = Encoding.ASCII.GetBytes(SendMessage);
            if (ComDevice.IsOpen)
            {
                try
                {
                    ComDevice.Write(data, 0, data.Length);//发送数据
                    ShowMessage(SendMessage, MessageType.Send);
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

        /// <summary>
        /// 打开串口
        /// </summary>
        private void OperatePort()
        {
            try
            {
                if (ComDevice.IsOpen == false)
                {
                    //打开串口
                    ComDevice.PortName = SelectedCom.Key;               //串口
                    ComDevice.BaudRate = 9600;     //波特率
                                                   //ComDevice.BaudRate = ((int)ComConfig.BaudRate);     //波特率
                    ComDevice.Parity = Parity.Odd;                      //校验
                    ComDevice.DataBits = 8;                             //数据位
                    ComDevice.StopBits = StopBits.One;                  //停止位
                    try
                    {
                        ComDevice.Open();
                        IsComOpened = true;
                        ShowMessage($"开打串口{ComDevice.PortName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                        return;
                    }
                }
                else
                {
                    //关闭串口
                    try
                    {
                        ComDevice.Close();
                        IsComOpened = false;
                        ShowMessage($"关闭串口{ComDevice.PortName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
                }

            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
            finally
            {
                IsDrawersOpen.IsLeftDrawerOpen = false;
            }
        }







        /// <summary>
        /// Test
        /// </summary>
        private void Test()
        {
            var xx = SerialPort.GetPortNames();
            ////var y = SerialPort.
            //Ports.Clear();
            //foreach (var item in xx)
            //{
            //    Ports.Add(item);
            //}
            GetComPorts();
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
        /// definity
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> AAA { get => _Test; set => SetProperty(ref _Test, value); }
        private ObservableCollection<KeyValuePair<string, string>> _Test = new();
        /// <summary>
        /// 串口列表
        /// </summary>
        //public ObservableCollection<Dictionary<string, string>> Coms { get => _Coms; set => SetProperty(ref _Coms, value); }
        //private ObservableCollection<Dictionary<string, string>> _Coms;
        /// <summary>
        /// 获取串口完整名字（包括驱动名字）
        /// </summary>
        private void GetComPorts()
        {
            //清空列表
            ComPorts.Clear();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'"))
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
                ShowMessage("获取串口成功");
            }
        }

        private void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            Messages.Add(new MessageData($"{message}", DateTime.Now, type));
        }
        #endregion
    }
}
