using System.Net.Http;
using System.Net.Sockets;

namespace Wu.CommTool.Modules.TcpClient.Models;

public partial class TcpClientModel : ObservableObject
{
    private System.Net.Sockets.TcpClient tcpClient;
    private NetworkStream networkStream;
    public static readonly ILog log = LogManager.GetLogger(typeof(TcpClientModel));

    #region 属性
    /// <summary>
    /// 服务器IP
    /// </summary>
    [ObservableProperty] string serverIp = "127.0.0.1";

    /// <summary>
    /// 服务器端口
    /// </summary>
    [ObservableProperty] int serverPort = 9999;

    /// <summary>
    /// 是否已连接
    /// </summary>
    [ObservableProperty] bool isConnected = false;

    /// <summary>
    /// 发送输入框内容
    /// </summary>
    [ObservableProperty] string sendInput = string.Empty;
    #endregion


    /// <summary>
    /// 连接服务器
    /// </summary>
    [RelayCommand]
    private void Connect()
    {
        try
        {
            if (tcpClient?.Connected == true)
            {
                return; // 已连接，无需重复连接
            }

            tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.Connect(ServerIp, ServerPort);
            networkStream = tcpClient.GetStream();
            ShowMessage($"连接服务器{ServerIp}:{ServerPort}");
            IsConnected = true;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"连接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    [RelayCommand]
    private void Disconnect()
    {
        try
        {
            networkStream?.Close();
            tcpClient?.Close();
            ShowMessage($"断开连接");
            IsConnected = false;
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Disconnection error: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送
    /// </summary>
    [RelayCommand]
    private void Send()
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            ShowErrorMessage($"Not connected to server");
            return;
        }

        try
        {
            string message = SendInput;
            byte[] data = Encoding.UTF8.GetBytes(message);

            networkStream.Write(data, 0, data.Length);
            ShowSendMessage($"{message}");
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Send error: {ex.Message}");
        }
    }


    #region 页面消息
    /// <summary>
    /// 页面消息
    /// </summary>
    [ObservableProperty] ObservableCollection<MessageData> messages = [];

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
