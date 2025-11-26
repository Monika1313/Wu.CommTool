namespace Wu.CommTool.Modules.MrtuSlave.Models;

/// <summary>
/// 数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum DataType
{
    /// <summary>
    /// 16位无符号整数
    /// </summary>
    UInt16,

    /// <summary>
    /// 16位有符号整数
    /// </summary>
    Int16,

    /// <summary>
    /// 32位无符号整数
    /// </summary>
    UInt32,

    /// <summary>
    /// 32位有符号整数
    /// </summary>
    Int32,

    /// <summary>
    /// 32位浮点数
    /// </summary>
    Float,

    /// <summary>
    /// 64位浮点数
    /// </summary>
    Double
}

/// <summary>
/// 字节序
/// </summary>
public enum Endianness
{
    /// <summary>
    /// 大端字节序
    /// </summary>
    BigEndian,

    /// <summary>
    /// 小端字节序
    /// </summary>
    LittleEndian
}

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
    /// 原始值
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActualValue))]
    ushort value;

    public object ActualValue
    {
        get => GetActualValue();
        set
        {
            SetActualValue(value);
            //OnPropertyChanged(nameof(Value));
        }
    }


    /// <summary>
    /// 描述 名称
    /// </summary>
    [ObservableProperty] private string description;

    /// <summary>
    /// 数据类型
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActualValue))]
    DataType dataType;

    /// <summary>
    /// 字节序
    /// </summary>
    [ObservableProperty] private Endianness endianness;

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

    // 用于访问相邻寄存器的回调函数
    private Func<ushort, ushort> readRegisterCallback;
    private Action<ushort, ushort> writeRegisterCallback;

    public ModbusRegister(ushort address, string description, bool isCoil = false,
                         DataType dataType = DataType.UInt16, Endianness endianness = Endianness.BigEndian,
                         Func<ushort, ushort> readRegisterCallback = null, Action<ushort, ushort> writeRegisterCallback = null)
    {
        Address = address;
        Description = description;
        IsCoil = isCoil;
        DataType = dataType;
        Endianness = endianness;
        this.readRegisterCallback = readRegisterCallback;
        this.writeRegisterCallback = writeRegisterCallback;
        Value = 0;
    }



    /// <summary>
    /// 获取实际值（根据数据类型转换）
    /// </summary>
    public object GetActualValue()
    {
        return DataConverter.GetValue(this, readRegisterCallback);
    }

    /// <summary>
    /// 设置实际值（根据数据类型转换）
    /// </summary>
    public void SetActualValue(object value)
    {
        DataConverter.SetValue(this, value, writeRegisterCallback);
    }
}

/// <summary>
/// 数据转换器
/// </summary>
public static class DataConverter
{
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static object GetValue(ModbusRegister register, Func<ushort, ushort> readRegisterCallback)
    {
        return GetValue(register.Value, register.DataType, register.Endianness,
                       register.Address, readRegisterCallback);
    }

    /// <summary>
    /// 根据数据类型设置实际值
    /// </summary>
    public static void SetValue(ModbusRegister register, object value, Action<ushort, ushort> writeRegisterCallback)
    {
        SetValue(value, register.DataType, register.Endianness,
                register.Address, writeRegisterCallback);
    }

    /// <summary>
    /// 通用值获取方法
    /// </summary>
    public static object GetValue(ushort baseValue, DataType dataType, Endianness endianness,
                                 ushort baseAddress, Func<ushort, ushort> getRegisterValue)
    {
        if (getRegisterValue == null)
            return baseValue;

        switch (dataType)
        {
            case DataType.UInt16:
                return baseValue;

            case DataType.Int16:
                return (short)baseValue;

            case DataType.UInt32:
                var uint32Bytes = Get32BitBytes(baseAddress, getRegisterValue, endianness);
                return BitConverter.ToUInt32(uint32Bytes, 0);

            case DataType.Int32:
                var int32Bytes = Get32BitBytes(baseAddress, getRegisterValue, endianness);
                return BitConverter.ToInt32(int32Bytes, 0);

            case DataType.Float:
                var floatBytes = Get32BitBytes(baseAddress, getRegisterValue, endianness);
                return BitConverter.ToSingle(floatBytes, 0);

            case DataType.Double:
                var doubleBytes = Get64BitBytes(baseAddress, getRegisterValue, endianness);
                return BitConverter.ToDouble(doubleBytes, 0);

            default:
                return baseValue;
        }
    }

    /// <summary>
    /// 通用值设置方法
    /// </summary>
    public static void SetValue(object value, DataType dataType, Endianness endianness,
                               ushort baseAddress, Action<ushort, ushort> setRegisterValue)
    {
        if (setRegisterValue == null)
            return;

        switch (dataType)
        {
            case DataType.UInt16:
                setRegisterValue(baseAddress, Convert.ToUInt16(value));
                break;

            case DataType.Int16:
                setRegisterValue(baseAddress, (ushort)Convert.ToInt16(value));
                break;

            case DataType.UInt32:
                var uint32Bytes = BitConverter.GetBytes(Convert.ToUInt32(value));
                Set32BitBytes(uint32Bytes, baseAddress, setRegisterValue, endianness);
                break;

            case DataType.Int32:
                var int32Bytes = BitConverter.GetBytes(Convert.ToInt32(value));
                Set32BitBytes(int32Bytes, baseAddress, setRegisterValue, endianness);
                break;

            case DataType.Float:
                var floatBytes = BitConverter.GetBytes(Convert.ToSingle(value));
                Set32BitBytes(floatBytes, baseAddress, setRegisterValue, endianness);
                break;

            case DataType.Double:
                var doubleBytes = BitConverter.GetBytes(Convert.ToDouble(value));
                Set64BitBytes(doubleBytes, baseAddress, setRegisterValue, endianness);
                break;
        }
    }

    private static byte[] Get32BitBytes(ushort baseAddress, Func<ushort, ushort> getRegisterValue, Endianness endianness)
    {
        var bytes = new byte[4];
        var value1 = getRegisterValue(baseAddress);
        var value2 = getRegisterValue((ushort)(baseAddress + 1));

        if (endianness == Endianness.BigEndian)
        {
            bytes[0] = (byte)(value1 >> 8);
            bytes[1] = (byte)(value1 & 0xFF);
            bytes[2] = (byte)(value2 >> 8);
            bytes[3] = (byte)(value2 & 0xFF);
        }
        else
        {
            bytes[0] = (byte)(value2 & 0xFF);
            bytes[1] = (byte)(value2 >> 8);
            bytes[2] = (byte)(value1 & 0xFF);
            bytes[3] = (byte)(value1 >> 8);
        }

        return bytes;
    }

    private static byte[] Get64BitBytes(ushort baseAddress, Func<ushort, ushort> getRegisterValue, Endianness endianness)
    {
        var bytes = new byte[8];
        var values = new ushort[4];
        for (int i = 0; i < 4; i++)
        {
            values[i] = getRegisterValue((ushort)(baseAddress + i));
        }

        if (endianness == Endianness.BigEndian)
        {
            for (int i = 0; i < 4; i++)
            {
                bytes[i * 2] = (byte)(values[i] >> 8);
                bytes[i * 2 + 1] = (byte)(values[i] & 0xFF);
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                bytes[(3 - i) * 2] = (byte)(values[i] & 0xFF);
                bytes[(3 - i) * 2 + 1] = (byte)(values[i] >> 8);
            }
        }

        return bytes;
    }

    private static void Set32BitBytes(byte[] bytes, ushort baseAddress, Action<ushort, ushort> setRegisterValue, Endianness endianness)
    {
        ushort value1, value2;

        if (endianness == Endianness.BigEndian)
        {
            value1 = (ushort)((bytes[0] << 8) | bytes[1]);
            value2 = (ushort)((bytes[2] << 8) | bytes[3]);
        }
        else
        {
            value1 = (ushort)((bytes[2] << 8) | bytes[3]);
            value2 = (ushort)((bytes[0] << 8) | bytes[1]);
        }

        setRegisterValue(baseAddress, value1);
        setRegisterValue((ushort)(baseAddress + 1), value2);
    }

    private static void Set64BitBytes(byte[] bytes, ushort baseAddress, Action<ushort, ushort> setRegisterValue, Endianness endianness)
    {
        var values = new ushort[4];

        if (endianness == Endianness.BigEndian)
        {
            for (int i = 0; i < 4; i++)
            {
                values[i] = (ushort)((bytes[i * 2] << 8) | bytes[i * 2 + 1]);
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                values[i] = (ushort)((bytes[(3 - i) * 2 + 1] << 8) | bytes[(3 - i) * 2]);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            setRegisterValue((ushort)(baseAddress + i), values[i]);
        }
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
            Registers.Add(new ModbusRegister(i, $"", false,
                DataType.UInt16, Endianness.BigEndian,
                ReadRegister, WriteRegister));
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

    /// <summary>
    /// 获取寄存器的实际值（根据数据类型转换）
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public object GetRegisterActualValue(ushort address)
    {
        var register = Registers.FirstOrDefault(r => r.Address == address);
        return register?.GetActualValue() ?? 0;
    }

    /// <summary>
    /// 设置寄存器的实际值（根据数据类型转换）
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void SetRegisterActualValue(ushort address, object value)
    {
        var register = Registers.FirstOrDefault(r => r.Address == address);
        register?.SetActualValue(value);
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