using Microsoft.Win32;

namespace Wu.CommTool.Modules.MrtuSlave.Models;

/// <summary>
/// 寄存器
/// </summary>
public partial class ModbusRegister : ObservableObject
{
    /// <summary>
    /// 寄存器地址
    /// </summary>
    [ObservableProperty]
    private ushort address;

    [ObservableProperty] private ushort value;

    [ObservableProperty] private string description;

    [ObservableProperty] private bool isCoil;

    public ModbusRegister(ushort address, string description, bool isCoil = false)
    {
        Address = address;
        Description = description;
        IsCoil = isCoil;
        Value = 0;
    }
}

/// <summary>
/// 保持寄存器
/// </summary>
public partial class HoldingRegisters : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ModbusRegister> registers;

    public HoldingRegisters()
    {
        Registers = [];
        InitializeRegisters();
    }

    /// <summary>
    /// 初始化寄存器
    /// </summary>
    private void InitializeRegisters()
    {
        for (ushort i = 0; i < 3000; i++)
        {
            Registers.Add(new ModbusRegister(i, $"", false));
        }
    }

    /// <summary>
    /// 读取寄存器值
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public ushort ReadRegister(ushort address)
    {
        var register = Registers.FirstOrDefault(r => r.Address == address);
        return register?.Value ?? 0;
    }

    /// <summary>
    /// 写入寄存器
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void WriteRegister(ushort address, ushort value)
    {
        var register = Registers.FirstOrDefault(r => r.Address == address);
        if (register != null)
        {
            register.Value = value;
        }
    }

    /// <summary>
    /// 读多个寄存器
    /// </summary>
    /// <param name="startAddress"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public ushort[] ReadRegisters(ushort startAddress, ushort count)
    {
        var values = new ushort[count];
        for (ushort i = 0; i < count; i++)
        {
            values[i] = ReadRegister((ushort)(startAddress + i));
        }
        return values;
    }

    /// <summary>
    /// 写入多个寄存器
    /// </summary>
    /// <param name="startAddress"></param>
    /// <param name="values"></param>
    public void WriteRegisters(ushort startAddress, ushort[] values)
    {
        for (ushort i = 0; i < values.Length; i++)
        {
            WriteRegister((ushort)(startAddress + i), values[i]);
        }
    }
}

/// <summary>
/// 输入寄存器
/// </summary>
public partial class InputRegisters : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ModbusRegister> registers;

    public InputRegisters()
    {
        Registers = [];
        InitializeRegisters();
    }

    /// <summary>
    /// 初始化寄存器
    /// </summary>
    private void InitializeRegisters()
    {
        for (ushort i = 0; i < 100; i++)
        {
            Registers.Add(new ModbusRegister(i, $"保持寄存器{i}", false));
        }
    }

    /// <summary>
    /// 读取寄存器值
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public ushort ReadRegister(ushort address)
    {
        var register = Registers.FirstOrDefault(r => r.Address == address);
        return register?.Value ?? 0;
    }

    /// <summary>
    /// 写入寄存器
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void WriteRegister(ushort address, ushort value)
    {
        var register = Registers.FirstOrDefault(r => r.Address == address);
        if (register != null)
        {
            register.Value = value;
        }
    }

    /// <summary>
    /// 读多个寄存器
    /// </summary>
    /// <param name="startAddress"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public ushort[] ReadRegisters(ushort startAddress, ushort count)
    {
        var values = new ushort[count];
        for (ushort i = 0; i < count; i++)
        {
            values[i] = ReadRegister((ushort)(startAddress + i));
        }
        return values;
    }

    /// <summary>
    /// 写入多个寄存器
    /// </summary>
    /// <param name="startAddress"></param>
    /// <param name="values"></param>
    public void WriteRegisters(ushort startAddress, ushort[] values)
    {
        for (ushort i = 0; i < values.Length; i++)
        {
            WriteRegister((ushort)(startAddress + i), values[i]);
        }
    }
}

/// <summary>
/// 线圈寄存器
/// </summary>
public partial class CoilRegisters : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ModbusRegister> coils;

    public CoilRegisters()
    {
        Coils = new ObservableCollection<ModbusRegister>();
        InitializeCoils();
    }

    private void InitializeCoils()
    {
        for (ushort i = 0; i < 100; i++)
        {
            Coils.Add(new ModbusRegister(i, $"线圈{i}", true));
        }
    }

    public bool ReadCoil(ushort address)
    {
        var coil = Coils.FirstOrDefault(c => c.Address == address);
        return coil?.Value != 0;
    }

    public void WriteCoil(ushort address, bool value)
    {
        var coil = Coils.FirstOrDefault(c => c.Address == address);
        if (coil != null)
        {
            coil.Value = value ? (ushort)1 : (ushort)0;
        }
    }

    public bool[] ReadCoils(ushort startAddress, ushort count)
    {
        var values = new bool[count];
        for (ushort i = 0; i < count; i++)
        {
            values[i] = ReadCoil((ushort)(startAddress + i));
        }
        return values;
    }

    public void WriteCoils(ushort startAddress, bool[] values)
    {
        for (ushort i = 0; i < values.Length; i++)
        {
            WriteCoil((ushort)(startAddress + i), values[i]);
        }
    }
}


/// <summary>
/// 协议解析
/// </summary>
public partial class ModbusRTUProtocol : ObservableObject
{
    // Modbus功能码
    private const byte READ_COILS = 0x01;
    private const byte READ_DISCRETE_INPUTS = 0x02;
    private const byte READ_HOLDING_REGISTERS = 0x03;
    private const byte READ_INPUT_REGISTERS = 0x04;
    private const byte WRITE_SINGLE_COIL = 0x05;
    private const byte WRITE_SINGLE_REGISTER = 0x06;
    private const byte WRITE_MULTIPLE_COILS = 0x0F;
    private const byte WRITE_MULTIPLE_REGISTERS = 0x10;

    private readonly HoldingRegisters holdingRegisters;
    private readonly CoilRegisters coilRegisters;

    public ModbusRTUProtocol(HoldingRegisters holdingRegisters, CoilRegisters coilRegisters)
    {
        this.holdingRegisters = holdingRegisters;
        this.coilRegisters = coilRegisters;
    }


    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public byte[] ProcessRequest(byte[] request)
    {
        if (request.Length < 4) return CreateErrorResponse(request[0], request[1], 0x01); // 非法功能码

        byte slaveAddress = request[0];
        byte functionCode = request[1];
        ushort startAddress = (ushort)((request[2] << 8) | request[3]);
        ushort quantity = (ushort)((request[4] << 8) | request[5]);

        try
        {
            return functionCode switch
            {
                READ_COILS => HandleReadCoils(slaveAddress, functionCode, startAddress, quantity),
                READ_HOLDING_REGISTERS => HandleReadHoldingRegisters(slaveAddress, functionCode, startAddress, quantity),
                WRITE_SINGLE_COIL => HandleWriteSingleCoil(slaveAddress, functionCode, startAddress, request),
                WRITE_SINGLE_REGISTER => HandleWriteSingleRegister(slaveAddress, functionCode, startAddress, request),
                WRITE_MULTIPLE_COILS => HandleWriteMultipleCoils(slaveAddress, functionCode, startAddress, request),
                WRITE_MULTIPLE_REGISTERS => HandleWriteMultipleRegisters(slaveAddress, functionCode, startAddress, request),
                _ => CreateErrorResponse(slaveAddress, functionCode, 0x01) // 非法功能码
            };
        }
        catch (Exception)
        {
            return CreateErrorResponse(slaveAddress, functionCode, 0x04); // 从站设备故障
        }
    }

    private byte[] HandleReadCoils(byte slaveAddress, byte functionCode, ushort startAddress, ushort quantity)
    {
        if (quantity < 1 || quantity > 2000)
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        var coilValues = coilRegisters.ReadCoils(startAddress, quantity);
        int byteCount = (quantity + 7) / 8;
        byte[] data = new byte[byteCount + 2]; // 从站地址 + 功能码 + 字节数 + 数据 + CRC

        data[0] = slaveAddress;
        data[1] = functionCode;
        data[2] = (byte)byteCount;

        for (int i = 0; i < quantity; i++)
        {
            if (coilValues[i])
            {
                data[3 + i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return AddCRC(data, 0, data.Length);
    }

    private byte[] HandleReadHoldingRegisters(byte slaveAddress, byte functionCode, ushort startAddress, ushort quantity)
    {
        if (quantity < 1 || quantity > 125)
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        var registerValues = holdingRegisters.ReadRegisters(startAddress, quantity);
        byte[] data = new byte[3 + quantity * 2]; // 从站地址 + 功能码 + 字节数 + 数据 + CRC

        data[0] = slaveAddress;
        data[1] = functionCode;
        data[2] = (byte)(quantity * 2);

        for (int i = 0; i < quantity; i++)
        {
            data[3 + i * 2] = (byte)(registerValues[i] >> 8);
            data[4 + i * 2] = (byte)(registerValues[i] & 0xFF);
        }

        return AddCRC(data, 0, data.Length);
    }

    private byte[] HandleWriteSingleCoil(byte slaveAddress, byte functionCode, ushort startAddress, byte[] request)
    {
        ushort value = (ushort)((request[4] << 8) | request[5]);
        bool coilValue = value == 0xFF00;

        coilRegisters.WriteCoil(startAddress, coilValue);

        // 返回相同的请求作为响应
        byte[] response = new byte[8];
        Array.Copy(request, 0, response, 0, 6);
        return AddCRC(response, 0, 6);
    }

    private byte[] HandleWriteSingleRegister(byte slaveAddress, byte functionCode, ushort startAddress, byte[] request)
    {
        ushort value = (ushort)((request[4] << 8) | request[5]);
        holdingRegisters.WriteRegister(startAddress, value);

        // 返回相同的请求作为响应
        byte[] response = new byte[8];
        Array.Copy(request, 0, response, 0, 6);
        return AddCRC(response, 0, 6);
    }

    private byte[] HandleWriteMultipleCoils(byte slaveAddress, byte functionCode, ushort startAddress, byte[] request)
    {
        ushort quantity = (ushort)((request[4] << 8) | request[5]);
        byte byteCount = request[6];

        bool[] values = new bool[quantity];
        for (int i = 0; i < quantity; i++)
        {
            int byteIndex = 7 + i / 8;
            int bitIndex = i % 8;
            values[i] = (request[byteIndex] & (1 << bitIndex)) != 0;
        }

        coilRegisters.WriteCoils(startAddress, values);

        // 返回确认响应
        byte[] response = new byte[8];
        response[0] = slaveAddress;
        response[1] = functionCode;
        response[2] = (byte)(startAddress >> 8);
        response[3] = (byte)(startAddress & 0xFF);
        response[4] = (byte)(quantity >> 8);
        response[5] = (byte)(quantity & 0xFF);

        return AddCRC(response, 0, 6);
    }

    private byte[] HandleWriteMultipleRegisters(byte slaveAddress, byte functionCode, ushort startAddress, byte[] request)
    {
        ushort quantity = (ushort)((request[4] << 8) | request[5]);
        byte byteCount = request[6];

        ushort[] values = new ushort[quantity];
        for (int i = 0; i < quantity; i++)
        {
            values[i] = (ushort)((request[7 + i * 2] << 8) | request[8 + i * 2]);
        }

        holdingRegisters.WriteRegisters(startAddress, values);

        // 返回确认响应
        byte[] response = new byte[8];
        response[0] = slaveAddress;
        response[1] = functionCode;
        response[2] = (byte)(startAddress >> 8);
        response[3] = (byte)(startAddress & 0xFF);
        response[4] = (byte)(quantity >> 8);
        response[5] = (byte)(quantity & 0xFF);

        return AddCRC(response, 0, 6);
    }

    private byte[] CreateErrorResponse(byte slaveAddress, byte functionCode, byte exceptionCode)
    {
        byte[] response = new byte[5];
        response[0] = slaveAddress;
        response[1] = (byte)(functionCode | 0x80); // 设置错误标志
        response[2] = exceptionCode;
        return AddCRC(response, 0, 3);
    }

    private byte[] AddCRC(byte[] data, int start, int length)
    {
        ushort crc = CalculateCRC(data, start, length);
        byte[] result = new byte[length + 2];
        Array.Copy(data, start, result, 0, length);
        result[length] = (byte)(crc & 0xFF);
        result[length + 1] = (byte)(crc >> 8);
        return result;
    }

    private ushort CalculateCRC(byte[] data, int start, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = start; i < start + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        return crc;
    }
}