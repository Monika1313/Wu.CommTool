namespace Wu.CommTool.Modules.MrtuSlave.Models;

/// <summary>
/// 寄存器
/// </summary>
public partial class ModbusRegister : ObservableObject
{
    /// <summary>
    /// 寄存器地址
    /// </summary>
    [ObservableProperty] private ushort address;

    /// <summary>
    /// 值
    /// </summary>
    [ObservableProperty] private ushort value;

    /// <summary>
    /// 描述 名称
    /// </summary>
    [ObservableProperty] private string description;

    #region 线圈寄存器
    /// <summary>
    /// 线圈 值不为0就是1
    /// </summary>
    [ObservableProperty] private bool isCoil;


    [RelayCommand]
    private void SetTrue()
    {
        if (IsCoil)
        {
            Value = 1;
        }
    }

    [RelayCommand]
    private void SetFalse()
    {
        if (IsCoil)
        {
            Value = 0;
        }
    } 
    #endregion

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
public partial class InputRegisters : HoldingRegisters { }

/// <summary>
/// 线圈寄存器
/// </summary>
public partial class CoilRegisters : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ModbusRegister> coils;

    public CoilRegisters()
    {
        Coils = [];
        InitializeCoils();
    }

    private void InitializeCoils()
    {
        for (ushort i = 0; i < 3000; i++)
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
public partial class MrtuProtocol : ObservableObject
{
    private readonly HoldingRegisters holdingRegisters;//保持寄存器
    private readonly InputRegisters inputRegisters;//输入寄存器
    private readonly CoilRegisters coilRegisters;//线圈寄存器

    public MrtuProtocol(HoldingRegisters holdingRegisters, InputRegisters inputRegisters, CoilRegisters coilRegisters)
    {
        this.holdingRegisters = holdingRegisters;
        this.inputRegisters = inputRegisters;
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
            var funCode = (ModbusRtuFunctionCode)functionCode; //功能码转换enum

            return funCode switch
            {
                ModbusRtuFunctionCode._0x01 => HandleReadCoils(slaveAddress, functionCode, startAddress, quantity),
                ModbusRtuFunctionCode._0x03 => HandleReadHoldingRegisters(slaveAddress, functionCode, startAddress, quantity),
                ModbusRtuFunctionCode._0x04 => HandleReadInputRegisters(slaveAddress, functionCode, startAddress, quantity),
                ModbusRtuFunctionCode._0x05 => HandleWriteSingleCoil(slaveAddress, functionCode, startAddress, request),
                ModbusRtuFunctionCode._0x06 => HandleWriteSingleHoldingRegister(slaveAddress, functionCode, startAddress, request),
                ModbusRtuFunctionCode._0x0F => HandleWriteMultipleCoils(slaveAddress, functionCode, startAddress, request),
                ModbusRtuFunctionCode._0x10 => HandleWriteMultipleHoldingRegisters(slaveAddress, functionCode, startAddress, request),
                //ModbusRtuFunctionCode._0x02 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x86 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x8F => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x81 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x82 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x83 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x84 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x85 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x90 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x14 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x94 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x15 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x95 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x16 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x96 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x17 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x97 => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0x2B => throw new NotImplementedException(),
                //ModbusRtuFunctionCode._0xAB => throw new NotImplementedException(),
                _ => CreateErrorResponse(slaveAddress, functionCode, 0x01) // 非法功能码
            };
        }
        catch (Exception)
        {
            return CreateErrorResponse(slaveAddress, functionCode, 0x04); // 从站设备故障
        }
    }

    #region 处理 线圈寄存器
    /// <summary>
    /// 读线圈
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    private byte[] HandleReadCoils(byte slaveAddress, byte functionCode, ushort startAddress, ushort quantity)
    {
        if (quantity < 1 || quantity > 2000)
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        var coilValues = coilRegisters.ReadCoils(startAddress, quantity);
        int byteCount = (quantity + 7) / 8;

        // 修正：响应格式 = 从站地址(1) + 功能码(1) + 字节数(1) + 数据(byteCount)
        byte[] data = new byte[3 + byteCount];

        data[0] = slaveAddress;
        data[1] = functionCode;
        data[2] = (byte)byteCount;

        // 修正：正确的字节填充逻辑
        for (int i = 0; i < quantity; i++)
        {
            if (coilValues[i])
            {
                int byteIndex = 3 + i / 8;  // 数据从第3字节开始
                int bitIndex = i % 8;
                data[byteIndex] |= (byte)(1 << bitIndex);
            }
        }

        // 修正：CRC计算应该只包含数据部分（不包括CRC自身）
        return AddCRC(data, 0, 3 + byteCount);
    }
    /// <summary>
    /// 写单个线圈
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="request"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 处理写多个线圈
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="request"></param>
    /// <returns></returns>
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
    #endregion


    #region 处理保持寄存器
    /// <summary>
    /// 读保持寄存器
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 处理写单个保持寄存器
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    private byte[] HandleWriteSingleHoldingRegister(byte slaveAddress, byte functionCode, ushort startAddress, byte[] request)
    {
        ushort value = (ushort)((request[4] << 8) | request[5]);
        holdingRegisters.WriteRegister(startAddress, value);

        // 返回相同的请求作为响应
        byte[] response = new byte[8];
        Array.Copy(request, 0, response, 0, 6);
        return AddCRC(response, 0, 6);
    }

    /// <summary>
    /// 处理写多个保持寄存器
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    private byte[] HandleWriteMultipleHoldingRegisters(byte slaveAddress, byte functionCode, ushort startAddress, byte[] request)
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

    #endregion

    #region 处理 输入寄存器
    /// <summary>
    /// 读保持寄存器
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="startAddress"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    private byte[] HandleReadInputRegisters(byte slaveAddress, byte functionCode, ushort startAddress, ushort quantity)
    {
        if (quantity < 1 || quantity > 125)
            return CreateErrorResponse(slaveAddress, functionCode, 0x04); // 非法数据值

        var registerValues = inputRegisters.ReadRegisters(startAddress, quantity);
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
    #endregion


    /// <summary>
    /// 错误应答
    /// </summary>
    /// <param name="slaveAddress"></param>
    /// <param name="functionCode"></param>
    /// <param name="exceptionCode"></param>
    /// <returns></returns>
    private byte[] CreateErrorResponse(byte slaveAddress, byte functionCode, byte exceptionCode)
    {
        byte[] response = new byte[5];
        response[0] = slaveAddress;
        response[1] = (byte)(functionCode | 0x80); // 设置错误标志
        response[2] = exceptionCode;
        return AddCRC(response, 0, 3);
    }

    #region CRC
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
    #endregion
}