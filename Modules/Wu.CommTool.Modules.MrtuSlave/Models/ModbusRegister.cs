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
