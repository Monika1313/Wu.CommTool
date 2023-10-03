using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MQTTnet;
using MQTTnet.Server;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Wu.CommTool.Core.Extensions;
using Wu.CommTool.Shared.Eunms;
using Wu.CommTool.Shared.Models;
using Wu.Extensions;
using Wu.ViewModels;
using Wu.Wpf.Models;
using Wu.Wpf.Common;
using MQTTnet.Protocol;
using System.Threading.Tasks;
using Wu.CommTool.Modules.MqttServer.Models;
using Wu.CommTool.Modules.MqttServer.Model;
using Wu.CommTool.Shared.Enums.Mqtt;
using MQTTnet.Internal;
using Wu.CommTool.Modules.MqttServer.Views;

namespace Wu.CommTool.Modules.MqttServer.ViewModels
{
    public class MqttServerViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        //public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string DialogHostName { get; set; } = MqttServerView.ViewName;
        private MQTTnet.Server.MqttServer mqttServer;                                 //Mqtt服务器
        #endregion

        public MqttServerViewModel() { }
        public MqttServerViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;

            ExecuteCommand = new(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            OpenJsonDataViewCommand = new DelegateCommand<MessageData>(OpenJsonDataView);
            //UnsubscribeTopicCommand = new DelegateCommand<MqttSubedTopic>(UnsubscribeTopic);

            //从默认配置文件中读取配置
            try
            {
                string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttServerConfig\Default.jsonMSC");
                var xx = Shared.Common.Utils.ReadJsonFile(p);
                var x = JsonConvert.DeserializeObject<MqttServerConfig>(xx);
                if (x == null)
                    return;
                MqttServerConfig = x;
                ShowMessage("读取配置成功");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("配置文件读取失败");
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
        /// 打开抽屉
        /// </summary>
        public OpenDrawers IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private OpenDrawers _IsDrawersOpen = new();

        /// <summary>
        /// IsPause
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause;

        /// <summary>
        /// Mqtt服务器配置
        /// </summary>
        public MqttServerConfig MqttServerConfig { get => _MqttServerConfig; set => SetProperty(ref _MqttServerConfig, value); }
        private MqttServerConfig _MqttServerConfig = new();

        /// <summary>
        /// MqttUsers
        /// </summary>
        public ObservableCollection<MqttUser> MqttUsers { get => _MqttUsers; set => SetProperty(ref _MqttUsers, value); }
        private ObservableCollection<MqttUser> _MqttUsers = new();
        #endregion


        #region **************************************** 命令 ****************************************
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        ///// <summary>
        ///// 取消订阅主题
        ///// </summary>
        public DelegateCommand<MqttSubedTopic> UnsubscribeTopicCommand { get; private set; }

        /// <summary>
        /// 打开json格式化界面
        /// </summary>
        public DelegateCommand<MessageData> OpenJsonDataViewCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Clear": Clear(); break;
                case "Pause": Pause(); break;
                case "OpenLeftDrawer": IsDrawersOpen.LeftDrawer = true; break;
                case "OpenRightDrawer": IsDrawersOpen.RightDrawer = true; break;
                case "OpenDialogView": OpenDialogView(); break;
                case "RunMqttServer": RunMqttServer(); break;                              //打开服务器
                case "StopMqttServer": StopMqttServer(); break;                            //关闭服务器
                case "ImportConfig": ImportConfig(); break;
                case "ExportConfig": ExportConfig(); break;
                default: break;
            }
        }


        #region Mqtt服务器事件
        /// <summary>
        /// 启动Mqtt服务器
        /// </summary>
        private async void RunMqttServer()
        {
            try
            {
                var mqttFactory = new MqttFactory();
                var mqttServerOptions =
                    new MqttServerOptionsBuilder()
                    .WithDefaultEndpoint()
                    .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(MqttServerConfig.ServerIp))//设置MQTT服务器的IP
                    .WithDefaultEndpointPort(1883)//设置服务器的端口
                    .Build();
                mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
                mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;//客户端连接时验证
                mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;//客户端连接成功后
                mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;//客户端断开连接
                mqttServer.ClientSubscribedTopicAsync += MqttServer_ClientSubscribedTopicAsync;//客户端订阅主题
                mqttServer.ClientUnsubscribedTopicAsync += MqttServer_ClientUnsubscribedTopicAsync;//客户端取消订阅
                mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;//拦截用户发布的消息
                mqttServer.ClientAcknowledgedPublishPacketAsync += MqttServer_ClientAcknowledgedPublishPacketAsync;//

                mqttServer.InterceptingClientEnqueueAsync += MqttServer_InterceptingClientEnqueueAsync;
                mqttServer.ApplicationMessageNotConsumedAsync += MqttServer_ApplicationMessageNotConsumedAsync;
                await mqttServer.StartAsync();//启动

                MqttServerConfig.IsOpened = true;
                ShowMessage($"IP: {MqttServerConfig.ServerIp}  Port:{MqttServerConfig.ServerPort}  服务器开启成功");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }


        /// <summary>
        /// 关闭Mqtt服务器
        /// </summary>
        private async void StopMqttServer()
        {
            try
            {
                //关闭服务器
                await mqttServer.StopAsync();

                MqttServerConfig.IsOpened = false;
                MqttUsers.Clear();//清除已登录的用户列表
                ShowMessage($"Mqtt服务器关闭");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }


        /// <summary>
        /// Mqtt服务器发布消息
        /// </summary>
        /// <returns></returns>
        public async Task Publish_Message_From_Broker()
        {
            var message = new MqttApplicationMessageBuilder().WithTopic("HelloWorld").WithPayload("Test").Build();
            await mqttServer.InjectApplicationMessage(
                new InjectedMqttApplicationMessage(message)
                {
                    SenderClientId = "SenderClientId"
                });
        }


        /// <summary>
        /// 强制踢客户端
        /// </summary>
        /// <returns></returns>
        public async Task Force_Disconnecting_Client()
        {
            var affectedClient = (await mqttServer.GetClientsAsync()).FirstOrDefault(c => c.Id == "MyClient");
            if (affectedClient != null)
            {
                await affectedClient.DisconnectAsync();
            }
        }

        /// <summary>
        /// 拦截用户发布的消息
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            if (arg == null) 
                return Task.CompletedTask;
            try
            {
                //更新用户最新接收消息的时间
                var user = MqttUsers.FirstOrDefault(x => x.ClientId.Equals(arg.ClientId));
                if (user is not null)
                    user.LastDataTime = DateTime.Now;

                //若暂停更新接收数据 则不显示
                if (IsPause)
                    return Task.CompletedTask;


                //处理接收的消息
                if (arg.ApplicationMessage.PayloadSegment.Array == null)//若接收的数据为空则
                {
                    ShowReceiveMessage($"", $"主题：{arg.ApplicationMessage.Topic}");
                    return Task.CompletedTask;
                }
                else
                {
                    var payload = arg.ApplicationMessage.PayloadSegment.ToArray();
                    //var payload = arg.ApplicationMessage.Payload ?? Array.Empty<byte>();

                    switch (MqttServerConfig.ReceivePaylodType)
                    {
                        case MqttPayloadType.Json:
                        case MqttPayloadType.Plaintext:
                            //接收的数据以UTF8解码
                            ShowReceiveMessage($"{Encoding.UTF8.GetString(payload)}", $"主题:{arg.ApplicationMessage.Topic}");
                            break;
                        //case MqttPayloadType.Json:
                        //    ShowReceiveMessage($"{Encoding.UTF8.GetString(payload).ToJsonString()}", $"主题:{arg.ApplicationMessage.Topic}");
                        //    break;
                        case MqttPayloadType.Hex:
                            //接收的数据以16进制字符串解码
                            ShowReceiveMessage($"{BitConverter.ToString(payload).Replace("-", "").InsertFormat(4, " ")}", $"主题:{arg.ApplicationMessage.Topic}");
                            break;
                        case MqttPayloadType.Base64:
                            ShowReceiveMessage($"{Convert.ToBase64String(payload)}", $"主题:{arg.ApplicationMessage.Topic}");
                            break;
                    }
                }
                return CompletedTask.Instance;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("接收数据解析错误 (请尝试更换数据解析格式) : " + ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 客户端订阅事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttServer_ClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs arg)
        {
            try
            {
                //客户端订阅事件
                if (arg == null)
                    return Task.CompletedTask;
                //查找客户端列表
                var x = MqttUsers.FirstOrDefault(x => x.ClientId.Equals(arg.ClientId));
                if (x != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        //添加该主题
                        x.MqttSubedTopics.Add(new MqttSubedTopic { Parent = x, Topic = arg.TopicFilter.Topic });
                    });
                }

                ShowMessage($"客户端：“{arg.ClientId}” 订阅主题：“{arg.TopicFilter.Topic}”");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 客户端取消订阅
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttServer_ClientUnsubscribedTopicAsync(ClientUnsubscribedTopicEventArgs arg)
        {
            try
            {
                ShowMessage($"客户端：“{arg.ClientId}” 取消订阅主题：“{arg.TopicFilter}”");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接用户验证
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
        {
            try
            {
                //验证客户端ID有效性
                if (string.IsNullOrWhiteSpace(arg.ClientId))
                {
                    arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    ShowErrorMessage($"用户名：“{arg.UserName}” 客户端ID：“{arg.ClientId}” 客户端ID无效!");
                    return Task.CompletedTask;
                }

                //验证账号密码
                bool acceptflag = !(string.IsNullOrWhiteSpace(arg.UserName) || string.IsNullOrWhiteSpace(arg.Password));
                //验证失败
                if (!acceptflag)
                {
                    arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    ShowErrorMessage($"用户名：“{arg.UserName}”  客户端ID：“{arg.ClientId}” 请求登录验证失败!");
                    return Task.CompletedTask;
                }
                arg.ReasonCode = MqttConnectReasonCode.Success;                //验证成功
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"客户端验证登录事件异常:{ex.Message}");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttServer_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            try
            {
                var user = MqttUsers.FirstOrDefault(t => t.ClientId == arg.ClientId);
                if (user != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MqttUsers.Remove(user);
                    });
                    ShowMessage($"用户名：“{user.UserName}”  客户端ID：“{arg.ClientId}” 已断开连接!");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"客户端断开连接事件处理异常:{ex.Message}");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 客户端连接成功后
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //添加该用户
                    MqttUsers.Add(new MqttUser()
                    {
                        ClientId = arg.ClientId,
                        UserName = arg.UserName,
                        //PassWord = arg.Password,
                        LoginTime = DateTime.Now,
                        LastDataTime = DateTime.Now
                    });
                });
                ShowMessage($"用户名：“{arg.UserName}”  客户端ID：“{arg.ClientId}” 已连接!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"客户端验证登录事件异常:{ex.Message}");
            }
            return Task.CompletedTask;
        }

        private Task MqttServer_ApplicationMessageNotConsumedAsync(ApplicationMessageNotConsumedEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task MqttServer_ClientAcknowledgedPublishPacketAsync(ClientAcknowledgedPublishPacketEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task MqttServer_InterceptingClientEnqueueAsync(InterceptingClientApplicationMessageEnqueueEventArgs arg)
        {
            return Task.CompletedTask;
        }
        #endregion






        #region 配置文件
        /// <summary>
        /// 导出配置文件
        /// </summary>
        private void ExportConfig()
        {
            try
            {
                string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttServerConfig"); //配置文件目录
                Wu.Utils.IoUtil.Exists(dict);                                                                   //验证文件夹是否存在, 不存在则创建
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    Title = "请选择导出配置文件...",                             //对话框标题
                    Filter = "json files(*.jsonMSC)|*.jsonMSC",                 //文件格式过滤器
                    FilterIndex = 1,                                            //默认选中的过滤器
                    FileName = "Default",                                       //默认文件名
                    DefaultExt = "jsonMSC",                                     //默认扩展名
                    InitialDirectory = dict,                                    //指定初始的目录
                    OverwritePrompt = true,                                     //文件已存在警告
                    AddExtension = true,                                        //若用户省略扩展名将自动添加扩展名
                };
                if (sfd.ShowDialog() != true)
                    return;
                var content = JsonConvert.SerializeObject(MqttServerConfig);    //将当前的配置序列化为json字符串
                Shared.Common.Utils.WriteJsonFile(sfd.FileName, content);              //保存文件
                HcGrowlExtensions.Success("配置文件导出成功", MqttServerView.ViewName);
            }
            catch (Exception ex)
            {
                HcGrowlExtensions.Warning("配置文件导出失败", MqttServerView.ViewName);
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
                string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttServerConfig");
                Wu.Utils.IoUtil.Exists(dict);
                //选中配置文件
                OpenFileDialog dlg = new()
                {
                    Title = "请选择导入配置文件...",                                              //对话框标题
                    Filter = "json files(*.jsonMSC)|*.jsonMSC",    //文件格式过滤器
                    FilterIndex = 1,                                                         //默认选中的过滤器
                    InitialDirectory = dict
                };

                if (dlg.ShowDialog() != true)
                    return;
                var xx = Shared.Common.Utils.ReadJsonFile(dlg.FileName);
                var x = JsonConvert.DeserializeObject<MqttServerConfig>(xx);
                MqttServerConfig = x;
                HcGrowlExtensions.Success("配置文件导入成功", MqttServerView.ViewName);
            }
            catch (Exception ex)
            {
                HcGrowlExtensions.Warning("配置文件导入失败", MqttServerView.ViewName);
                ShowErrorMessage(ex.Message);
            }
        }
        #endregion


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
        /// 清空页面消息
        /// </summary>
        private void Clear()
        {
            try
            {
                Messages.Clear();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }


        /// <summary>
        /// 导航至该页面触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {

        }


        /// <summary>
        /// 打开该弹窗时执行
        /// </summary>
        public void OnDialogOpened(IDialogParameters parameters)
        {

        }


        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
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
            //若窗口处于打开状态则关闭
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
        }

        /// <summary>
        /// 弹窗
        /// </summary>
        private void OpenDialogView()
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

        protected void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);
        protected void ShowReceiveMessage(string message, string title = "") => ShowMessage(message, MessageType.Receive, title);
        protected void ShowSendMessage(string message) => ShowMessage(message, MessageType.Send);


        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        protected void ShowMessage(string message, MessageType type = MessageType.Info, string title = "")
        {
            try
            {
                void action()
                {
                    Messages.Add(new MessageData($"{message}", DateTime.Now, type, title));
                    //log.Info(message);
                    while (Messages.Count > 100)
                    {
                        Messages.RemoveAt(0);
                    }
                }
                Wu.Wpf.Utils.ExecuteFun(action);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        public void PublishMessage()
        {
            //该方法发布至所有客户端
            //foreach (var user in MqttUsers)
            //{
            //    server.PublishAsync(new MqttApplicationMessage()
            //    {
            //        Topic = user.ClientId,                                                              //发布消息时使用的客户端ID
            //        QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce,     //消息质量
            //        Retain = false,
            //        Payload = Encoding.UTF8.GetBytes("发布的消息")                                    //内容
            //    });
            //}
        }

        /// <summary>
        /// 打开json格式化界面
        /// </summary>
        /// <param name="obj"></param>
        private async void OpenJsonDataView(MessageData obj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(obj.Content))
                {
                    HcGrowlExtensions.Warning("无法进行Json格式化...", MqttServerView.ViewName);
                    return;
                }
                try
                {
                    var xx = JsonConvert.DeserializeObject(obj.Content);
                }
                catch (Exception ex)
                {
                    HcGrowlExtensions.Warning("无法进行Json格式化...", MqttServerView.ViewName);
                    return;
                }

                if (obj.Type.Equals(MessageType.Send) || obj.Type.Equals(MessageType.Receive))
                {
                    DialogParameters param = new()
                        {
                            { "Value", obj }
                        };
                    var dialogResult = await dialogHost.ShowDialog("JsonDataView", param, DialogHostName);
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
