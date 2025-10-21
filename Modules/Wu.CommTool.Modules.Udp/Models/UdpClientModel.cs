using System.Collections.Concurrent;
using System.Threading;
using Wu.CommTool.Core.Enums.Tcp;

namespace Wu.CommTool.Modules.Udp.Models;

public partial class UdpClientModel : ObservableObject
{
    private UdpClient udpClient = new();
    public static readonly ILog log = LogManager.GetLogger(typeof(UdpClientModel));

    // 线程安全的接收队列
    private ConcurrentQueue<string> receivedMessages = new ConcurrentQueue<string>();


    #region 属性
    public IPEndPoint RemoteEndPoint => new(IPAddress.Parse(RemoteIp), RemotePort);
    [ObservableProperty] string remoteIp = "127.0.0.1";
    [ObservableProperty] int remotePort = 13333;

    public IPEndPoint LocalEndPoint => new(IPAddress.Parse(LocalIp), LocalPort);
    [ObservableProperty] string localIp = "0.0.0.0";
    [ObservableProperty] int localPort = 9999;


    /// <summary>
    /// 发送输入框内容
    /// </summary>
    [ObservableProperty] string sendInput = string.Empty;

    /// <summary>
    /// 发送消息类型
    /// </summary>
    [ObservableProperty] TcpDataType sendDataType = TcpDataType.UTF8;

    /// <summary>
    /// 接收消息类型
    /// </summary>
    [ObservableProperty] TcpDataType receiveDataType = TcpDataType.UTF8;

    /// <summary>
    /// UDP客户端是否已打开
    /// </summary>
    [ObservableProperty] bool isOpened;
    #endregion

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

            //创建客户端
            udpClient = new UdpClient(LocalEndPoint);
            udpClient.Client.SendBufferSize = 1024 * 1024;

            IsOpened = true;
            StartListening();//开启监听
            ShowMessage($"打开UDP客户端: {((IPEndPoint)udpClient.Client.LocalEndPoint)}");
            return;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"打开UDP客户端失败: {ex.Message}");
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void Close()
    {
        try
        {
            udpClient?.Close();
            ShowMessage($"关闭UDP客户端");

            // 关闭UDP客户端
            udpClient?.Close();

            IsOpened = false;
            ShowMessage("UDP客户端已关闭");
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

        if (string.IsNullOrEmpty(SendInput))
        {
            ShowErrorMessage("发送 消息不能为空");
            return;
        }

        try
        {
            switch (SendDataType)
            {
                case TcpDataType.ASCII:
                    byte[] data = Encoding.ASCII.GetBytes(SendInput);
                    udpClient.Send(data, data.Length, RemoteEndPoint);
                    ShowSendMessage($"{SendInput}", RemoteEndPoint.ToString());
                    break;
                case TcpDataType.HEX:
                    string hexString = SendInput.Replace(" ", "");

                    // 检查长度是否为偶数
                    if (hexString.Length % 2 != 0)
                    {
                        ShowErrorMessage("十六进制字符串长度必须是偶数");
                        return;
                    }

                    // 检查是否只包含有效的十六进制字符
                    if (!System.Text.RegularExpressions.Regex.IsMatch(hexString, @"^[0-9A-Fa-f]+$"))
                    {
                        ShowErrorMessage("字符串包含无效的十六进制字符");
                        return;
                    }

                    byte[] data2 = new byte[hexString.Length / 2];

                    for (int i = 0; i < data2.Length; i++)
                    {
                        data2[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                    }
                    udpClient.Send(data2, data2.Length, RemoteEndPoint);
                    ShowSendMessage($"{BitConverter.ToString(data2).Replace("-", " ")}", RemoteEndPoint.ToString());
                    break;
                case TcpDataType.UTF8:
                    byte[] utf8Data = Encoding.UTF8.GetBytes(SendInput);
                    udpClient.Send(utf8Data, utf8Data.Length, RemoteEndPoint);
                    ShowSendMessage($"{SendInput}", RemoteEndPoint.ToString());
                    break;
                case TcpDataType.Unicode:
                    byte[] unicodeData = Encoding.Unicode.GetBytes(SendInput);
                    udpClient.Send(unicodeData, unicodeData.Length, RemoteEndPoint);
                    ShowSendMessage($"{SendInput}", RemoteEndPoint.ToString());
                    break;
                default:
                    break;
            }
            return;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"发送失败: {ex.Message}");
            return;
        }
    }

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// 开始异步接收消息
    /// </summary>
    public void StartListening()
    {
        if (!IsOpened) return;
        Task.Run(async () => await ReceiveMessagesAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// 异步接收消息
    /// </summary>
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsOpened)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                var receivedBuffer = result.Buffer;

                switch (ReceiveDataType)
                {
                    case TcpDataType.ASCII:
                        string receivedData = Encoding.ASCII.GetString(receivedBuffer);
                        ShowReceiveMessage($"{receivedData}", result.RemoteEndPoint.ToString());
                        break;
                    case TcpDataType.HEX:
                        ShowReceiveMessage($"{BitConverter.ToString(receivedBuffer).Replace("-", "").InsertFormat(4, " ")}", result.RemoteEndPoint.ToString());
                        break;
                    case TcpDataType.UTF8:
                        string utf8Data = Encoding.UTF8.GetString(receivedBuffer);
                        ShowReceiveMessage($"{utf8Data}", result.RemoteEndPoint.ToString());
                        break;
                    case TcpDataType.Unicode:
                        string unicodeData = Encoding.Unicode.GetString(receivedBuffer);
                        ShowReceiveMessage($"{unicodeData}", result.RemoteEndPoint.ToString());
                        break;
                    default:
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                // UDP客户端已被释放，正常退出
                break;
            }
            catch (Exception)
            {
                // 短暂延迟后继续接收
                await Task.Delay(100, cancellationToken);
            }
        }
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

    protected void ShowReceiveMessage(string message, string remoteEndPoint = "")
    {
        try
        {
            void action()
            {
                Messages.Add(new UdpMessageData($"{message}", DateTime.Now, MessageType.Receive, remoteEndPoint, ReceiveDataType.ToString()));
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


    protected void ShowSendMessage(string message, string remoteEndPoint = "")
    {
        try
        {
            void action()
            {
                Messages.Add(new UdpMessageData($"{message}", DateTime.Now, MessageType.Send, remoteEndPoint, SendDataType.ToString()));
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
                Messages.Add(new UdpMessageData($"{message}", DateTime.Now, type, title));
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



