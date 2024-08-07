using System.Collections.Concurrent;
using System.IO.Ports;
namespace Wu.CommTool.Modules.ModbusRtu.Models;

public class MrtuSerialPort : IDisposable
{
    public MrtuDeviceManager Owner { get; set; }

    #region 字段
    SerialPort SerialPort = new();              //串口
    Task publishHandleTask; //发布消息处理线程
    Task receiveHandleTask; //接收消息处理线程
    EventWaitHandle WaitPublishFrameEnqueue = new AutoResetEvent(true); //等待发布消息入队
    EventWaitHandle WaitUartReceived = new AutoResetEvent(true); //接收到串口数据完成标志
    ConcurrentQueue<(string, int)> PublishFrameQueue = new();      //数据帧发送队列
    ConcurrentQueue<string> ReceiveFrameQueue = new();    //数据帧处理队列
    ComConfig comConfig; 
    #endregion

    public MrtuSerialPort(ComConfig config)
    {
        //配置串口
        SerialPort.PortName = config.ComPort.Port;                          //串口
        SerialPort.BaudRate = (int)config.BaudRate;                         //波特率
        SerialPort.Parity = (System.IO.Ports.Parity)config.Parity;          //校验
        SerialPort.DataBits = config.DataBits;                              //数据位
        SerialPort.StopBits = (System.IO.Ports.StopBits)config.StopBits;    //停止位
        SerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);//串口接收事件

        //数据帧处理子线程
        publishHandleTask = new Task(PublishFrame);
        receiveHandleTask = new Task(ReceiveFrame);
        publishHandleTask.Start();
        receiveHandleTask.Start();
        this.comConfig = config;
    }



    #region 串口操作
    /// <summary>
    /// 打开串口
    /// </summary>
    public void OpenSerialPort()
    {
        try
        {
            if (SerialPort.IsOpen)
            {
                return;
            }

            try
            {
                SerialPort.Open();               //打开串口
                Debug.WriteLine($"打开串口 {SerialPort.PortName} : {comConfig.ComPort.DeviceName}  波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// 关闭串口
    /// </summary>
    public void CloseSerialPort()
    {
        try
        {
            //若串口未开启则返回
            if (!SerialPort.IsOpen)
            {
                return;
            }

            Debug.WriteLine($"关闭串口{SerialPort.PortName}");
            SerialPort.Close();                   //关闭串口 
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message, MessageType.Error);
        }
    } 
    #endregion

    /// <summary>
    /// 接收串口消息 该方法必须必须必须使用同步不能用异步
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            //若串口未开启则返回
            if (SerialPort == null || !SerialPort.IsOpen)
            {
                SerialPort?.DiscardInBuffer();//丢弃接收缓冲区的数据
                return;
            }
            #region 接收数据
            List<byte> frameCache = []; //接收数据二次缓存 串口接收数据先缓存至此
            List<byte> frame = [];      //接收的数据帧
            bool isNot = false;         //前8字节不是一帧标志 不做标记将导致对响应帧多次重复校验
            string msg = string.Empty;  //
            int times = 0;              //计算次数 连续数ms无数据判断为一帧结束
            do
            {
                //若串口已被关闭则退出
                if (SerialPort.IsOpen == false)
                    return;
                times++;//计时
                //串口接收到新的数据时执行
                if (SerialPort.BytesToRead > 0)
                {
                    times = 0;                                       //重置等待时间
                    int dataCount = SerialPort.BytesToRead;          //获取串口缓存中的数据量
                    byte[] tempBuffer = new byte[dataCount];         //声明数组
                    SerialPort.Read(tempBuffer, 0, dataCount); //从串口缓存读取数据 从第0个读取n个字节, 写入tempBuffer 
                    frameCache.AddRange(tempBuffer);                 //添加进接收的数据缓存列表
                }
                //二级缓存frameCache中还有未处理完的数据
                if (frameCache.Count > 0)
                {
                    #region 根据功能码调整帧至正确的起始位置(由于数据中可能存在类似功能码的数据, 可能会有错误)
                    //做主站不需要该功能
                    //if (comConfig.AutoFrame == Enable.启用 && frameCache.Count >= 8 && (times > 1))
                    //{
                    //    //TODO 根据接收数据中功能码位置调整帧至正确的起始位置
                    //    //获取缓存中所有的功能码位置
                    //    var funcs = ModbusUtils.GetIndicesOfFunctions(frameCache);
                    //    //接收缓存至少2字节,且功能码至少1个

                    //    //将功能码调整至第二字节的位置
                    //    if (frameCache.Count >= 1 && funcs.Count > 0)
                    //    {
                    //        //若前2个功能码是连续的, 则第一个功能码应判定为地址
                    //        if (funcs.Count >= 2                         //有多个功能码
                    //            && (funcs[1] - funcs[0] == 1) //前两个功能码是连续的
                    //            && funcs[0] != 0)                   //第一字节不是地址
                    //        {
                    //            frame = frameCache.Take(funcs[0]).ToList();//将这一帧前面的输出
                    //            //输出接收到的数据
                    //            ReceiveFrameQueue.Enqueue(BitConverter.ToString(frame.ToArray()).Replace('-', ' '));//接收到的消息入队
                    //            frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的8字节
                    //            isNot = false;
                    //            continue;
                    //        }
                    //        //前2字节都没有功能码,则将功能码调整至第二字节
                    //        else if (funcs[0] > 2)
                    //        {
                    //            frame = frameCache.Take(funcs[0] - 1).ToList();//功能码前一个字节为地址要保留,所以要-1
                    //            //输出接收到的数据
                    //            ReceiveFrameQueue.Enqueue(BitConverter.ToString(frame.ToArray()).Replace('-', ' '));//接收到的消息入队
                    //            frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的8字节
                    //            isNot = false;
                    //            continue;
                    //        }
                    //    }
                    //}
                    #endregion

                    #region 防粘包处理 前8字节为请求帧的处理
                    //做主站不需要该功能

                    //由于监控串口网络时,请求帧和应答帧时间间隔较短,会照成接收粘包  通过先截取一段数据分析是否为请求帧,为请求帧则先解析
                    //0X01请求帧8字节 0x02请求帧8字节 0x03请求帧8字节 0x04请求帧8字节 0x05请求帧8字节  0x06请求帧8字节 0x0F请求帧数量不定 0x10请求帧数量不定
                    //由于大部分请求帧长度为8字节 故对接收字节前8字节截取校验判断是否为一帧可以解决大部分粘包问题

                    ////当二级缓存大于等于8字节时 对其进行crc校验,验证通过则为一帧
                    //if (!isNot && frameCache.Count >= 8)
                    //{
                    //    frame = frameCache.Take(8).ToList();   //截取frameCache前8个字节 对其进行crc校验,验证通过则为一帧
                    //    var crcOk = ModbusUtils.IsModbusCrcOk(frame);       //先验证前8字节是否能校验成功

                    //    #region TODO 这部分未完成
                    //    //TODO 0x03、0x04、0x10粘包问题已处理 其他功能码的未做
                    //    //若8字节校验未通过,则可能不是上述描述的请求帧,应根据对应帧的具体内容具体解析
                    //    if (!crcOk)
                    //    {
                    //        //0x10请求帧 帧长度需要根据帧的实际情况计算  长度=9+N  从站ID(1) 功能码(1) 起始地址(2) 寄存器数量(2) 字节数(1)  寄存器值(n) 校验码(2)
                    //        if (frame[1] == 0x10 && frameCache.Count >= (frame[6] + 9))
                    //        {
                    //            frame = frameCache.Take(frame[6] + 9).ToList();
                    //        }
                    //        else if (frame[1] == 0x10 && frameCache.Count < (frame[6] + 9))
                    //        {
                    //            //数据量不够则继续接收 不能用continue,否则无法执行程序最后的延时1ms
                    //        }

                    //        //0x03响应帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                    //        else if (frame[1] == 0x03 && frameCache.Count >= (frame[2] + 5))
                    //        {
                    //            frame = frameCache.Take(frame[2] + 5).ToList();
                    //        }
                    //        else if (frame[1] == 0x03 && frameCache.Count < (frame[2] + 5))
                    //        {
                    //            //数据量不够则继续接收 不能用continue,否则无法执行程序最后的延时1ms
                    //        }
                    //        //0x04响应帧   从站ID(1) 功能码(1) 字节数(1)  寄存器值(N*×2) 校验码(2)
                    //        else if (frame[1] == 0x04 && frameCache.Count >= (frame[2] + 5))
                    //        {
                    //            frame = frameCache.Take(frame[2] + 5).ToList();
                    //        }
                    //        else if (frame[1] == 0x04 && frameCache.Count < (frame[2] + 5))
                    //        {
                    //            //数据量不够则继续接收 不能用continue,否则无法执行程序最后的延时1ms
                    //        }

                    //        //解析出可能的帧并校验成功
                    //        if (frame.Count > 0 && ModbusUtils.IsModbusCrcOk(frame))
                    //        {
                    //            //输出接收到的数据
                    //            ReceiveFrameQueue.Enqueue(BitConverter.ToString(frame.ToArray()).Replace('-', ' '));//接收到的消息入队
                    //            frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的8字节
                    //            times = 0;                                     //重置计时器
                    //            continue;
                    //        }
                    //    }
                    //    #endregion

                    //    //CRC校验通过
                    //    if (crcOk)
                    //    {
                    //        //输出接收到的数据
                    //        ReceiveFrameQueue.Enqueue(BitConverter.ToString(frame.ToArray()).Replace('-', ' '));//接收到的消息入队
                    //        frameCache.RemoveRange(0, frame.Count);   //从缓存中移除已处理的8字节
                    //        times = 0;                                     //重置计时器
                    //    }
                    //    //验证失败,标记并不再重复校验
                    //    else
                    //    {
                    //        isNot = true;
                    //    }
                    //}
                    #endregion
                }

                //ModbusRtu标准协议 一帧最大长度是256字节
                //限制一次接收的最大数量 避免多设备连接时 导致数据收发无法判断帧结束
                if (frameCache.Count > comConfig.MaxLength)
                    break;
                Thread.Sleep(1);//同步等待
            } while (times < comConfig.TimeOut);
            #endregion

            msg = BitConverter.ToString(frameCache.ToArray());
            msg = msg.Replace('-', ' ');
            ReceiveFrameQueue.Enqueue(msg);//接收到的消息入队

            WaitUartReceived.Set();//置位数据接收完成标志
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message, MessageType.Receive);
        }
        finally
        {

        }
    }

    /// <summary>
    /// 执行串口发送帧
    /// </summary>
    private bool ExecutePublishMessage(string message)
    {
        throw new NotImplementedException();
        //try
        //{
        //    //发送数据不能为空
        //    if (message is null || message.Length.Equals(0))
        //    {
        //        ShowErrorMessage("发送的数据不能为空");
        //        return false;
        //    }

        //    //验证数据字符必须符合16进制
        //    Regex regex = new(@"^[0-9 a-f A-F -]*$");
        //    if (!regex.IsMatch(message))
        //    {
        //        ShowErrorMessage("数据字符仅限 0123456789 ABCDEF");
        //        return false;
        //    }

        //    byte[] data;
        //    try
        //    {
        //        data = message.Replace("-", string.Empty).GetBytes();
        //    }
        //    catch (Exception)
        //    {
        //        ShowErrorMessage($"数据转换16进制失败，发送数据位数量必须为偶数(16进制一个字节2位数)。");
        //        return false;
        //    }

        //    if (SerialPort.IsOpen)
        //    {
        //        try
        //        {
        //            if (!IsPause)
        //                ShowSendMessage(new ModbusRtuFrame(data));
        //            SerialPort.Write(data, 0, data.Length);     //发送数据
        //            SendBytesCount += data.Length;                    //统计发送数据总数

        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            ShowErrorMessage(ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        ShowErrorMessage("串口未打开");
        //    }
        //    return false;
        //}
        //catch (Exception ex)
        //{
        //    ShowErrorMessage(ex.Message);
        //    return false;
        //}

    }

    /// <summary>
    /// 发送数据帧处理线程
    /// </summary>
    private async void PublishFrame()
    {
        WaitPublishFrameEnqueue.Reset();
        while (true)
        {
            try
            {
                //判断队列是否不空 若为空则等待
                if (PublishFrameQueue.Count == 0)
                {
                    WaitPublishFrameEnqueue.WaitOne();
                    continue;//需要再次验证队列是否为空
                }
                //判断串口是否已打开,若已关闭则不执行
                if (comConfig.IsOpened)
                {
                    comConfig.IsSending = true;
                    PublishFrameQueue.TryDequeue(out var frame);  //出队 数据帧
                    ExecutePublishMessage(frame.Item1);              //请求发送数据帧
                    await Task.Delay(frame.Item2);            //等待一段时间
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {

            }
        }
    }


    /// <summary>
    /// 接收数据帧处理线程
    /// </summary>
    private void ReceiveFrame()
    {
        WaitUartReceived.Reset();
        while (true)
        {
            try
            {
                //若无消息需要处理则进入等待
                if (ReceiveFrameQueue.Count == 0)
                {
                    WaitUartReceived.WaitOne(); //等待接收消息
                }

                //从接收消息队列中取出一条消息
                ReceiveFrameQueue.TryDequeue(out var frame);
                if (string.IsNullOrWhiteSpace(frame))
                {
                    continue;
                }
                //实例化ModbusRtu帧
                var mFrame = new ModbusRtuFrame(frame.GetBytes());

                //对接收的消息直接进行crc校验
                var crc = Wu.Utils.Crc.Crc16Modbus(frame.GetBytes());   //校验码 校验通过的为0000

                #region 界面输出接收的消息 若校验成功则根据接收到内容输出不同的格式
                if (false)
                {
                    //若暂停更新显示则不输出
                }
                else if (mFrame.Type.Equals(ModbusRtuFrameType.校验失败))
                {
                    //ShowReceiveMessage(mFrame);

                    continue;
                }
                //校验成功
                else
                {
                    //ShowReceiveMessage(mFrame);
                }
                #endregion


                List<byte> frameList = frame.GetBytes().ToList();//将字符串类型的数据帧转换为字节列表
                int slaveId = frameList[0];                 //从站地址
                int func = frameList[1];                    //功能码

                #region 对接收的数据分功能码展示

                //TODO 解析接收的帧并赋值
                //03功能码或04功能码
                //if (mFrame.Type.Equals(ModbusRtuFrameType._0x03响应帧) || mFrame.Type.Equals(ModbusRtuFrameType._0x04响应帧))
                //{
                //    //若自动读取开启则解析接收的数据
                //    if (DataMonitorConfig.IsOpened)
                //    {
                //        //验证数据是否为请求的数据 根据 从站地址 功能码 数据字节数量
                //        if (frameList[0] == DataMonitorConfig.SlaveId && frameList[2] == DataMonitorConfig.Quantity * 2)
                //        {
                //            Analyse(frameList);
                //        }
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    //TODO Dispose
    public void Dispose()
    {
        
        try
        {
            CloseSerialPort();//关闭串口
        }
        catch (Exception ex)
        {
            //
        }
    }
}
