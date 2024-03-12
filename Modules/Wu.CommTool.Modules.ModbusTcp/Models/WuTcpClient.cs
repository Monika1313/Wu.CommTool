using System.Text;
using System.Timers;

namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Tcp客户端
/// </summary>
public partial class WuTcpClient : TcpClient
{
    public WuTcpClient()
    {
        //timer = new Timer(1000);
        //timer.Elapsed += Timer_Elapsed;
        //timer.Start();
    }

    //Timer timer;

    ///// <summary>
    ///// 定时检测是否离线
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    //{
    //    try
    //    {
    //        if (!this.Connected)
    //        {
    //            ClientDisconnected?.Invoke(new WuTcpClientEventArgs());
    //            timer.Stop();
    //        }
    //    }
    //    catch (Exception)
    //    {

    //    }
    //}




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
    public event Action<EventArgs> MessageSending;
    #endregion



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
            //timer?.Start();
            Task.Run(() => ReceiveData());
            ClientConnected?.Invoke(new WuTcpClientEventArgs());//触发连接成功事件
        }
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    void ReceiveData()
    {
        try
        {
            while (this.Connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = this.GetStream().Read(buffer, 0, buffer.Length);
                //string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                string hexString = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ""); // 将字节数组转换为16进制字符串
                MessageReceived?.Invoke(hexString);
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
        //timer.Stop();
        if (!Connected)
        {
            ClientDisconnected?.Invoke(new WuTcpClientEventArgs());//触发离线事件
        }
    }


    public void SendMessage()
    {

    }





}
