using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wu.CommTool.Core.Extensions;
using Wu.CommTool.Modules.MqttClient.Models;
using Wu.CommTool.Shared.Enums;
using Wu.CommTool.Shared.Enums.Mqtt;
using Wu.CommTool.Shared.Eunms;
using Wu.CommTool.Shared.Models;
using Wu.Extensions;
using Wu.ViewModels;
using Wu.Wpf.Common;
using Wu.Wpf.Models;

namespace Wu.CommTool.Modules.MqttClient.ViewModels
{
    public class MqttClientViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        //public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IMqttClient client;
        public string DialogHostName { get; set; } = "MqttClientView";

        private string MqttClientConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttClientConfig");
        private static string viewName = "MqttClientView";
        #endregion

        public MqttClientViewModel() { }
        public MqttClientViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            //log.Info("MqttClient模块加载...");
            this.provider = provider;
            this.dialogHost = dialogHost;

            ExecuteCommand = new(Execute);
            SubTopicCommand = new DelegateCommand<MqttTopic>(SubTopic);
            UnsubscribeTopicCommand = new DelegateCommand<string>(UnsubscribeTopic);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            OpenJsonDataViewCommand = new DelegateCommand<MessageData>(OpenJsonDataView);
            ImportConfigCommand = new DelegateCommand<ConfigFile>(ImportConfig);

            MqttClientConfig.SubscribeTopics.Add(new MqttTopic("+/#"));//默认订阅所有主题

            //从默认配置文件中读取配置
            try
            {
                string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttClientConfig\Default.jsonMCC");
                var xx = Shared.Common.Utils.ReadJsonFile(p);
                var x = JsonConvert.DeserializeObject<MqttClientConfig>(xx);
                if (x == null)
                    return;
                MqttClientConfig = x;
                ShowMessage("读取配置成功");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("配置文件读取失败");
            }

            //读取配置文件夹
            RefreshQuickImportList();
        }


        /// <summary>
        /// 退订主题
        /// </summary>
        /// <param name="obj"></param>
        private void SubTopic(MqttTopic obj)
        {
            try
            {
                var xx = MqttClientConfig.SubscribeTopics.Remove(obj);
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
        /// OpenDrawers
        /// </summary>
        public OpenDrawers IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private OpenDrawers _IsDrawersOpen = new();

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
        public MqttTopic NewSubTopic { get => _NewSubTopic; set => SetProperty(ref _NewSubTopic, value); }
        private MqttTopic _NewSubTopic = new(string.Empty);

        /// <summary>
        /// 主动关闭客户端标志
        /// </summary>
        public bool ManualStopFlag { get => _ManualStopFlag; set => SetProperty(ref _ManualStopFlag, value); }
        private bool _ManualStopFlag;

        /// <summary>
        /// 配置文件列表
        /// </summary>
        public ObservableCollection<ConfigFile> ConfigFiles { get => _ConfigFiles; set => SetProperty(ref _ConfigFiles, value); }
        private ObservableCollection<ConfigFile> _ConfigFiles = new();
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
        public DelegateCommand<MqttTopic> SubTopicCommand { get; private set; }

        /// <summary>
        /// 取消订阅主题
        /// </summary>
        public DelegateCommand<string> UnsubscribeTopicCommand { get; private set; }

        /// <summary>
        /// 打开json格式化界面
        /// </summary>
        public DelegateCommand<MessageData> OpenJsonDataViewCommand { get; private set; }

        /// <summary>
        /// 快速导入配置
        /// </summary>
        public DelegateCommand<ConfigFile> ImportConfigCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public async void Execute(string obj)
        {
            switch (obj)
            {
                case "Open": OpenMqttClient(); break;
                case "Close": CloseMqttClient(); break;
                case "Clear": Clear(); break;
                case "Pause": Pause(); break;
                case "Publish": Publish(); break;
                case "AddTopic": AddTopic(); break;                               //添加订阅的主题
                case "SubscribeTopic": await SubscribeTopic(NewSubTopic.Topic); break;                               //添加订阅的主题
                case "OpenLeftDrawer": IsDrawersOpen.LeftDrawer = true; break;
                case "OpenRightDrawer": IsDrawersOpen.RightDrawer = true; break;
                case "OpenDialogView": OpenDialogView(); break;
                case "ImportConfig": ImportConfig(); break;
                case "ExportConfig": ExportConfig(); break;
                case "RefreshQuickImportList": RefreshQuickImportList(); break;                 //刷新快速导入配置列表
                default: break;
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
                string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttClientConfig");
                Wu.Utils.IOUtil.Exists(dict);
                SaveFileDialog sfd = new()
                {
                    Title = "请选择导出配置文件...",                                              //对话框标题
                    Filter = "json files(*.jsonMCC)|*.jsonMCC",    //文件格式过滤器
                    FilterIndex = 1,                                                         //默认选中的过滤器
                    FileName = "Default",                                           //默认文件名
                    DefaultExt = "jsonMCC",                                     //默认扩展名
                    InitialDirectory = dict,                //指定初始的目录
                    OverwritePrompt = true,                                                  //文件已存在警告
                    AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
                };
                if (sfd.ShowDialog() != true)
                    return;
                //将当前的配置序列化为json字符串
                var content = JsonConvert.SerializeObject(MqttClientConfig);
                //保存文件
                Shared.Common.Utils.WriteJsonFile(sfd.FileName, content);
                HcGrowlExtensions.Success("配置导出完成", viewName);
                RefreshQuickImportList();
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
                string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttClientConfig");
                Wu.Utils.IOUtil.Exists(dict);
                //选中配置文件
                OpenFileDialog dlg = new()
                {
                    Title = "请选择导入配置文件...",                      //对话框标题
                    Filter = "json files(*.jsonMCC)|*.jsonMCC",          //文件格式过滤器
                    FilterIndex = 1,                                     //默认选中的过滤器
                    InitialDirectory = dict
                };

                if (dlg.ShowDialog() != true)
                    return;
                var xx = Shared.Common.Utils.ReadJsonFile(dlg.FileName);
                var x = JsonConvert.DeserializeObject<MqttClientConfig>(xx);
                MqttClientConfig = x;
                HcGrowlExtensions.Success($"配置文件\"{Path.GetFileNameWithoutExtension(dlg.FileName)}\"导入成功", viewName);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 导入配置文件
        /// </summary>
        /// <param name="obj"></param>
        private void ImportConfig(ConfigFile obj)
        {
            try
            {
                var xx = Shared.Common.Utils.ReadJsonFile(obj.FullName);//读取文件
                var x = JsonConvert.DeserializeObject<MqttClientConfig>(xx)!;//反序列化
                if (x == null)
                {
                    ShowErrorMessage("读取配置文件失败");
                    return;
                }
                MqttClientConfig = x;
                //ShowMessage("导入配置完成");
                HcGrowlExtensions.Success($"配置文件\"{Path.GetFileNameWithoutExtension(obj.FullName)}\"导入成功", viewName);
            }
            catch (Exception ex)
            {
                HcGrowlExtensions.Warning($"配置文件导入失败", viewName);
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 更新快速导入配置列表
        /// </summary>
        private void RefreshQuickImportList()
        {
            try
            {
                DirectoryInfo Folder = new DirectoryInfo(MqttClientConfigDict);
                var a = Folder.GetFiles().Where(x => x.Extension.ToLower().Equals(".jsonmcc")).Select(item => new ConfigFile(item));
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
        /// 添加订阅的主题
        /// </summary>
        private void AddTopic()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewSubTopic.Topic))
                {
                    //不能订阅空主题
                    return;
                }
                //若重复了则不执行操作
                if (MqttClientConfig.SubscribeTopics.FirstOrDefault(x => x.Topic.Equals(NewSubTopic.Topic.Trim())) != null)
                    return;
                MqttClientConfig.SubscribeTopics.Add(new MqttTopic(NewSubTopic.Topic.Trim()));
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
            //try
            //{
            //    if (MqttClientConfig.IsOpened.Equals(false) || client.IsConnected.Equals(false))
            //    {
            //        var re = await OpenMqttClient();
            //        if (re.Equals(false))
            //        {
            //            return;
            //        }
            //    }

            //    //根据选择的消息质量进行设置
            //    var mqttAMB = new MqttApplicationMessageBuilder();

            //    //根据设置的消息质量发布消息
            //    switch (MqttClientConfig.QosLevel)
            //    {
            //        case QosLevel.AtLeastOnce:
            //            mqttAMB.WithAtLeastOnceQoS();
            //            break;
            //        case QosLevel.AtMostOnce:
            //            mqttAMB.WithAtMostOnceQoS();
            //            break;
            //        case QosLevel.ExactlyOnce:
            //            mqttAMB.WithExactlyOnceQoS();
            //            break;
            //        default:
            //            break;
            //    }


            //    switch (MqttClientConfig.SendPaylodType)
            //    {
            //        case MqttPayloadType.Plaintext:
            //            mqttAMB.WithPayload(SendMessage);
            //            break;
            //        case MqttPayloadType.Base64:
            //            mqttAMB.WithPayload(Convert.ToBase64String(Encoding.Default.GetBytes(SendMessage)));
            //            break;
            //        case MqttPayloadType.Json:
            //            mqttAMB.WithPayload(SendMessage.ToJsonString());
            //            break;
            //        case MqttPayloadType.Hex:
            //            mqttAMB.WithPayload(StringExtention.GetBytes(SendMessage.Replace(" ", string.Empty)));
            //            break;
            //    }

            //    var mam = mqttAMB.WithTopic(MqttClientConfig.PublishTopic)                  //发布的主题
            //    //.WithPayload(SendMessage)
            //    //.WithExactlyOnceQoS()
            //    .WithRetainFlag()
            //    .Build();

            //    //发布
            //    var result = await client.PublishAsync(mam, CancellationToken.None);
            //    ShowSendMessage($"{SendMessage}", $"主题：{MqttClientConfig.PublishTopic}");
            //}
            //catch (Exception ex)
            //{
            //    ShowErrorMessage($"发送失败:{ex.Message}");
            //}
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
                ManualStopFlag = true;//主动关闭客户端
                if (client.IsConnected)
                {
                    ShowMessage($"断开连接...");
                }
                else
                {
                    ShowMessage($"取消连接...");
                }
                await client.DisconnectAsync();          //断开连接
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MqttClientConfig.IsOpened = false;
                });

            }
            catch (Exception ex)
            {
                ShowMessage($"断开连接失败 {ex.Message}");
            }
        }

        /// <summary>
        /// 打开Mqtt客户端
        /// </summary>
        private async Task<bool> OpenMqttClient()
        {
            try
            {
                if (MqttClientConfig.IsOpened)
                {
                    return false;
                }
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(MqttClientConfig.ServerIp, MqttClientConfig.ServerPort)                  //服务器IP和端口
                    .WithClientId(MqttClientConfig.ClientId)                                                //客户端ID
                    .WithCredentials(MqttClientConfig.UserName, MqttClientConfig.Password).Build();         //账号

                client = new MqttFactory().CreateMqttClient();
                client.ConnectedAsync += Client_ConnectedAsync;
                client.DisconnectedAsync += Client_DisconnectedAsync;
                client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
                client.InspectPacketAsync += Client_InspectPacketAsync;
                client.ConnectingAsync += Client_ConnectingAsync;

                ShowMessage("连接中...");
                MqttClientConfig.IsOpened = true;//连接时需要直接至位, 否则重复连接期间重复点击连接将导致异常
                await client.ConnectAsync(options);                //启动连接
                return true;
            }
            catch (Exception ex)
            {
                return false;
                //ShowErrorMessage($"{ex.Message}");
            }
        }

        private Task Client_ConnectingAsync(MqttClientConnectingEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task Client_InspectPacketAsync(MQTTnet.Diagnostics.InspectMqttPacketEventArgs arg)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 客户端接收事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                if (IsPause)
                {
                    return Task.CompletedTask;
                }

                var payload = arg.ApplicationMessage.PayloadSegment.ToArray();
                switch (MqttClientConfig.ReceivePaylodType)
                {
                    case MqttPayloadType.Json:
                    case MqttPayloadType.Plaintext:
                        ShowReceiveMessage($"{Encoding.UTF8.GetString(payload)}", $"主题：{arg.ApplicationMessage.Topic}");
                        break;
                    //case MqttPayloadType.Json:
                    //    ShowReceiveMessage($"{Encoding.UTF8.GetString(payload).ToJsonString()}", $"主题：{arg.ApplicationMessage.Topic}");
                    //    break;
                    case MqttPayloadType.Hex:
                        ShowReceiveMessage($"{BitConverter.ToString(payload).Replace("-", "").InsertFormat(4, " ")}", $"主题：{arg.ApplicationMessage.Topic}");
                        break;
                    case MqttPayloadType.Base64:
                        ShowReceiveMessage($"{Convert.ToBase64String(payload)}", $"主题：{arg.ApplicationMessage.Topic}");
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("接收数据解析错误 (请尝试更换数据解析格式) : " + ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 掉线或主动离线都会调用该方法
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            if (arg == null)
                return Task.CompletedTask;
            //异常导致的掉线
            if (arg.Exception != null)
            {
                MqttClientConfig.IsOpened = false;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowErrorMessage(arg.Exception.Message.ToString());
                    //ShowErrorMessage("已断开连接");
                });
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                //非主动离线则显示故障消息
                if (!ManualStopFlag)
                    ShowErrorMessage("已断开连接");
                ManualStopFlag = false;                         //复位主动离线标志
                MqttClientConfig.IsOpened = false;              //修改连接状态
                //清除订阅成功的主题
                MqttClientConfig.SubscribeSucceeds.Clear();
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task Client_ConnectedAsync(MqttClientConnectedEventArgs arg)
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
                    //await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(x).Build());       //订阅服务端消息
                    //ShowMessage($"已订阅主题: {x}");
                    await SubscribeTopic(x.Topic);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"订阅失败: {ex.Message}");//连接成功 
            }
            return;
        }



        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <returns></returns>
        private async Task SubscribeTopic(string topic)
        {
            try
            {
                //若已订阅则返回
                if (MqttClientConfig.SubscribeSucceeds.Contains(topic))
                {
                    return;
                }
                if (client is null || client.IsConnected == false)
                {
                    ShowErrorMessage("未连接服务器");
                    return;
                }

                var result = await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());       //订阅服务端消息
                //根据结果判断
                ShowMessage($"已订阅主题: {topic}");
                //订阅成功 添加进订阅成功的列表
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MqttClientConfig.SubscribeSucceeds.Add(topic);
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 取消订阅订阅主题
        /// </summary>
        /// <returns></returns>
        private async void UnsubscribeTopic(string topic)
        {
            try
            {
                //取消订阅主题
                var result = await client.UnsubscribeAsync(topic);
                //根据结果判断
                ShowMessage($"已取消订阅主题: {topic}");
                //取消订阅成功 从订阅成功的列表移除
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MqttClientConfig.SubscribeSucceeds.Remove(topic);
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }





        protected void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);
        protected void ShowReceiveMessage(string message, string title = "") => ShowMessage(message, MessageType.Receive, title);
        protected void ShowSendMessage(string message, string title = "") => ShowMessage(message, MessageType.Send, title);

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
        /// 导航至该页面触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {

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
        /// 打开json格式化界面
        /// </summary>
        /// <param name="obj"></param>
        private async void OpenJsonDataView(MessageData obj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(obj.Content))
                {
                    HcGrowlExtensions.Warning("无法进行Json格式化...", viewName);
                    return;
                }
                try
                {
                    var xx = JsonConvert.DeserializeObject(obj.Content);
                }
                catch (Exception ex)
                {
                    HcGrowlExtensions.Warning("无法进行Json格式化...", viewName);
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
