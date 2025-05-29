using System.Collections.Generic;

namespace Wu.CommTool.Modules.TcpServer.Models;

public partial class TcpServerModel : ObservableObject
{
    private System.Net.Sockets.TcpListener tcpListener;
    private List<System.Net.Sockets.TcpClient> clients = [];
    public static readonly ILog log = LogManager.GetLogger(typeof(TcpServerModel));

    #region 属性
    /// <summary>
    /// 服务器IP
    /// </summary>
    [ObservableProperty] string serverIp = "127.0.0.1";

    [ObservableProperty] int serverPort = 9999;

    [ObservableProperty] bool isConnected;

    /// <summary>
    /// 发送输入框内容
    /// </summary>
    [ObservableProperty] string sendInput = string.Empty;
    #endregion

    public TcpServerModel()
    {
        ShowErrorMessage("功能未做...");
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
