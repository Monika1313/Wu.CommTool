using MaterialDesignThemes.Wpf;
using MQTTnet.Client.Options;
using MQTTnet;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Wu.CommTool.Common;
using Wu.CommTool.Extensions;
using Wu.CommTool.Models;
using static System.Windows.Forms.AxHost;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using System.Text;
using Wu.FzWater.Mqtt;
using System.Threading;

namespace Wu.CommTool.ViewModels
{
    public class MqttClientViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        private IMqttClient client;
        public string DialogHostName { get; set; } = "MqttClientView";
        #endregion

        public MqttClientViewModel() { }
        public MqttClientViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;

            ExecuteCommand = new(Execute);
            DeSubTopicCommand = new DelegateCommand<string>(DeSubTopic);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void DeSubTopic(string obj)
        {
            //TODO
            try
            {
                var xx = MqttClientConfig.SubscribeTopics.Remove(obj.ToString());
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        #region **************************************** 属性 ****************************************
        /// <summary>
        /// CurrentDto
        /// </summary>
        public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
        private object _CurrentDto = new();

        /// <summary>
        /// 页面消息
        /// </summary>
        public ObservableCollection<MessageData> Messages { get => _Messages; set => SetProperty(ref _Messages, value); }
        private ObservableCollection<MessageData> _Messages = new();

        /// <summary>
        /// 暂停更新接收的数据
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause = false;

        /// <summary>
        /// IsDrawersOpen
        /// </summary>
        public IsDrawersOpen IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private IsDrawersOpen _IsDrawersOpen = new();

        /// <summary>
        /// MqttClientConfig
        /// </summary>
        public MqttClientConfig MqttClientConfig { get => _MqttClientConfig; set => SetProperty(ref _MqttClientConfig, value); }
        private MqttClientConfig _MqttClientConfig = new();

        /// <summary>
        /// 发送的消息
        /// </summary>
        public string SendMessage { get => _SendMessage; set => SetProperty(ref _SendMessage, value); }
        private string _SendMessage = string.Empty;

        /// <summary>
        /// 新的订阅主题
        /// </summary>
        public string NewSubTopic { get => _NewSubTopic; set => SetProperty(ref _NewSubTopic, value); }
        private string _NewSubTopic = string.Empty;
        #endregion


        #region **************************************** 命令 ****************************************
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }


        /// <summary>
        /// 取消订阅主题
        /// </summary>
        public DelegateCommand<string> DeSubTopicCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": Search(); break;
                case "Open": OpenMqttClient(); break;
                case "Close": CloseMqttClient(); break;
                case "Clear": Clear(); break;
                case "Publish": Publish(); break;
                case "AddSubTopic": AddSubTopic(); break;                               //添加订阅的主题
                case "OpenLeftDrawer": IsDrawersOpen.IsLeftDrawerOpen = true; break;
                case "OpenRightDrawer": IsDrawersOpen.IsRightDrawerOpen = true; break;
                case "OpenDialogView": OpenDialogView(); break;
                default: break;
            }
        }

        /// <summary>
        /// 添加订阅的主题
        /// </summary>
        private void AddSubTopic()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewSubTopic))
                {
                    //不能订阅空主题
                    return;
                }
                //若重复了则取消
                if (MqttClientConfig.SubscribeTopics.Contains(NewSubTopic))
                    return;
                MqttClientConfig.SubscribeTopics.Add(NewSubTopic.Trim());
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        private async void Publish()
        {
            try
            {
                //byte[] xxx = null;
                //将待发送的消息进行编码
                //switch (SelectedDeCodeMode.Id)
                //{
                //    case 1:
                //        //xxx = Encoding.BigEndianUnicode.GetBytes(PublishMessage);
                //        ////接收到的信息
                //        ////var bytes = Wu.Wu_string.GetBytes("01-03-88-3E-B8-3A-07-3E-B5-AA-AB-3E-A7-8E-39-3E-AE-ED-09-3E-B5-AA-AB-3E-A9-D7-4E-00-00-00-00-42-9C-C4-80-42-99-6B-00-42-49-E0-00-43-CA-16-00-00-24-00-06-00-00-00-6C-00-14-00-28-00-00-00-78-00-1F-00-26-00-00-00-6D-00-1D-00-2D-00-00-00-7C-00-00-00-00-00-00-00-00-00-00-00-FA-01-19-01-00-01-12-00-00-00-00-00-00-00-00-00-6B-C1-A6-00-00-00-00-00-00-01-F4-01-F4-01-9D-00-00-00-02-00-02-00-01-3D-93-A1-2F-00-02-41-00-00-00-D4-33");
                //        ////截取消息部分  不包含校验码
                //        //var byteCrc = bytes.Skip(0).Take(bytes.Length).ToArray();
                //        //xxx = 
                //        //校验结果
                //        //var xx = Wu_Crc.Crc16Modbus(byteCrc);
                //        xxx = GetBytes(PublishMessage.Replace(" ", string.Empty));
                //        break;
                //    case 2:
                //        xxx = Encoding.UTF8.GetBytes(PublishMessage);
                //        break;
                //}

                //根据选择的消息质量进行设置
                var mqttAMB = new MqttApplicationMessageBuilder();

                //switch (switch_on)
                //{
                //    default:
                //}

                var mam = mqttAMB.WithTopic(MqttClientConfig.PublishTopic)                  //发布的主题
                .WithPayload(SendMessage)
                //.WithExactlyOnceQoS()
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();


                //var msg = new MqttApplicationMessageBuilder()
                //.WithTopic(MqttClientConfig.PublishTopic)                  //发布的主题
                //.WithPayload(SendMessage)
                ////.WithExactlyOnceQoS()
                //.WithAtLeastOnceQoS()
                //.WithRetainFlag()
                //.Build();

                await client.PublishAsync(mam, CancellationToken.None);
                ShowSendMessage($"发送消息:{SendMessage}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"发送失败:{ex.Message}");
            }
        }

        /// <summary>
        /// 清空页面消息
        /// </summary>
        private void Clear()
        {
            Messages = new();
        }

        /// <summary>
        /// 关闭Mqtt客户端
        /// </summary>
        private async void CloseMqttClient()
        {
            try
            {
                await client.DisconnectAsync();          //断开连接
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MqttClientConfig.IsOpened = false;
                });
                ShowMessage($"断开连接");
            }
            catch (Exception ex)
            {
                ShowMessage($"断开连接失败 {ex.Message}");
            }
        }

        /// <summary>
        /// 打开Mqtt客户端
        /// </summary>
        private async void OpenMqttClient()
        {
            try
            {
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(MqttClientConfig.ServerIp, MqttClientConfig.ServerPort)
                    .WithClientId(MqttClientConfig.ClientId)
                    .WithCredentials(MqttClientConfig.UserName, MqttClientConfig.Password).Build();

                client = new MqttFactory().CreateMqttClient()
                    .UseConnectedHandler(Connected)                        //连接成功事件
                    .UseDisconnectedHandler(DisConnected)                  //连接断开事件
                    .UseApplicationMessageReceivedHandler(ReceiveMessage); //接收消息事件

                ShowMessage("连接中...");
                await client.ConnectAsync(options);                //启动连接
                MqttClientConfig.IsOpened = true;
            }
            catch (Exception ex)
            {
                //ShowErrorMessage($"{ex.Message}");
            }
        }

        /// <summary>
        /// 接收消息事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task ReceiveMessage(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {

                ShowReceiveMessage(Encoding.UTF8.GetString(arg.ApplicationMessage.Payload));

                //switch (SelectedDeCodeMode.Id)
                //{
                //    case 2:
                //        str = Encoding.UTF8.GetString(c.ApplicationMessage.Payload) + "\r\n";
                //        break;
                //    case 1:
                //        str = $"接收消息：主题：“{c.ApplicationMessage.Topic}”\r\n" + dataFrame.ToString();
                //        //$"内容：“{(c.ApplicationMessage?.Payload == null ? null : BitConverter.ToString(c.ApplicationMessage?.Payload))}”";
                //        //接收的数据以字节显示
                //        break;
                //}


            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 掉线或主动离线都会调用该方法
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task DisConnected(MqttClientDisconnectedEventArgs arg)
        {
            if (arg == null)
                return;
            //异常导致的掉线
            if (arg.Exception != null)
            {
                MqttClientConfig.IsOpened = false;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowErrorMessage(arg.Exception.Message.ToString());
                    ShowErrorMessage("已断开连接");
                });

            }
        }

        /// <summary>
        /// 连接成功事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task Connected(MqttClientConnectedEventArgs arg)
        {
            try
            {
                ShowMessage($"连接服务器成功");//连接成功 
                //没有需要订阅消息  没有订阅或者 订阅的主题为空
                if (MqttClientConfig.SubscribeTopics.Count.Equals(0))
                    return;

                //订阅主题
                foreach (var x in MqttClientConfig.SubscribeTopics)
                {
                    await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(x).Build());       //订阅服务端消息
                    ShowMessage($"已订阅主题: {x}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"订阅失败: {ex.Message}");//连接成功 
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
            if (parameters != null && parameters.ContainsKey("Value"))
            {
                //var oldDto = parameters.GetValue<Dto>("Value");
                //var getResult = await employeeService.GetSinglePersonalStorageAsync(oldDto);
                //if(getResult != null && getResult.Status)
                //{
                //    CurrentDto = getResult.Result;
                //}
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            if (!DialogHost.IsDialogOpen(DialogHostName))
                return;
            //添加返回的参数
            DialogParameters param = new DialogParameters();
            param.Add("Value", CurrentDto);
            //关闭窗口,并返回参数
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
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
                DialogParameters param = new()
                {
                    { "Value", CurrentDto }
                };
                //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
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
        #endregion
    }
}
