using HandyControl.Controls;
using MaterialDesignThemes.Wpf;
using MQTTnet;
using MQTTnet.Server;
using MqttnetServer.Model;
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
        public string DialogHostName { get; set; }
        private IMqttServer server;                                 //Mqtt服务器
        private List<MqttUser> users = new List<MqttUser>();     //用户列表

        #endregion

        public MqttServerViewModel() { }
        public MqttServerViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;

            ExecuteCommand = new(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
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
        private MqttServerConfig _MqttServerConfig =new();

        /// <summary>
        /// IsPause
        /// </summary>
        public bool IsPause { get => _IsPause; set => SetProperty(ref _IsPause, value); }
        private bool _IsPause;
        #endregion


        #region **************************************** 命令 ****************************************
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

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
                case "Search": Search(); break;
                case "Clear": Clear(); break;
                case "OpenMqttServer": OpenMqttServer(); break;                              //打开服务器
                case "CloseMqttServer": CloseMqttServer(); break;                              //打开服务器
                case "OpenLeftDrawer": IsDrawersOpen.IsLeftDrawerOpen = true; break;
                case "OpenDialogView": OpenDialogView(); break;
                default: break;
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
                //Mqtt服务器设置
                var optionBuilder = new MqttServerOptionsBuilder()
                    .WithClientId("server")                                                     //设置服务端发布消息时使用的ClientId
                    .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(MqttServerConfig.ServerIp))    //使用指定的Ip地址
                    .WithDefaultEndpointPort(MqttServerConfig.ServerPort)                             //使用指定的端口号
                    .WithConnectionValidator(LoginVerify)                                              //客户端登录验证事件
                    .WithSubscriptionInterceptor(ClientSubscription)                                  //客户端订阅事件
                    .WithApplicationMessageInterceptor(ServerReceived);                               //接收数据处理方法

                //创建服务器
                server = new MqttFactory().CreateMqttServer();
               
                //客户端断开连接处理
                server.UseClientDisconnectedHandler(c =>
                {
                    var user = users.FirstOrDefault(t => t.ClientId == c.ClientId);
                    if (user != null)
                    {
                        users.Remove(user);
                        ShowMessage($"订阅者：“{user.UserName}”  客户端：“{c.ClientId}” 已断开连接!");
                    }
                });

                //开启服务器
                await server.StartAsync(optionBuilder.Build());

                MqttServerConfig.IsOpened = true;
                ShowMessage("服务器开启成功");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message,MessageType.Error);
            }

        }

        /// <summary>
        /// 接收到消息事件
        /// </summary>
        /// <param name="obj"></param>
        private void ServerReceived(MqttApplicationMessageInterceptorContext obj)
        {
            
        }

        /// <summary>
        /// 客户端发布消息
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ClientSubscription(MqttSubscriptionInterceptorContext obj)
        {
            
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LoginVerify(MqttConnectionValidatorContext obj)
        {
            
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

        /// <summary>
        /// 界面显示数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        private void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            try
            {
                Messages.Add(new MessageData($"{message}", DateTime.Now, type));
            }
            catch (Exception ex) { }
        }
        #endregion
    }
}
