﻿namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Tcp客户端
/// </summary>
public partial class ModbusTcpClient : TcpClient
{
    public ModbusTcpClient()
    {

    }

    #region 事件
    /// <summary>
    /// 连接成功事件
    /// </summary>
    public event Action<WuTcpClientEventArgs> ClientConnected;

    /// <summary>
    /// 连接中事件
    /// </summary>
    public event Action ClientConnecting;

    /// <summary>
    /// 离线事件
    /// </summary>
    public event Action<WuTcpClientEventArgs> ClientDisconnected;

    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event Action<string> MessageReceived;

    /// <summary>
    /// 消息发送事件
    /// </summary>
    public event Action<string> MessageSending;

    /// <summary>
    /// 发生错误事件
    /// </summary>
    public event Action<string> ErrorOccurred;
    #endregion

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    public async new Task ConnectAsync(string hostname, int port)
    {
        ClientConnecting?.Invoke();//触发连接中事件
        await base.ConnectAsync(hostname, port);
        if (Connected)
        {
            _ = Task.Run(ReceiveData);
            ClientConnected?.Invoke(new WuTcpClientEventArgs());//触发连接成功事件
        }
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    private void ReceiveData()
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = this.GetStream().Read(buffer, 0, buffer.Length);
                
                if (bytesRead > 0)
                {
                    string hexString = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ""); // 将字节数组转换为16进制字符串
                    MessageReceived?.Invoke(hexString);
                }
                //当被动离线时字节为0 退出循环并主动关闭
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred while receiving data: " + ex.Message);
        }
        finally
        {
            if (this.Connected)
            {
                this.Close(); // 关闭连接
            }
        }
    }

    public new void Close()
    {
        base.Close();
        if (!Connected)
        {
            ClientDisconnected?.Invoke(new WuTcpClientEventArgs());//触发离线事件
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message"></param>
    public void SendMessage(string message)
    {
        try
        {
            if (Connected)
            {
                //byte[] data = Encoding.ASCII.GetBytes(message);
                byte[] data = ModbusUtils.FromHex(message);
                this.GetStream().Write(data, 0, data.Length);
                MessageSending?.Invoke(message);
            }
            else
            {
                ErrorOccurred("未连接...");
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred(ex.Message);
            this.Close();
        }
    }
}
