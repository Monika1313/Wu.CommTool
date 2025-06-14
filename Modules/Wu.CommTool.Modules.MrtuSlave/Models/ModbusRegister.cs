namespace Wu.CommTool.Modules.MrtuSlave.Models;

public abstract class ModbusRegister
{
    public ushort Address { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public bool IsReadOnly { get; protected set; }

    public abstract object GetValue();
    public abstract void SetValue(object value);

    protected ModbusRegister(ushort address, string name, string description, bool isReadOnly)
    {
        Address = address;
        Name = name;
        Description = description;
        IsReadOnly = isReadOnly;
    }
}


public class CoilRegister : ModbusRegister
{
    private bool _value;

    public CoilRegister(ushort address, string name, string description, bool initialValue = false)
        : base(address, name, description, false)
    {
        _value = initialValue;
    }

    public override object GetValue() => _value;

    public override void SetValue(object value)
    {
        if (value is bool boolValue)
        {
            _value = boolValue;
        }
        else
        {
            throw new ArgumentException("Value must be of type bool");
        }
    }

    // 便捷方法
    public bool Value
    {
        get => _value;
        set => _value = value;
    }
}


public class DiscreteInputRegister : ModbusRegister
{
    private bool _value;

    public DiscreteInputRegister(ushort address, string name, string description, bool initialValue = false)
        : base(address, name, description, true)
    {
        _value = initialValue;
    }

    public override object GetValue() => _value;

    public override void SetValue(object value)
    {
        throw new InvalidOperationException("Discrete input registers are read-only");
    }

    // 内部设置方法，供设备驱动使用
    public void UpdateValue(bool value) => _value = value;

    public bool Value => _value;
}

public class HoldingRegister : ModbusRegister
{
    private ushort _value;

    public HoldingRegister(ushort address, string name, string description, ushort initialValue = 0)
        : base(address, name, description, false)
    {
        _value = initialValue;
    }

    public override object GetValue() => _value;

    public override void SetValue(object value)
    {
        if (value is ushort ushortValue)
        {
            _value = ushortValue;
        }
        else
        {
            throw new ArgumentException("Value must be of type ushort");
        }
    }

    // 便捷方法
    public ushort Value
    {
        get => _value;
        set => _value = value;
    }
}

public class InputRegister : ModbusRegister
{
    private ushort _value;

    public InputRegister(ushort address, string name, string description, ushort initialValue = 0)
        : base(address, name, description, true)
    {
        _value = initialValue;
    }

    public override object GetValue() => _value;

    public override void SetValue(object value)
    {
        throw new InvalidOperationException("Input registers are read-only");
    }

    // 内部设置方法，供设备驱动使用
    public void UpdateValue(ushort value) => _value = value;

    public ushort Value => _value;
}


public class ModbusRegisterCollection
{
    private readonly Dictionary<ushort, ModbusRegister> _registers = new Dictionary<ushort, ModbusRegister>();
    private readonly Dictionary<string, ModbusRegister> _namedRegisters = new Dictionary<string, ModbusRegister>();

    public void AddRegister(ModbusRegister register)
    {
        if (_registers.ContainsKey(register.Address))
        {
            throw new ArgumentException($"Register address {register.Address} already exists");
        }

        _registers.Add(register.Address, register);
        _namedRegisters.Add(register.Name, register);
    }

    public ModbusRegister GetRegisterByAddress(ushort address)
    {
        return _registers.TryGetValue(address, out var register) ? register : null;
    }

    public ModbusRegister GetRegisterByName(string name)
    {
        return _namedRegisters.TryGetValue(name, out var register) ? register : null;
    }

    public IEnumerable<ModbusRegister> GetAllRegisters()
    {
        return _registers.Values.OrderBy(r => r.Address);
    }

    public bool TryReadCoil(ushort address, out bool value)
    {
        if (GetRegisterByAddress(address) is CoilRegister coil)
        {
            value = coil.Value;
            return true;
        }
        value = false;
        return false;
    }

    public bool TryWriteCoil(ushort address, bool value)
    {
        if (GetRegisterByAddress(address) is CoilRegister coil && !coil.IsReadOnly)
        {
            coil.Value = value;
            return true;
        }
        return false;
    }

    // 类似方法实现其他寄存器类型的读写...
}