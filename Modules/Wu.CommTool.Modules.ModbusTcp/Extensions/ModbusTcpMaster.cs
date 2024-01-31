using NModbus.Device;

namespace Wu.CommTool.Modules.ModbusTcp.Extensions;

public class ModbusTcpMaster : ModbusIpMaster
{
    public ModbusTcpMaster(IModbusTransport transport) : base(transport)
    {

    }
}
