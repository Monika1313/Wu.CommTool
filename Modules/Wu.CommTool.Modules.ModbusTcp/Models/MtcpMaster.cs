using NModbus;
using System.Net;
using System.Net.Sockets;

namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Modbus Tcp 从站=Master
/// </summary>
public partial class MtcpMaster : ObservableObject
{

    [ObservableProperty]
    bool isOnline;

    void xxx()
    {
        //TcpClient client;
        //IModbusMaster master;
        //IModbusFactory factory;
        //factory = new ModbusFactory();
        //client = new TcpClient();
        //client.Connect("127.0.0.1", 502);
        //master = factory.CreateMaster(client);
    }

    void 创建Master()
    {
        try
        {
            //IModbusFactory mf = new ModbusFactory();
            //TcpListener tcpListener = new(IPAddress.Any, 502);
            //tcpListener.Start();
            //IModbusTcpSlaveNetwork xx = mf.CreateSlaveNetwork(tcpListener);
            //var aa = mf.create
            //var xxx = mf.CreateSlaveNetwork(tcpListener);


        }
        catch (Exception ex)
        {

        }

    }




}
