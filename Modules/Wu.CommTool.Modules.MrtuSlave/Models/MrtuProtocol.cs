namespace Wu.CommTool.Modules.MrtuSlave.Models;

/// <summary>
/// Modbus RTU 协议解析
/// 模拟的是大端设备
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

        byte[] data = new byte[3 + byteCount];

        data[0] = slaveAddress;
        data[1] = functionCode;
        data[2] = (byte)byteCount;

        for (int i = 0; i < quantity; i++)
        {
            if (coilValues[i])
            {
                int byteIndex = 3 + i / 8;
                int bitIndex = i % 8;
                data[byteIndex] |= (byte)(1 << bitIndex);
            }
        }

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
        // 读取的寄存器数量范围∈[1,125]
        if (quantity < 1 || quantity > 125)
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        // 检查数据类型边界 - 确保不会读取跨数据类型的寄存器
        if (!ValidateDataTypeBoundaries(startAddress, quantity))
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        // 读取寄存器值
        var registerValues = holdingRegisters.ReadRegisters(startAddress, quantity);

        // 构建响应数据
        byte[] data = new byte[3 + quantity * 2];

        data[0] = slaveAddress;
        data[1] = functionCode;
        data[2] = (byte)(quantity * 2);

        // 填充寄存器值
        for (int i = 0; i < quantity; i++)
        {
            // 高字节在前，低字节在后
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

        // 获取寄存器信息
        var register = holdingRegisters.Registers.FirstOrDefault(r => r.Address == startAddress);
        if (register != null)
        {
            // 根据数据类型处理写入
            if (register.DataType == DataType.UInt16 || register.DataType == DataType.Int16)
            {
                // 16位数据类型直接写入
                holdingRegisters.WriteRegister(startAddress, value);
            }
            else
            {
                // 对于32位和64位数据类型，只写入部分数据可能不完整
                // 这里保持原有行为，写入单个寄存器值
                holdingRegisters.WriteRegister(startAddress, value);
            }
        }
        else
        {
            holdingRegisters.WriteRegister(startAddress, value);
        }

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

        // 检查数据类型边界
        if (!ValidateDataTypeBoundaries(startAddress, quantity))
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        ushort[] values = new ushort[quantity];
        for (int i = 0; i < quantity; i++)
        {
            values[i] = (ushort)((request[7 + i * 2] << 8) | request[8 + i * 2]);
        }

        holdingRegisters.WriteRegisters(startAddress, values);

        byte[] response = new byte[8];
        response[0] = slaveAddress;
        response[1] = functionCode;
        response[2] = (byte)(startAddress >> 8);
        response[3] = (byte)(startAddress & 0xFF);
        response[4] = (byte)(quantity >> 8);
        response[5] = (byte)(quantity & 0xFF);

        return AddCRC(response, 0, 6);
    }

    /// <summary>
    /// 验证数据类型边界，确保不会跨数据类型读取/写入
    /// </summary>
    /// <param name="startAddress">起始地址</param>
    /// <param name="quantity">数量</param>
    /// <returns>是否有效</returns>
    private bool ValidateDataTypeBoundaries(ushort startAddress, ushort quantity)
    {
        if (quantity == 1) return true; // 单个寄存器总是有效的

        // 检查起始地址的寄存器类型
        var startRegister = holdingRegisters.Registers.FirstOrDefault(r => r.Address == startAddress);
        if (startRegister == null) return true;

        // 根据数据类型检查边界
        int requiredRegisters = GetRequiredRegistersForDataType(startRegister.DataType);
        if (requiredRegisters > 1)
        {
            // 对于多寄存器数据类型，确保读取范围不跨越数据类型边界
            if (quantity > requiredRegisters)
            {
                // 如果读取数量超过数据类型所需寄存器数，检查是否跨越了不同类型
                for (ushort i = 0; i < quantity; i++)
                {
                    var currentRegister = holdingRegisters.Registers.FirstOrDefault(r => r.Address == (ushort)(startAddress + i));
                    if (currentRegister == null) continue;

                    // 如果遇到不同类型的数据，且不是起始数据类型的延续，则无效
                    if (currentRegister.DataType != startRegister.DataType &&
                        i < GetRequiredRegistersForDataType(currentRegister.DataType))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 获取数据类型所需的寄存器数量
    /// </summary>
    /// <param name="dataType">数据类型</param>
    /// <returns>所需寄存器数量</returns>
    private int GetRequiredRegistersForDataType(DataType dataType)
    {
        return dataType switch
        {
            DataType.UInt16 => 1,
            DataType.Int16 => 1,
            DataType.UInt32 => 2,
            DataType.Int32 => 2,
            DataType.Float => 2,
            DataType.Double => 4,
            _ => 1
        };
    }
    #endregion

    #region 处理 输入寄存器
    /// <summary>
    /// 读输入寄存器
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

        // 检查数据类型边界
        if (!ValidateDataTypeBoundaries(startAddress, quantity))
            return CreateErrorResponse(slaveAddress, functionCode, 0x03); // 非法数据值

        var registerValues = inputRegisters.ReadRegisters(startAddress, quantity);
        byte[] data = new byte[3 + quantity * 2];

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
        response[1] = (byte)(functionCode | 0x80);
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
