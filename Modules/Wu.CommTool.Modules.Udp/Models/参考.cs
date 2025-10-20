using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wu.CommTool.Modules.Udp.Models;

public class UdpDebugTool : IDisposable
{
    private UdpClient sendClient;
    private UdpClient receiveClient;
    private IPEndPoint targetEndPoint;
    private int receivePort;
    private bool isReceiving = false;
    private bool isOpened = false;
    private Thread receiveThread;
    private int sendCount = 0;
    private int receiveCount = 0;
    private System.Timers.Timer autoSendTimer;

    // 线程安全的接收队列
    private ConcurrentQueue<string> receivedMessages = new ConcurrentQueue<string>();

    public string TargetAddress => targetEndPoint?.ToString() ?? "未设置";
    public int ListenPort => receivePort;
    public bool IsOpened => isOpened;
    public bool IsReceiving => isReceiving;
    public int SendCount => sendCount;
    public int ReceiveCount => receiveCount;

    /// <summary>
    /// 打开UDP连接
    /// </summary>
    public bool Open(string targetIp, int targetPort, int listenPort = 0)
    {
        try
        {
            if (isOpened)
            {
                Console.WriteLine("[警告] UDP连接已打开，请先关闭当前连接");
                return false;
            }

            this.targetEndPoint = new IPEndPoint(IPAddress.Parse(targetIp), targetPort);
            this.receivePort = listenPort == 0 ? targetPort : listenPort;

            // 创建发送客户端
            sendClient = new UdpClient();
            sendClient.Client.SendBufferSize = 1024 * 1024;

            // 创建接收客户端（绑定到指定端口）
            receiveClient = new UdpClient(receivePort);
            receiveClient.Client.ReceiveBufferSize = 1024 * 1024;

            isOpened = true;
            Console.WriteLine($"[系统] UDP连接已打开");
            Console.WriteLine($"[系统] 目标地址: {targetEndPoint}");
            Console.WriteLine($"[系统] 监听端口: {receivePort}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 打开UDP连接失败: {ex.Message}");
            Close();
            return false;
        }
    }

    /// <summary>
    /// 关闭UDP连接
    /// </summary>
    public void Close()
    {
        // 停止自动发送
        StopAutoSend();

        // 停止接收
        StopReceiving();

        // 关闭UDP客户端
        try
        {
            sendClient?.Close();
            receiveClient?.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[警告] 关闭UDP客户端时发生错误: {ex.Message}");
        }

        isOpened = false;
        Console.WriteLine("[系统] UDP连接已关闭");
    }

    /// <summary>
    /// 开始接收数据
    /// </summary>
    public bool StartReceiving()
    {
        if (!isOpened)
        {
            Console.WriteLine("[错误] 请先打开UDP连接");
            return false;
        }

        if (isReceiving)
        {
            Console.WriteLine("[警告] 接收功能已在运行");
            return true;
        }

        isReceiving = true;
        receiveThread = new Thread(ReceiveLoop)
        {
            IsBackground = true,
            Name = "UDP接收线程"
        };
        receiveThread.Start();

        Console.WriteLine($"[系统] 开始监听端口: {receivePort}");
        return true;
    }

    /// <summary>
    /// 停止接收数据
    /// </summary>
    public void StopReceiving()
    {
        if (!isReceiving) return;

        isReceiving = false;

        // 中断接收操作
        try
        {
            receiveClient?.Close();
        }
        catch { }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            if (!receiveThread.Join(1000))
            {
                receiveThread.Abort();
            }
        }

        Console.WriteLine("[系统] 已停止接收数据");
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public bool SendMessage(string message)
    {
        if (!isOpened)
        {
            Console.WriteLine("[错误] 请先打开UDP连接");
            return false;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            sendClient.Send(data, data.Length, targetEndPoint);
            Console.WriteLine($"[发送] {DateTime.Now:HH:mm:ss.fff} -> {message}");
            sendCount++;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 发送失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 发送十六进制数据
    /// </summary>
    public bool SendHex(string hexString)
    {
        if (!isOpened)
        {
            Console.WriteLine("[错误] 请先打开UDP连接");
            return false;
        }

        try
        {
            byte[] data = HexStringToBytes(hexString);
            sendClient.Send(data, data.Length, targetEndPoint);
            Console.WriteLine($"[发送] {DateTime.Now:HH:mm:ss.fff} -> HEX: {hexString}");
            sendCount++;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 发送失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 接收循环
    /// </summary>
    private void ReceiveLoop()
    {
        // 重新创建接收客户端（因为之前的被关闭了）
        try
        {
            receiveClient = new UdpClient(receivePort);
            receiveClient.Client.ReceiveBufferSize = 1024 * 1024;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 重新创建接收客户端失败: {ex.Message}");
            isReceiving = false;
            return;
        }

        while (isReceiving)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedData = receiveClient.Receive(ref remoteEndPoint);

                ProcessReceivedData(receivedData, remoteEndPoint);
                receiveCount++;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (isReceiving) // 只有在仍然接收时才显示错误
                {
                    Console.WriteLine($"[错误] 接收数据时发生错误: {ex.Message}");
                    Thread.Sleep(100);
                }
            }
        }
    }

    /// <summary>
    /// 处理接收到的数据
    /// </summary>
    private void ProcessReceivedData(byte[] data, IPEndPoint remoteEndPoint)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

        string textData = Encoding.UTF8.GetString(data);
        bool isPrintableText = IsPrintableText(textData);

        if (isPrintableText)
        {
            string displayText = $"[接收] {timestamp} FROM {remoteEndPoint} -> {textData}";
            Console.WriteLine(displayText);
            receivedMessages.Enqueue(displayText);
        }
        else
        {
            string hexData = BitConverter.ToString(data).Replace("-", " ");
            string displayText = $"[接收] {timestamp} FROM {remoteEndPoint} -> HEX: {hexData} ({data.Length} bytes)";
            Console.WriteLine(displayText);
            receivedMessages.Enqueue(displayText);
        }
    }

    /// <summary>
    /// 开始自动发送
    /// </summary>
    public bool StartAutoSend(int intervalMs, string message)
    {
        if (!isOpened)
        {
            Console.WriteLine("[错误] 请先打开UDP连接");
            return false;
        }

        StopAutoSend();

        autoSendTimer = new System.Timers.Timer(intervalMs);
        autoSendTimer.Elapsed += (s, e) =>
        {
            if (isOpened)
            {
                SendMessage($"{message} [自动#{sendCount + 1}]");
            }
        };
        autoSendTimer.Start();

        Console.WriteLine($"开始自动发送，间隔: {intervalMs}ms, 消息: {message}");
        return true;
    }

    /// <summary>
    /// 停止自动发送
    /// </summary>
    public void StopAutoSend()
    {
        autoSendTimer?.Stop();
        autoSendTimer?.Dispose();
        autoSendTimer = null;
    }

    /// <summary>
    /// 显示状态信息
    /// </summary>
    public void ShowStatus()
    {
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($"连接状态: {(isOpened ? "已打开" : "已关闭")}");
        Console.WriteLine($"接收状态: {(isReceiving ? "运行中" : "已停止")}");
        Console.WriteLine($"目标地址: {TargetAddress}");
        Console.WriteLine($"监听端口: {ListenPort}");
        Console.WriteLine($"发送统计: {sendCount} 条消息");
        Console.WriteLine($"接收统计: {receiveCount} 条消息");
        Console.WriteLine(new string('=', 50));
    }

    /// <summary>
    /// 显示最近的接收消息
    /// </summary>
    public void ShowRecentMessages(int count = 10)
    {
        Console.WriteLine($"最近 {count} 条接收消息:");
        Console.WriteLine(new string('-', 60));

        var messages = receivedMessages.ToArray();
        int start = Math.Max(0, messages.Length - count);

        for (int i = start; i < messages.Length; i++)
        {
            Console.WriteLine(messages[i]);
        }
    }

    /// <summary>
    /// 清空统计
    /// </summary>
    public void ClearStatistics()
    {
        sendCount = 0;
        receiveCount = 0;
        while (receivedMessages.TryDequeue(out _)) { }
        Console.WriteLine("[系统] 统计信息已清空");
    }

    private bool IsPrintableText(string text)
    {
        foreach (char c in text)
        {
            if (!char.IsControl(c) || c == '\r' || c == '\n' || c == '\t')
                continue;
            return false;
        }
        return true;
    }

    private byte[] HexStringToBytes(string hex)
    {
        hex = hex.Replace(" ", "").Replace("-", "");
        if (hex.Length % 2 != 0)
            throw new ArgumentException("十六进制字符串长度必须为偶数");

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    public void Dispose()
    {
        Close();
    }
}
