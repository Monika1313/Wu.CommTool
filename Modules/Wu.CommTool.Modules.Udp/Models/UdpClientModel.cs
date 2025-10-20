using System.Net.Sockets;
using Wu.CommTool.Core.Enums.Tcp;

namespace Wu.CommTool.Modules.Udp.Models;

public partial class UdpClientModel : ObservableObject
{
    private UdpClient udpReceiveClient;
    private UdpClient udpSendClient;
    private IPEndPoint serverEndPoint;
    public static readonly ILog log = LogManager.GetLogger(typeof(UdpClientModel));


    #region 属性
    [ObservableProperty] string serverIp = "127.0.0.1";
    [ObservableProperty] int serverPort = 9999;
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
    #endregion


    public UdpClientModel()
    {
        // 设置超时时间
        udpSendClient.Client.ReceiveTimeout = 1000;
    }
    

    /// <summary>
    /// 发送消息
    /// </summary>
    [RelayCommand]
    public async Task SendMessage(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpSendClient.SendAsync(data, data.Length, serverEndPoint);
            ShowSendMessage(message);
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"发送失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送字节数据
    /// </summary>
    public void SendBytes(byte[] data)
    {
        try
        {
            udpSendClient.Send(data, data.Length, serverEndPoint);
            Console.WriteLine($"[发送] {DateTime.Now:HH:mm:ss.fff} -> 字节数据 {data.Length} bytes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 发送失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 广播消息到整个子网
    /// </summary>
    public void BroadcastMessage(string message, int port)
    {
        try
        {
            using (var broadcaster = new UdpClient())
            {
                broadcaster.EnableBroadcast = true;
                IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
                byte[] data = Encoding.UTF8.GetBytes(message);
                broadcaster.Send(data, data.Length, broadcastEndPoint);
                Console.WriteLine($"[广播] {DateTime.Now:HH:mm:ss.fff} -> {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 广播失败: {ex.Message}");
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
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Disconnection error: {ex.Message}");
        }
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
