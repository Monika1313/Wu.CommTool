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
            var factory = new ModbusFactory();
            IModbusMaster master = factory.CreateMaster(client);
            byte slaveId = 1;
            var aa = await master.ReadHoldingRegistersAsync(slaveId, 0, 5);
            #endregion
        }
        catch (Exception ex)
        {

        }
    }
}
