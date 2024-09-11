namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Modbus Tcp 从站=Master
/// </summary>
public partial class MtcpMaster : ObservableObject
{
    private static readonly ILog log = LogManager.GetLogger(typeof(MtcpMaster));
    public MtcpMaster()
    {
        ShowMessage("开发中...");
    }

    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MessageData> messages = [];


    #region ModbusTcp服务器参数
    [ObservableProperty]
    string serverIp = "127.0.0.1";

    [ObservableProperty]
    int serverPort = 502;

    [ObservableProperty]
    int connectTimeout = 3000;

    [ObservableProperty]
    int requestTimeout = 1000;
    #endregion

    #region 属性
    [ObservableProperty]
    ObservableCollection<MtcpCustomFrame> mtcpCustomFrames = [
        new ("00 0A 00 00 00 06 01 03 00 00 00 01 "),
        new ("00 0B 00 00 00 06 01 03 00 00 00 04 "),
        new (""),
    ];
    #endregion

    #region 方法
    [RelayCommand]
    [property: JsonIgnore]
    private void Execute(string cmd)
    {
        switch (cmd)
        {
            case "新增行":
                MtcpCustomFrames.Add(new MtcpCustomFrame());
                break;
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void TestMaster()
    {
        try
        {
            #region modbus tcp 读取保持寄存器测试
            //验证当前TcpClient是否有效并连接成功;
            //var client2 = new TcpClient();

            //using TcpClient client2 = new TcpClient("127.0.0.1", 502);
            //client2.ReceiveTimeout = 1000;
            //client2.SendTimeout = 1000;
            ////client.ConnectAsync(serverIp, serverPort);
            //var factory = new ModbusFactory(logger: new DebugModbusLogger());
            //master = factory.CreateMaster(client2);

            //byte slaveId = 1;
            //byte startAddress = 0;
            //byte numberOfPoints = 5;

            ////请求
            //var request = new ReadHoldingInputRegistersRequest(
            //        ModbusFunctionCodes.ReadHoldingRegisters,
            //        slaveId,
            //        startAddress,
            //        numberOfPoints);

            //var ccc = master.Transport.BuildMessageFrame(request);//生成 读取保持寄存器帧


            //ShowMessage(ccc.ToHexString(), MessageType.Send);//输入16进制的帧

            //var aa = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numberOfPoints);

            //ShowMessage(string.Join(" ", aa), MessageType.Receive);
            #endregion
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }



    MbusTcpClient mbusTcpClient;

    [ObservableProperty]
    bool isOnline;


    /// <summary>
    /// 建立Tcp/Ip连接
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    [property: JsonIgnore]
    async Task Connect()
    {
        try
        {
            //建立TcpIp连接
            mbusTcpClient?.Dispose();
            mbusTcpClient = new MbusTcpClient();
            mbusTcpClient.ClientConnecting += () =>
            {
                ShowMessage($"连接中...");
            };
            mbusTcpClient.ClientConnected += (e) =>
                {
                    IsOnline = true;
                    ShowMessage($"连接服务器成功... {ServerIp}:{ServerPort}");
                };
            mbusTcpClient.ClientDisconnected += (e) =>
                {
                    IsOnline = false;
                    ShowMessage("断开连接...");
                };
            mbusTcpClient.MessageSending += (s) =>
            {
                ShowSendMessage(new MtcpFrame(s));
            };
            mbusTcpClient.MessageReceived += (s) =>
            {
                ShowReceiveMessage(new MtcpFrame(s));
            };
            mbusTcpClient.ErrorOccurred += (s) =>
            {
                ShowErrorMessage(s);
            };
            await mbusTcpClient.ConnectAsync(ServerIp, ServerPort);
        }
        catch (Exception ex)
        {
            IsOnline = mbusTcpClient.Connected;
            ShowErrorMessage($"连接失败...{ex.Message}");
        }
    }

    /// <summary>
    /// 发送自定义帧
    /// </summary>
    /// <param name="mtcpCustomFrame"></param>
    /// <returns></returns>
    [RelayCommand]
    [property: JsonIgnore]
    public async Task SendCustomMessage(MtcpCustomFrame mtcpCustomFrame)
    {
        if (mtcpCustomFrame.Frame.Length.Equals(0))
        {
            ShowErrorMessage("发送消息不能为空...");
            return;
        }
        //若未初始化客户端或未连接,则先连接
        if (mbusTcpClient == null || !mbusTcpClient.Connected)
        {
            await Connect();
        }
        string message = mtcpCustomFrame.Frame.Replace(" ", "");
        if (message.Length % 2 == 1)
        {
            ShowErrorMessage("消息少个字符");
            return;
        }
        mbusTcpClient.SendMessage(mtcpCustomFrame.Frame);
    }


    /// <summary>
    /// 发送消息帧
    /// </summary>
    /// <param name="mtcpFrame"></param>
    /// <returns></returns>
    [RelayCommand]
    [property: JsonIgnore]
    public void SendMessage(MtcpFrame mtcpFrame)
    {
        ////若未初始化客户端或未连接,则先连接
        //if (mbusTcpClient == null || !mbusTcpClient.Connected)
        //{
        //    await Connect();
        //}

        //string message = mtcpFrame.Replace(" ", "");
        //if (message.Length % 2 == 1)
        //{
        //    ShowErrorMessage("消息少个字符");
        //    return;
        //}
        //mbusTcpClient.SendMessage(mtcpFrame.Frame);
    }

    /// <summary>
    /// 断开Tcp连接
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private void DisConnect()
    {
        try
        {
            mbusTcpClient.Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    //删除行
    [RelayCommand]
    [property: JsonIgnore]
    private void DeleteLine(MtcpCustomFrame obj)
    {
        if (MtcpCustomFrames.Count > 1)
        {
            MtcpCustomFrames.Remove(obj);
        }
        else
        {
            ShowErrorMessage("不能删除最后一行...");
        }
    }

    /// <summary>
    /// 复制帧内容
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    [property: JsonIgnore]
    private void CopyFrame(MtcpMessageData obj)
    {
        try
        {
            string re = string.Empty;
            foreach (var item in obj.MtcpSubMessageData)
            {
                re += $"{item.Content} ";
            }
            System.Windows.Clipboard.SetDataObject(re);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }
    #endregion

    #region******************************  页面消息  ******************************
    /// <summary>
    /// 页面显示消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
        try
        {
            void action()
            {
                Messages.Add(new MessageData($"{message}", DateTime.Now, type));
                log.Info(message);
                while (Messages.Count > 260)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
        }
        catch (Exception) { }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    /// <param name="message"></param>
    public void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);

    /// <summary>
    /// 页面展示接收数据消息
    /// </summary>
    /// <param name="frame"></param>
    public void ShowReceiveMessage(MtcpFrame frame)
    {
        try
        {
            void action()
            {
                var msg = new MtcpMessageData("", DateTime.Now, MessageType.Receive, frame);
                Messages.Add(msg);
                log.Info($"接收:{frame}");
                while (Messages.Count > 200)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
        }
        catch (Exception) { }
    }

    /// <summary>
    /// 页面展示发送数据消息
    /// </summary>
    /// <param name="frame"></param>
    public void ShowSendMessage(MtcpFrame frame)
    {
        try
        {
            void action()
            {
                var msg = new MtcpMessageData("", DateTime.Now, MessageType.Send, frame);
                Messages.Add(msg);
                log.Info(message: $"发送:{frame}");
                while (Messages.Count > 200)
                {
                    Messages.RemoveAt(0);
                }
            }
            Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
        }
        catch (System.Exception) { }
    }


    /// <summary>
    /// 清空消息
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    public void MessageClear()
    {
        Messages.Clear();
    }

    /// <summary>
    /// 暂停更新接收的数据
    /// </summary>
    [RelayCommand]
    public void Pause()
    {
        //IsPause = !IsPause;
        //if (IsPause)
        //{
        //    ShowMessage("暂停更新接收的数据");
        //}
        //else
        //{
        //    ShowMessage("恢复更新接收的数据");
        //}
    }
    #endregion
}
