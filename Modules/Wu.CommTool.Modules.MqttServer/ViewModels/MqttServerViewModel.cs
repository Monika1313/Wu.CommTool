namespace Wu.CommTool.Modules.MqttServer.ViewModels;

public partial class MqttServerViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public static readonly ILog log = LogManager.GetLogger(typeof(MqttServerViewModel));
    private MQTTnet.Server.MqttServer mqttServer;                                 //Mqtt服务器
    private readonly string mqttServerConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MqttServerConfig");
    #endregion

    public MqttServerViewModel() { }
    public MqttServerViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        GetDefaultConfig();
    }

    /// <summary>
    /// 读取默认配置文件 若无则生成
    /// </summary>
    private void GetDefaultConfig()
    {
        //从默认配置文件中读取配置
        try
        {
            var filePath = Path.Combine(mqttServerConfigFolder, @"Default.jsonMSC");
            if (File.Exists(filePath))
            {
                var x = JsonConvert.DeserializeObject<MqttServerConfig>(Core.Common.Utils.ReadJsonFile(filePath));
                if (x != null)
                {
                    MqttServerConfig = x;
                    ShowMessage("读取配置成功");
                }
            }
            else
            {
                //文件不存在则生成默认配置 
                MqttServerConfig = new MqttServerConfig();
                //在默认文件目录生成默认配置文件
                var content = JsonConvert.SerializeObject(MqttServerConfig);       //将当前的配置序列化为json字符串
                Core.Common.Utils.WriteJsonFile(filePath, content);                     //保存文件
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"配置文件读取失败:{ex.Message}");
        }
    }


    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; } = MqttServerView.ViewName;

    /// <summary>
    /// CurrentDto
    /// </summary>
    [ObservableProperty]
    object currentDto = new();

    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MessageData> messages = [];

    /// <summary>
    /// 打开抽屉
    /// </summary>
    [ObservableProperty]
    OpenDrawers isDrawersOpen = new();

    /// <summary>
    /// IsPause
    /// </summary>
    [ObservableProperty]
    bool isPause;

    /// <summary>
    /// Mqtt服务器配置
    /// </summary>
    [ObservableProperty]
    MqttServerConfig mqttServerConfig = new();

    /// <summary>
    /// MqttUsers
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MqttUser> mqttUsers = [];

    /// <summary>
    /// definity
    /// </summary>
    [ObservableProperty]
    string publishMessage = string.Empty;
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
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
            case "AddFwRule": AddFwRule(); break;//添加防火墙规则
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
                .WithDefaultEndpointPort(MqttServerConfig.ServerPort)//设置服务器的端口
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
    [RelayCommand]
    private async Task PublishMessageFromBroker()
    {
        try
        {
            //var message = new MqttApplicationMessageBuilder()
            //    .WithTopic(MqttServerConfig.PublishTopic)
            //    .WithQualityOfServiceLevel(MqttServerConfig.QosLevel)
            //    .WithPayload(PublishMessage).Build();


            //根据选择的消息质量进行设置
            var mqttAMB = new MqttApplicationMessageBuilder();

            //根据设置的消息质量发布消息
            switch (MqttServerConfig.QosLevel)
            {
                case QosLevel.AtLeastOnce:
                    mqttAMB.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                    break;
                case QosLevel.AtMostOnce:
                    mqttAMB.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                    break;
                case QosLevel.ExactlyOnce:
                    mqttAMB.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                    break;
                default:
                    break;
            }

            switch (MqttServerConfig.SendPaylodType)
            {
                case MqttPayloadType.Json:
                case MqttPayloadType.Plaintext:
                    mqttAMB.WithPayload(PublishMessage);
                    break;
                case MqttPayloadType.Base64:
                    mqttAMB.WithPayload(Convert.ToBase64String(Encoding.Default.GetBytes(PublishMessage)));
                    break;
                case MqttPayloadType.Hex:
                    mqttAMB.WithPayload(StringExtention.GetBytes(PublishMessage.Replace(" ", string.Empty)));
                    break;
            }

            //保留消息
            if (MqttServerConfig.IsRetain)
            {
                mqttAMB.WithRetainFlag();
            }
            else
            {
                mqttAMB.WithRetainFlag(false);
            }

            var message = mqttAMB.WithTopic(MqttServerConfig.PublishTopic).Build();

            ShowSendMessage($"{PublishMessage}", $"主题：{MqttServerConfig.PublishTopic}");
            await mqttServer.InjectApplicationMessage(
                new InjectedMqttApplicationMessage(message)
                {
                    SenderClientId = "SenderClientId"
                });
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
            //HcGrowlExtensions.Warning(ex.Message);
        }
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
                        ShowReceiveMessage($"{Encoding.UTF8.GetString(payload)}", $"主题：{arg.ApplicationMessage.Topic}");
                        break;
                    //case MqttPayloadType.Json:
                    //    ShowReceiveMessage($"{Encoding.UTF8.GetString(payload).ToJsonString()}", $"主题:{arg.ApplicationMessage.Topic}");
                    //    break;
                    case MqttPayloadType.Hex:
                        //接收的数据以16进制字符串解码
                        ShowReceiveMessage($"{BitConverter.ToString(payload).Replace("-", "").InsertFormat(4, " ")}", $"主题:{arg.ApplicationMessage.Topic}");
                        break;
                    case MqttPayloadType.Base64:
                        ShowReceiveMessage($"{Convert.ToBase64String(payload)}", $"主题：{arg.ApplicationMessage.Topic}");
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

            #region 验证账号密码
            ////验证账号密码
            //bool acceptflag = !(string.IsNullOrWhiteSpace(arg.UserName) || string.IsNullOrWhiteSpace(arg.Password));
            ////验证失败
            //if (!acceptflag)
            //{
            //    arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            //    ShowErrorMessage($"用户名：“{arg.UserName}”  客户端ID：“{arg.ClientId}” 请求登录验证失败!");
            //    return Task.CompletedTask;
            //} 
            #endregion
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
            Wu.Utils.IoUtil.Exists(mqttServerConfigFolder);                                                                   //验证文件夹是否存在, 不存在则创建
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                  //对话框标题
                Filter = "json files(*.jsonMSC)|*.jsonMSC",    //文件格式过滤器
                FilterIndex = 1,                               //默认选中的过滤器
                FileName = "Default",                          //默认文件名
                DefaultExt = "jsonMSC",                        //默认扩展名
                InitialDirectory = mqttServerConfigFolder,     //指定初始的目录
                OverwritePrompt = true,                        //文件已存在警告
                AddExtension = true,                           //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            var content = JsonConvert.SerializeObject(MqttServerConfig);    //将当前的配置序列化为json字符串
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);              //保存文件
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
            Wu.Utils.IoUtil.Exists(mqttServerConfigFolder);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonMSC)|*.jsonMSC",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                InitialDirectory = mqttServerConfigFolder
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
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

    [RelayCommand]
    void Save()
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

    [RelayCommand]
    void Cancel()
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
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    protected void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);

    protected void ShowReceiveMessage(string message, string title = "")
    {
        try
        {
            void action()
            {
                Messages.Add(new MqttMessageData($"{message}", DateTime.Now, MessageType.Receive, title));
                log.Info($"接收:{message}");
                while (Messages.Count > 100)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFun(action);
        }
        catch (Exception)
        {
        }
    }

    protected void ShowSendMessage(string message, string title = "")
    {
        try
        {
            void action()
            {
                Messages.Add(new MqttMessageData($"{message}", DateTime.Now, MessageType.Send, title));
                log.Info($"发送:{message}");
                while (Messages.Count > 100)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFun(action);
        }
        catch (Exception)
        {
        }
    }


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
                log.Info(message);
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
    public void PublishMessageToAllClient()
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
    [RelayCommand]
    private async Task OpenJsonDataView(MessageData obj)
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
                HcGrowlExtensions.Warning($"无法进行Json格式化...{ex.Message}", MqttServerView.ViewName);
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
            HcGrowlExtensions.Warning(ex.Message);
        }
    }
    #endregion

    async void AddFwRule()
    {
        try
        {
            string ruleName = @"1 Wu.CommTool Rule";
            //获取防火墙列表，查看是否有该应用的规则
            var allRules = FirewallManager.Instance.Rules.ToArray();
            //获取所有与该程序相关的
            var c = allRules.Where(x => x.FriendlyName.Equals(ruleName));
            // 获取当前运行的可执行文件的完整路径
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            //string currentExe = Environment.ProcessPath;
            //判断当前程序是否已存在该规则,若不存在则创建
            if (allRules.Any(x => x.FriendlyName.Equals(ruleName) && x.ApplicationName.Equals(currentExe) && x.Action == FirewallAction.Allow))
            {
                ShowMessage("已为该软件添加过规则,，无需再次添加。");
                return;
            }

            //判断管理员权限
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                //提示获取管理员权限
                var result = await dialogHost.Question("警告", "该操作需要管理员权限,点击确认以管理员权限重启该软件，重启后再使用该功能。", "Root");
                // 如果不是管理员，则重新启动具有管理员权限的应用程序
                if (result.Result != ButtonResult.OK)
                {
                    return;
                }

                var processInfo = new ProcessStartInfo(currentExe)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    // 用户取消了UAC提示或其他错误处理
                    ShowErrorMessage(ex.Message);
                    return;
                }
                Application.Current.Shutdown();
                return;
            }
            #region 开放入站规则




            //应用程序规则
            var rule = FirewallManager.Instance.CreateApplicationRule(
                ruleName,
                FirewallAction.Allow,//允许
                currentExe
            );
            rule.Direction = FirewallDirection.Inbound;//入站规则
            FirewallManager.Instance.Rules.Add(rule);//添加上诉防火墙规则
            ShowMessage("该应用程序的入站规则添加成功");
            #endregion
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"防火墙规则添加失败:{ex.Message}");
        }
    }

    void DeleteFwRule()
    {
        var myRule = FirewallManager.Instance.Rules.SingleOrDefault(r => r.Name == "My Rule");
        if (myRule != null)
        {
            FirewallManager.Instance.Rules.Remove(myRule);
        }
    }
}
