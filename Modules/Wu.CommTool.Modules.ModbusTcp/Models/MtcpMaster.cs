using NModbus.Logging;
using NModbus.Message;

namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// Modbus Tcp 从站=Master
/// </summary>
public partial class MtcpMaster : ObservableObject
{
    [ObservableProperty]
    bool isOnline;



    [RelayCommand]
    [property: JsonIgnore]
    async Task TestMaster()
    {
        try
        {
            #region modbus tcp 读取保持寄存器测试
            using TcpClient client = new TcpClient("127.0.0.1", 502);
            var factory = new ModbusFactory(logger: new DebugModbusLogger());
            IModbusMaster master = factory.CreateMaster(client);

            byte slaveId = 1;
            byte startAddress = 0;
            byte numberOfPoints = 5;

            //请求
            var request = new ReadHoldingInputRegistersRequest(
                    ModbusFunctionCodes.ReadHoldingRegisters,
                    slaveId,
                    startAddress,
                    numberOfPoints);

            var ccc = master.Transport.BuildMessageFrame(request);//生成 读取保持寄存器帧

            var aa = await master.ReadHoldingRegistersAsync(slaveId, startAddress, numberOfPoints);

            //var xx = master.
            #endregion
        }
        catch (Exception ex)
        {

        }
    }
}
