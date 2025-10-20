using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Wu.CommTool.Core.Enums.Tcp;

namespace Wu.CommTool.Modules.Udp.Models;

public partial class UdpClientModel : ObservableObject
{
    private UdpClient udpReceiveClient = new();
    private UdpClient udpSendClient = new();
    private IPEndPoint targetEndPoint;
    public static readonly ILog log = LogManager.GetLogger(typeof(UdpClientModel));

    private int receivePort;
    private bool isReceiving = false;
    private Thread receiveThread;
    private int sendCount = 0;
    private int receiveCount = 0;
    private System.Timers.Timer autoSendTimer;

    // 线程安全的接收队列
    private ConcurrentQueue<string> receivedMessages = new ConcurrentQueue<string>();

    public string TargetAddress => targetEndPoint?.ToString() ?? "未设置";
    public int ListenPort => receivePort;

    public bool IsReceiving => isReceiving;
    public int SendCount => sendCount;
    public int ReceiveCount => receiveCount;




    #region 属性
    [ObservableProperty] string serverIp = "127.0.0.1";
    [ObservableProperty] int serverPort = 13333;

    /// <summary>
    /// 用于接收的监听端口
    /// </summary>
    [ObservableProperty] int receiveListenPort;


    /// <summary>
    /// 发送输入框内容
    /// </summary>
    [ObservableProperty] string sendInput = string.Empty;

    /// <summary>
    /// 发送消息类型
    /// </summary>
    [ObservableProperty] TcpDataType sendTcpDataType = TcpDataType.Uft8;

    /// <summary>
    /// 接收消息类型
    /// </summary>
    [ObservableProperty] TcpDataType receiveTcpDataType = TcpDataType.Uft8;

    [ObservableProperty] bool isOpened;
    #endregion


    public UdpClientModel()
    {
        // 设置超时时间
        udpSendClient.Client.ReceiveTimeout = 1000;
    }





    /// <summary>
    /// 连接服务器
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private void Open()
    {
        try
        {
            if (IsOpened)
            {
                ShowErrorMessage("UDP连接已打开，请先关闭当前连接");
                return;
            }

            this.targetEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
            this.receivePort = ReceiveListenPort == 0 ? ServerPort : ReceiveListenPort;

            // 创建发送客户端
            udpSendClient = new UdpClient();
            udpSendClient.Client.SendBufferSize = 1024 * 1024;

            // 创建接收客户端（绑定到指定端口）
            udpReceiveClient = new UdpClient(receivePort);
            udpReceiveClient.Client.ReceiveBufferSize = 1024 * 1024;

            IsOpened = true;
            ShowMessage($"UDP连接打开成功。目标地址: {targetEndPoint}。监听端口: {receivePort}");
            return;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"打开UDP失败: {ex.Message}");
            Close();
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void Close()
    {
        try
        {
            udpSendClient?.Close();
            ShowMessage($"断开连接");

            //// 停止自动发送
            //StopAutoSend();
            //// 停止接收
            //StopReceiving();

            // 关闭UDP客户端
            udpSendClient?.Close();
            udpReceiveClient?.Close();

            IsOpened = false;
            ShowMessage("UDP已关闭");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"关闭UDP客户端时发生错误: {ex.Message}");
        }
    }



    /// <summary>
    /// 发送消息
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private void SendMessage()
    {
        if (!IsOpened)
        {
            Open();
            if (!IsOpened) return;
        }

        try
        {
            //TODO 根据选择的消息格式发送
            byte[] data = Encoding.UTF8.GetBytes(SendInput);
            udpSendClient.Send(data, data.Length, targetEndPoint);
            ShowSendMessage($"{SendInput}");
            sendCount++;
            return;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"发送失败: {ex.Message}");
            return;
        }
    }

    /// <summary>
    /// 发送字节数据
    /// </summary>
    public void SendBytes(byte[] data)
    {
        //try
        //{
        //    udpSendClient.Send(data, data.Length, serverEndPoint);
        //    Console.WriteLine($"[发送] {DateTime.Now:HH:mm:ss.fff} -> 字节数据 {data.Length} bytes");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"[错误] 发送失败: {ex.Message}");
        //}
    }

    /// <summary>
    /// 广播消息到整个子网
    /// </summary>
    public void BroadcastMessage(string message, int port)
    {
        //try
        //{
        //    using (var broadcaster = new UdpClient())
        //    {
        //        broadcaster.EnableBroadcast = true;
        //        IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
        //        byte[] data = Encoding.UTF8.GetBytes(message);
        //        broadcaster.Send(data, data.Length, broadcastEndPoint);
        //        Console.WriteLine($"[广播] {DateTime.Now:HH:mm:ss.fff} -> {message}");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"[错误] 广播失败: {ex.Message}");
        //}
    }





    #region 页面消息
    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty][property: JsonIgnore] ObservableCollection<MessageData> messages = [];

    protected void ShowErrorMessage(string message) => ShowMessage(message, MessageType.Error);

    protected void ShowReceiveMessage(string message, string title = "")
    {
        try
        {
            void action()
            {
                Messages.Add(new MqttMessageData($"{message}", DateTime.Now, MessageType.Receive, title));
                log.Info($"接收:{message}");
                while (Messages.Count > 150)
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
                while (Messages.Count > 150)
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
    /// 清空页面消息
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private void Clear()
    {
        try
        {
            Wu.Wpf.Utils.ExecuteFun(() => Messages.Clear());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"清空消息失败: {ex.Message}");
        }
    }
    #endregion
}



