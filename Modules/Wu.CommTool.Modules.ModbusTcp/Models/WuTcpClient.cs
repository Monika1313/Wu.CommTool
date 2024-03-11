namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Tcp客户端
/// </summary>
public partial class WuTcpClient : TcpClient
{

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
    public event Action<EventArgs> ClientDisconnected;

    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event Action<EventArgs> MessageReceived;

    /// <summary>
    /// 消息发送事件
    /// </summary>
    public event Action<EventArgs> MessageSending;

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    public new void Connect(string hostname, int port)
    {
        ClientConnecting?.Invoke();//触发连接中事件
        base.Connect(hostname, port);
        if (Connected)
        {
            ClientConnected?.Invoke(new WuTcpClientEventArgs());//触发连接成功事件
        }
    }
    public void SendMessage()
    {

    }





}
