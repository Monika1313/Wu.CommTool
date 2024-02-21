using NModbus.Extensions.Enron;
using System.Collections;

namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Modbus Tcp 从站=Master
/// </summary>
public partial class MtcpMaster : ObservableObject
{
    public MtcpMaster()
    {
        ShowMessage("开发中...");
        ShowMessage("开发中...");
        ShowMessage("开发中...");
    }
    IModbusMaster master;



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

    #region 方法
    [RelayCommand]
    async Task Execute(string cmd)
    {
        switch (cmd)
        {
            case "":
                break;

        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    async Task TestMaster()
    {
        try
        {
            #region modbus tcp 读取保持寄存器测试
            //验证当前TcpClient是否有效并连接成功;
            //var client2 = new TcpClient();

            using TcpClient client2 = new TcpClient("127.0.0.1", 502);
            //client.ConnectAsync(serverIp, serverPort);
            var factory = new ModbusFactory(logger: new DebugModbusLogger());
            master = factory.CreateMaster(client2);

            byte slaveId = 1;
            byte startAddress = 0;
            byte numberOfPoints = 5;

            //请求
            var request = new ReadHoldingInputRegistersRequest(
                    ModbusFunctionCodes.ReadHoldingRegisters,
                    slaveId,
                    startAddress,
                    numberOfPoints);

            var ccc = master.Transport.BuildMessageFrame(request);//生成 读取保持寄存器帧


            ShowMessage(ccc.ToHexString(), MessageType.Send);//输入16进制的帧

            var aa = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numberOfPoints);

            ShowMessage(string.Join(" ", aa), MessageType.Receive);
            #endregion
        }
        catch (Exception ex)
        {

        }
    }



    TcpClient client = new();

    [ObservableProperty]
    bool isOnline;


    private bool isConnecting = false;

    [RelayCommand]
    async Task Connect()
    {
        try
        {
            if (isConnecting)
            {
                return;
            }
            else
            {
                isConnecting = true;
            }
            //client = new TcpClient(ServerIp,ServerPort);     //关闭时会是否,需要重新初始化
            client = new TcpClient();     //关闭时会是否,需要重新初始化
            await client.ConnectAsync(ServerIp, ServerPort);
            IsOnline = client.Connected;
            ShowMessage("建立连接成功...");
            //var xxx =  client.Connected;
            //var factory = new ModbusFactory(logger: new DebugModbusLogger());
            //master = factory.CreateMaster(client);

        }
        catch (Exception)
        {
            IsOnline = client.Connected;
        }
        finally { isConnecting = false; }
    }

    [RelayCommand]
    async Task DisConnect()
    {
        try
        {
            client.Close();
            IsOnline = client.Connected;
            ShowMessage("关闭连接成功...");
        }
        catch (Exception)
        {

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

    ///// <summary>
    ///// 页面展示接收数据消息
    ///// </summary>
    ///// <param name="frame"></param>
    //public void ShowReceiveMessage(MtcpFrame frame)
    //{
    //    try
    //    {
    //        void action()
    //        {
    //            var msg = new ModbusRtuMessageData("", DateTime.Now, MessageType.Receive, frame)
    //            {
    //                MessageSubContents = new ObservableCollection<MessageSubContent>(frame.GetmessageWithErrMsg())
    //            };
    //            Messages.Add(msg);
    //            while (Messages.Count > 200)
    //            {
    //                Messages.RemoveAt(0);
    //            }
    //        }
    //        Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
    //    }
    //    catch (Exception) { }
    //}

    ///// <summary>
    ///// 页面展示发送数据消息
    ///// </summary>
    ///// <param name="frame"></param>
    //public void ShowSendMessage(ModbusRtuFrame frame)
    //{
    //    try
    //    {
    //        void action()
    //        {
    //            var msg = new ModbusRtuMessageData("", DateTime.Now, MessageType.Send, frame)
    //            {
    //                MessageSubContents = new ObservableCollection<MessageSubContent>(frame.GetmessageWithErrMsg())
    //            };

    //            Messages.Add(msg);
    //            while (Messages.Count > 200)
    //            {
    //                Messages.RemoveAt(0);
    //            }
    //        }
    //        Wu.Wpf.Utils.ExecuteFunBeginInvoke(action);
    //    }
    //    catch (System.Exception) { }
    //}


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
