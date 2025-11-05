using HandyControl.Controls;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Wu.CommTool.Core.Enums.Tcp;

namespace Wu.CommTool.Modules.TcpServer.Models;

public partial class TcpServerModel : ObservableObject
{
    private System.Net.Sockets.TcpListener tcpListener;
    //private List<System.Net.Sockets.TcpClient> clients = [];
    public static readonly ILog log = LogManager.GetLogger(typeof(TcpServerModel));


    #region 属性
    /// <summary>
    /// 服务器IP
    /// </summary>
    [ObservableProperty] string serverIp = "127.0.0.1";

    [ObservableProperty] int serverPort = 9999;

    [ObservableProperty] bool isRunning;

    /// <summary>
    /// 发送输入框内容
    /// </summary>
    [ObservableProperty] string sendInput = string.Empty;

    /// <summary>
    /// 发送消息类型
    /// </summary>
    [ObservableProperty] TcpDataType sendTcpDataType = TcpDataType.UTF8;

    /// <summary>
    /// 接收消息类型
    /// </summary>
    [ObservableProperty] TcpDataType receiveTcpDataType = TcpDataType.UTF8;


    /// <summary>
    /// 在线客户端列表
    /// </summary>
    [ObservableProperty][property:JsonIgnore] ObservableCollection<TcpClient> clients = [];

    /// <summary>
    /// 选择的客户端
    /// </summary>
    [ObservableProperty][property: JsonIgnore] TcpClient selectedClient = null;
    #endregion

    public TcpServerModel()
    {
        
    }

    [RelayCommand]
    [property: JsonIgnore]
    public async Task Start()
    {
        try
        {
            if (IsRunning)
            {
                ShowMessage("服务器已在运行中", MessageType.Info);
                return;
            }

            tcpListener = new TcpListener(IPAddress.Parse(ServerIp), ServerPort);
            tcpListener.Start();
            IsRunning = true;

            ShowMessage($"TCP服务器启动成功 {tcpListener.LocalEndpoint}");

            while (IsRunning)
            {
                // 等待客户端连接
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                ShowMessage($"客户端已连接: {client.Client.RemoteEndPoint}");

                Clients.Add(client);
                if (Clients.Count == 1)
                {
                    SelectedClient = client; // 如果是第一个客户端，自动选择
                }

                // 为每个客户端创建一个线程处理
                _ = HandleClientAsync(client);
            }
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                ShowErrorMessage($"地址已被使用: {ex.Message}");
            }
            else if(ex.SocketErrorCode == SocketError.OperationAborted)
            {
                //正常操作关闭服务器,不报警
                ShowMessage($"TCP服务器关闭成功");
            }
            else
            {
                ShowErrorMessage($"网络错误: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"服务器错误: {ex.Message}");
        }
        finally
        {
            tcpListener.Stop();
        }
    }

    /// <summary>
    /// 客户端处理方法
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    private async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // 客户端断开连接

                var receivedBuffer = buffer.Take(bytesRead).ToArray();// 只处理实际读取的字节数

                switch (ReceiveTcpDataType)
                {
                    case TcpDataType.ASCII:
                        string receivedData = Encoding.ASCII.GetString(receivedBuffer);
                        ShowReceiveMessage($"{receivedData}");
                        break;
                    case TcpDataType.HEX:
                        ShowReceiveMessage($"{BitConverter.ToString(receivedBuffer).Replace("-", "").InsertFormat(4, " ")}");
                        break;
                    case TcpDataType.UTF8:
                        string utf8Data = Encoding.UTF8.GetString(receivedBuffer);
                        ShowReceiveMessage($"{utf8Data}");
                        break;
                    case TcpDataType.Unicode:
                        string unicodeData = Encoding.Unicode.GetString(receivedBuffer);
                        ShowReceiveMessage($"{unicodeData}");
                        break;
                    default:
                        break;
                }
            }
        }
        catch(IOException ex)
        {

        }
        catch (Exception ex)
        {
            ShowErrorMessage($"处理客户端 {client.Client.RemoteEndPoint} 时出错: {ex.Message}");
        }
        finally
        {
            if (IsRunning && client != null)
            {
                ShowMessage($"客户端已断开连接 {client?.Client?.RemoteEndPoint}");
                client?.Close();
                Clients.Remove(client); // 从客户端列表中移除
                if (SelectedClient == client)
                {
                    SelectedClient = Clients.FirstOrDefault(); // 如果断开的客户端是当前选中的，选择下一个客户端
                }
            }
        }
    }

    /// <summary>
    /// 关闭服务器
    /// </summary>
    [RelayCommand]
    [property:JsonIgnore]
    public void Stop()
    {
        try
        {
            IsRunning = false;

            List<TcpClient> clientsToClose = [.. Clients];
            Clients.Clear();

            foreach (var client in clientsToClose)
            {
                try
                {
                    client.Close();
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"关闭客户端连接时出错: {ex.Message}");
                }
            }
            tcpListener.Stop();//关闭服务器
        }
        catch (Exception ex)
        {
            Growl.Error($"关闭服务器出错了: {ex.Message}");
        }
    }


    /// <summary>
    /// 发送数据
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private async Task Send()
    {
        if (SelectedClient == null || !SelectedClient.Connected)
        {
            ShowErrorMessage($"客户端已离线");
            return;
        }

        try
        {
            var networkStream = SelectedClient.GetStream();
            string message = SendInput;

            if (string.IsNullOrEmpty(message))
            {
                ShowErrorMessage("发送消息不能为空");
                return;
            }

            switch (SendTcpDataType)
            {
                case TcpDataType.ASCII:
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    networkStream.Write(data, 0, data.Length);
                    ShowSendMessage($"{message}");
                    break;
                case TcpDataType.HEX:
                    string hexString = SendInput.Replace(" ", "");

                    // 检查长度是否为偶数
                    if (hexString.Length % 2 != 0)
                    {
                        ShowSendMessage("错误：十六进制字符串长度必须是偶数");
                        return;
                    }

                    // 检查是否只包含有效的十六进制字符
                    if (!System.Text.RegularExpressions.Regex.IsMatch(hexString, @"^[0-9A-Fa-f]+$"))
                    {
                        ShowSendMessage("错误：字符串包含无效的十六进制字符");
                        return;
                    }

                    byte[] data2 = new byte[hexString.Length / 2];

                    for (int i = 0; i < data2.Length; i++)
                    {
                        data2[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                    }
                    networkStream.Write(data2, 0, data2.Length);
                    ShowSendMessage($"{BitConverter.ToString(data2).Replace("-", " ")}");
                    break;
                case TcpDataType.UTF8:
                    byte[] utf8Data = Encoding.UTF8.GetBytes(message);
                    networkStream.Write(utf8Data, 0, utf8Data.Length);
                    ShowSendMessage($"{message}");
                    break;
                case TcpDataType.Unicode:
                    byte[] unicodeData = Encoding.Unicode.GetBytes(message);
                    networkStream.Write(unicodeData, 0, unicodeData.Length);
                    ShowSendMessage($"{message}");
                    break;
                default:
                    break;
            }
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
