﻿using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wu.CommTool.Core.Enums.Tcp;

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
    [ObservableProperty][property: JsonIgnore] bool isConnected = false;

    /// <summary>
    /// 发送输入框内容
    /// </summary>
    [ObservableProperty] string sendInput = string.Empty;

    /// <summary>
    /// 自动重连
    /// </summary>
    [ObservableProperty] bool autoReConnect;

    /// <summary>
    /// 发送消息类型
    /// </summary>
    [ObservableProperty] TcpDataType sendTcpDataType = TcpDataType.Uft8;

    /// <summary>
    /// 接收消息类型
    /// </summary>
    [ObservableProperty] TcpDataType receiveTcpDataType = TcpDataType.Uft8;
    #endregion


    /// <summary>
    /// 连接服务器
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private async Task Connect()
    {
        try
        {
            if (tcpClient?.Connected == true)
            {
                return; // 已连接，无需重复连接
            }
            tcpClient = new System.Net.Sockets.TcpClient();
            ShowMessage("连接中...");
            await tcpClient.ConnectAsync(ServerIp, ServerPort);
            networkStream = tcpClient.GetStream();
            ShowMessage($"连接服务器成功 服务器:{ServerIp}:{ServerPort}  客户端:{tcpClient.Client.LocalEndPoint}");
            IsConnected = true;
            StartReceiving();// 启动接收数据
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
    [property: JsonIgnore]
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
    /// 发送数据
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private async Task Send()
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            await Connect();
            if (!IsConnected)
            {
                ShowErrorMessage($"Not connected to server");
                return;
            }
        }

        try
        {
            string message = SendInput;

            if (string.IsNullOrEmpty(message))
            {
                ShowErrorMessage("发送消息不能为空");
                return;
            }

            switch (SendTcpDataType)
            {
                case TcpDataType.Ascii:
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    networkStream.Write(data, 0, data.Length);
                    ShowSendMessage($"{message}");
                    break;
                case TcpDataType.Hex:
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
                case TcpDataType.Uft8:
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
            Disconnect();
        }
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    private async void StartReceiving()
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (tcpClient != null && tcpClient.Connected)
            {
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    // 服务器已关闭连接
                    ShowErrorMessage("服务器已关闭连接");
                    networkStream?.Close();
                    tcpClient?.Close();
                    IsConnected = false;
                    break;
                }

                var receivedBuffer = buffer.Take(bytesRead).ToArray();// 只处理实际读取的字节数

                switch (ReceiveTcpDataType)
                {
                    case TcpDataType.Ascii:
                        string receivedData = Encoding.ASCII.GetString(receivedBuffer);
                        ShowReceiveMessage($"{receivedData}");
                        break;
                    case TcpDataType.Hex:
                        ShowReceiveMessage($"{BitConverter.ToString(receivedBuffer).Replace("-", "").InsertFormat(4, " ")}");
                        break;
                    case TcpDataType.Uft8:
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
        catch (Exception ex)
        {
            if (tcpClient.Connected)
            {
                ShowErrorMessage($"Receive error: {ex.Message}");
            }
        }
    }


    //private async Task AutoReconnect()
    //{
    //    while (AutoReConnect)
    //    {
    //        if (tcpClient == null || !tcpClient.Connected)
    //        {
    //            try
    //            {
    //                if (tcpClient.Connected)
    //                {
    //                    StartReceiving();
    //                }
    //            }
    //            catch { }
    //        }
    //        await Task.Delay(5000); // 5秒重试一次
    //    }
    //}

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
