using System.IO.Ports;

namespace Wu.CommTool.Modules.ModbusRtu.Models;

public class MrtuSerialPort
{
    public MrtuSerialPort()
    {
        
    }
    public MrtuSerialPort(ComPort comPort)
    {
        ////配置串口
        //SerialPort.PortName = comPort.Port;                              //串口
        //SerialPort.BaudRate = (int)ComConfig.BaudRate;                         //波特率
        //SerialPort.Parity = (System.IO.Ports.Parity)ComConfig.Parity;          //校验
        //SerialPort.DataBits = ComConfig.DataBits;                              //数据位
        //SerialPort.StopBits = (System.IO.Ports.StopBits)ComConfig.StopBits;    //停止位
    }
    public SerialPort SerialPort = new();              //串口

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
                //ShowMessage($"打开串口 {SerialPort.PortName} : {ComConfig.ComPort.DeviceName}  波特率: {SerialPort.BaudRate} 校验: {SerialPort.Parity}");
            }
            catch (Exception ex)
            {
                //HcGrowlExtensions.Warning($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", nameof(ModbusRtuView));
                //ShowMessage($"打开串口失败, 该串口设备不存在或已被占用。{ex.Message}", MessageType.Error);
                return;
            }
        }
        catch (Exception ex)
        {
            //ShowMessage(ex.Message, MessageType.Error);
        }
    }

    /// <summary>
    /// 关闭串口
    /// </summary>
    public async void CloseSerialPort()
    {
        try
        {
            //若串口未开启则返回
            if (!SerialPort.IsOpen)
            {
                return;
            }

            //ShowMessage($"关闭串口{SerialPort.PortName}");
            SerialPort.Close();                   //关闭串口 
        }
        catch (Exception ex)
        {
            //ShowMessage(ex.Message, MessageType.Error);
        }
    }
}
