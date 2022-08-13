using DryIoc;
using HandyControl.Controls;
using MaterialDesignThemes.Wpf;
using MQTTnet;
using MQTTnet.Server;
using MqttnetServer.Model;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Wu.CommTool.Common;
using Wu.CommTool.Extensions;
using Wu.CommTool.Models;


namespace Wu.CommTool.ViewModels
{
    public class MqttServerViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        public string DialogHostName { get; set; } = "MqttServerView";
        private IMqttServer server;                                 //Mqtt服务器
        //private List<MqttUser> Users = new List<MqttUser>();     //用户列表

        #endregion

        public MqttServerViewModel() { }
        public MqttServerViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;

            ExecuteCommand = new(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            TestCommand = new DelegateCommand<object>(Test);
            CancleClientSubTopicCommand = new DelegateCommand<MqttSubedTopic>(CancleClientSubTopic);
        }

        /// <summary>
        /// 取消客户端订阅
        /// </summary>
        /// <param name="obj"></param>
        private void CancleClientSubTopic(MqttSubedTopic obj)
        {
            try
            {
                server.UnsubscribeAsync(obj.Parent.ClientId, obj.Topic);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void Test(object obj)
        {

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
        public IsDrawersOpen IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private IsDrawersOpen _IsDrawersOpen = new();

        /// <summary>
        /// Mqtt服务器配置
        /// </summary>
        public MqttServerConfig MqttServerConfig { get => _MqttServerConfig; set => SetProperty(ref _MqttServerConfig, value); }
        private MqttServerConfig _MqttServerConfig = new();

        /// <summary>
        /// IsPause
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause;


        /// <summary>
        /// MqttUsers
        /// </summary>
        public ObservableCollection<MqttUser> MqttUsers { get => _MqttUsers; set => SetProperty(ref _MqttUsers, value); }
        private ObservableCollection<MqttUser> _MqttUsers = new();

        /// <summary>
        /// 字符串格式化
        /// </summary>
        public int Format { get => _Format; set => SetProperty(ref _Format, value); }
        private int _Format = 0;
        #endregion


        #region **************************************** 命令 ****************************************
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        /// <summary>
        /// definity
        /// </summary>
        public DelegateCommand<object> TestCommand { get; private set; }


        /// <summary>
        /// definity
        /// </summary>
        public DelegateCommand<MqttSubedTopic> CancleClientSubTopicCommand { get; private set; }

        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": Search(); break;
                case "Clear": Clear(); break;
                case "Pause": Pause(); break;
                case "OpenMqttServer": OpenMqttServer(); break;                              //打开服务器
                case "CloseMqttServer": CloseMqttServer(); break;                              //打开服务器
                case "OpenLeftDrawer": IsDrawersOpen.IsLeftDrawerOpen = true; break;
                case "OpenRightDrawer": IsDrawersOpen.IsRightDrawerOpen = true; break;
                case "OpenDialogView": OpenDialogView(); break;
                case "Format":
                    if (Format == 0)
                    {
                        Format = 1;
                        ShowMessage("接收数据以Json格式化");
                    }
                    else
                    {
                        Format = 0;
                        ShowMessage("接收数据以字符串显示");
                    }
                    break;
                default: break;
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
        /// 关闭Mqtt服务器
        /// </summary>
        private async void CloseMqttServer()
        {
            try
            {
                //关闭服务器
                await server.StopAsync();
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
        /// 打开Mqtt服务器
        /// </summary>
        private async void OpenMqttServer()
        {
            //TODO 打开服务器
            try
            {
                //if (string.IsNullOrWhiteSpace(MqttServerConfig.ServerIp) || MqttServerConfig.ServerPort<=0)
                //{
                //    ShowErrorMessage("xxx");
                //    return;
                //}
                //Mqtt服务器设置
                var optionBuilder = new MqttServerOptionsBuilder()
                    .WithClientId("server")                                                     //设置服务端发布消息时使用的ClientId
                    .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(MqttServerConfig.ServerIp))    //使用指定的Ip地址
                    .WithDefaultEndpointPort(MqttServerConfig.ServerPort)                             //使用指定的端口号
                    .WithConnectionValidator(LoginVerify)                                              //客户端登录验证事件
                    .WithSubscriptionInterceptor(ClientSubscription)                                  //客户端订阅事件
                                                                                                      //.WithUnsubscriptionInterceptor(ClientUnsubscription)
                    .WithApplicationMessageInterceptor(ServerReceived);                               //接收数据处理方法

                //创建服务器
                server = new MqttFactory().CreateMqttServer();

                //客户端断开连接处理
                server.UseClientDisconnectedHandler(c =>
                {
                    try
                    {
                        var user = MqttUsers.FirstOrDefault(t => t.ClientId == c.ClientId);
                        if (user != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                MqttUsers.Remove(user);
                            });
                            ShowMessage($"订阅者：“{user.UserName}”  客户端：“{c.ClientId}” 已断开连接!");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"客户端断开连接事件处理异常:{ex.Message}");
                    }
                });

                //客户端取消订阅主题
                server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(ClientUnsubscribedTopicHandler);

                //开启服务器
                await server.StartAsync(optionBuilder.Build());

                MqttServerConfig.IsOpened = true;
                ShowMessage($"IP: {MqttServerConfig.ServerIp}  Port:{MqttServerConfig.ServerPort}  服务器开启成功");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageType.Error);
            }
        }


        /// <summary>
        /// 客户端取消订阅主题
        /// </summary>
        /// <param name="obj"></param>
        private void ClientUnsubscribedTopicHandler(MqttServerClientUnsubscribedTopicEventArgs obj)
        {
            try
            {
                ShowMessage($"客户端：“{obj.ClientId}” 取消订阅主题：“{obj.TopicFilter}”");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }


        /// <summary>
        /// 客户端登录验证
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LoginVerify(MqttConnectionValidatorContext obj)
        {
            try
            {
                //登录验证器
                bool flag = (obj.Username != "" && obj.Password != "");                         //验证账号密码
                if (!flag)                                                                      //验证失败
                {
                    obj.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }
                obj.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;                //验证成功

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    //添加该用户
                    MqttUsers.Add(new MqttUser()
                    {
                        ClientId = obj.ClientId,
                        UserName = obj.Username,
                        PassWord = obj.Password,
                        LoginTime = DateTime.Now,
                        LastDataTime = DateTime.Now
                    });
                });
                ShowMessage($"用户名：“{obj.Username}”  客户端ID：“{obj.ClientId}” 已连接!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"客户端验证登录事件异常:{ex.Message}");
            }
        }

        /// <summary>
        /// 接收到消息事件
        /// </summary>
        /// <param name="obj"></param>
        private void ServerReceived(MqttApplicationMessageInterceptorContext obj)
        {
            if (obj == null) return;
            try
            {
                obj.AcceptPublish = true;

                //更新用户最新接收消息的时间
                var user = MqttUsers.FirstOrDefault(x => x.ClientId.Equals(obj.ClientId));
                if (user is not null)
                    user.LastDataTime = DateTime.Now;
                //接收的数据以字节显示
                //ShowMessage($"客户端：“{obj.ClientId}” 发布主题：“{obj.ApplicationMessage.Topic}”\r\n内容：\r\n“{(obj.ApplicationMessage?.Payload == null ? null : BitConverter.ToString(obj.ApplicationMessage.Payload))}”");
                //接收的数据以UTF8解码

                //若暂停更新接收数据 则不显示
                if (IsPause)
                    return;

                if (Format == 0)
                {
                    ShowMessage($"客户端：{obj.ClientId}    发布主题：{obj.ApplicationMessage?.Topic}\r\n{(obj.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(obj.ApplicationMessage.Payload))}", MessageType.Receive);
                }
                else
                {
                    //Newtonsoft.Json.JsonConvert.SerializeObject(Encoding.UTF8.GetString(obj.ApplicationMessage.Payload));

                    ShowMessage($"客户端：{obj.ClientId}    发布主题：{obj.ApplicationMessage?.Topic}\r\n" +
                        $"{(obj.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(obj.ApplicationMessage.Payload).ToJsonString())}", MessageType.Receive);

                    //string str = JsonConvert.SerializeObject(Encoding.UTF8.GetString(obj.ApplicationMessage.Payload));
                    //ShowMessage($"客户端：{obj.ClientId}    发布主题：{obj.ApplicationMessage?.Topic}\r\n" +
                    //    $"{(obj.ApplicationMessage?.Payload == null ? null : str)}", MessageType.Receive);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 客户端订阅消息
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ClientSubscription(MqttSubscriptionInterceptorContext obj)
        {
            try
            {
                //客户端订阅事件
                if (obj == null)
                    return;
                //查找客户端列表
                var x = MqttUsers.FirstOrDefault(x => x.ClientId.Equals(obj.ClientId));
                if (x != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        //添加该主题
                        x.MqttSubedTopics.Add(new MqttSubedTopic { Parent = x, Topic = obj.TopicFilter.Topic });
                    });
                }

                //允许订阅
                obj.AcceptSubscription = true;

                ShowMessage($"客户端：“{obj.ClientId}” 订阅主题：“{obj.TopicFilter.Topic}”");
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
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
                Wu.Wpf.Common.Utils.ExecuteFun(action);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        public void PublishMessage()
        {
            //该方法发布至所有客户端
            foreach (var user in MqttUsers)
            {
                server.PublishAsync(new MqttApplicationMessage()
                {
                    Topic = user.ClientId,                                                              //发布消息时使用的客户端ID
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce,     //消息质量
                    Retain = false,
                    Payload = Encoding.UTF8.GetBytes("发布的消息")                                    //内容
                });
            }
        }
        #endregion
    }
}
