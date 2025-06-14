//namespace Wu.CommTool.Modules.MrtuSlave.Models;

/////// <summary>
/////// Modbus寄存器区域
/////// </summary>
////public partial class ModbusRegisterArea : ObservableObject
////{
////    List<ModbusRegister> registers = new List<ModbusRegister>();

////    private ModbusRegister CreateModbusRegister()
////    {
////        var register = new ModbusRegister(this);
////        registers.Add(register);
////        return register;
////    }


////    public ModbusRegisterArea()
////    {
////        // 初始化寄存器区域时可以添加一些默认的寄存器
////        for (int i = 0; i < 10; i++)
////        {
////            CreateModbusRegister();
////        }
////    }
////}

/////// <summary>
/////// Modbus寄存器
/////// </summary>
////public partial class ModbusRegister : ObservableObject
////{
////    private readonly ModbusRegisterArea owner;

////    public ModbusRegister(ModbusRegisterArea modbusRegisterArea)
////    {
////            owner = modbusRegisterArea;
////    }
////}


///// <summary>
///// Modbus寄存器区域，管理一组Modbus寄存器
///// </summary>
//public partial class ModbusRegisterArea : ObservableObject
//{
//    private List<ushort> registers = new List<ushort>();
//    private readonly List<ModbusRegister> _registers = new List<ModbusRegister>();

//    /// <summary>
//    /// 获取寄存器数量
//    /// </summary>
//    public int Count => _registers.Count;

//    /// <summary>
//    /// 区域名称
//    /// </summary>
//    [ObservableProperty] string name = "Modbus Register Area";

//    /// <summary>
//    /// 获取只读的寄存器集合
//    /// </summary>
//    public IReadOnlyList<ModbusRegister> Registers => _registers.AsReadOnly();

//    /// <summary>
//    /// 当寄存器值改变时触发的事件
//    /// </summary>
//    public event EventHandler<RegisterChangedEventArgs>? RegisterChanged;

//    #region 构造函数
//    public ModbusRegisterArea()
//    {
//        // 初始化10个寄存器
//        Expand(10);
//    }

//    public ModbusRegisterArea(int initialSize)
//    {
//        if (initialSize < 0)
//            throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size cannot be negative");

//        Expand(initialSize);
//    }
//    #endregion

//    /// <summary>
//    /// 扩展寄存器区域
//    /// </summary>
//    /// <param name="count">要添加的寄存器数量</param>
//    public void Expand(int count)
//    {
//        if (count <= 0)
//            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive");

//        for (int i = 0; i < count; i++)
//        {
//            CreateModbusRegister();
//        }
//    }

//    /// <summary>
//    /// 创建新的Modbus寄存器并添加到区域中
//    /// </summary>
//    private ModbusRegister CreateModbusRegister()
//    {
//        var register = new ModbusRegister(this, _registers.Count);
//        register.ValueChanged += OnRegisterValueChanged;
//        _registers.Add(register);
//        return register;
//    }

//    /// <summary>
//    /// 获取指定地址的寄存器
//    /// </summary>
//    public ModbusRegister this[int address]
//    {
//        get
//        {
//            if (address < 0 || address >= _registers.Count)
//                throw new ArgumentOutOfRangeException(nameof(address), $"Register address {address} is out of range");

//            return _registers[address];
//        }
//    }

//    /// <summary>
//    /// 批量读取寄存器值
//    /// </summary>
//    public ushort[] ReadRegisters(int startingAddress, int count)
//    {
//        if (startingAddress < 0 || startingAddress + count > _registers.Count)
//            throw new ArgumentOutOfRangeException($"Requested register range is invalid");

//        ushort[] values = new ushort[count];
//        for (int i = 0; i < count; i++)
//        {
//            values[i] = _registers[startingAddress + i].Value;
//        }
//        return values;
//    }

//    /// <summary>
//    /// 批量写入寄存器值
//    /// </summary>
//    public void WriteRegisters(int startingAddress, ushort[] values)
//    {
//        if (startingAddress < 0 || startingAddress + values.Length > _registers.Count)
//            throw new ArgumentOutOfRangeException($"Target register range is invalid");

//        for (int i = 0; i < values.Length; i++)
//        {
//            _registers[startingAddress + i].Value = values[i];
//        }
//    }

//    private void OnRegisterValueChanged(object? sender, RegisterValueChangedEventArgs e)
//    {
//        RegisterChanged?.Invoke(this, new RegisterChangedEventArgs(
//            e.Address,
//            e.OldValue,
//            e.NewValue));
//    }

//}

///// <summary>
///// 寄存器改变事件参数
///// </summary>
//public class RegisterChangedEventArgs : EventArgs
//{
//    public int Address { get; }
//    public ushort OldValue { get; }
//    public ushort NewValue { get; }

//    public RegisterChangedEventArgs(int address, ushort oldValue, ushort newValue)
//    {
//        Address = address;
//        OldValue = oldValue;
//        NewValue = newValue;
//    }
//}

///// <summary>
///// Modbus寄存器，表示一个16位的保持寄存器
///// </summary>
//public partial class ModbusRegister : ObservableObject, IDisposable
//{
//    private readonly ModbusRegisterArea _owner;
//    private ushort _value;
//    private bool _disposed = false;

//    /// <summary>
//    /// 获取此寄存器的地址（在区域中的索引）
//    /// </summary>
//    public int Address { get; }

//    /// <summary>
//    /// 获取拥有此寄存器的寄存器区域
//    /// </summary>
//    public ModbusRegisterArea Owner => _owner;

//    /// <summary>
//    /// 获取或设置寄存器的值
//    /// </summary>
//    public ushort Value
//    {
//        get => _value;
//        set
//        {
//            if (_value != value)
//            {
//                ushort oldValue = _value;
//                _value = value;
//                OnPropertyChanged();
//                ValueChanged?.Invoke(this, new RegisterValueChangedEventArgs(
//                    Address, oldValue, value));
//            }
//        }
//    }

//    /// <summary>
//    /// 当寄存器值改变时触发的事件
//    /// </summary>
//    public event EventHandler<RegisterValueChangedEventArgs>? ValueChanged;

//    /// <summary>
//    /// 寄存器名称（可选描述）
//    /// </summary>
//    [ObservableProperty]
//    private string _name = string.Empty;

//    /// <summary>
//    /// 寄存器描述（详细说明）
//    /// </summary>
//    [ObservableProperty]
//    private string _description = string.Empty;

//    /// <summary>
//    /// 是否只读寄存器
//    /// </summary>
//    [ObservableProperty]
//    private bool _isReadOnly = false;

//    public ModbusRegister(ModbusRegisterArea owner, int address)
//    {
//        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
//        Address = address;
//        _name = $"Register_{address}";
//    }

//    /// <summary>
//    /// 设置寄存器值而不触发事件
//    /// </summary>
//    public void SetValueSilent(ushort value)
//    {
//        _value = value;
//        OnPropertyChanged(nameof(Value));
//    }

//    /// <summary>
//    /// 获取寄存器的位值
//    /// </summary>
//    /// <param name="bit">位位置（0-15）</param>
//    /// <returns>位的布尔值</returns>
//    public bool GetBit(int bit)
//    {
//        if (bit < 0 || bit > 15)
//            throw new ArgumentOutOfRangeException(nameof(bit), "Bit position must be between 0 and 15");

//        return (_value & (1 << bit)) != 0;
//    }

//    /// <summary>
//    /// 设置寄存器的位值
//    /// </summary>
//    /// <param name="bit">位位置（0-15）</param>
//    /// <param name="value">要设置的值</param>
//    public void SetBit(int bit, bool value)
//    {
//        if (IsReadOnly)
//            throw new InvalidOperationException("Register is read-only");

//        if (bit < 0 || bit > 15)
//            throw new ArgumentOutOfRangeException(nameof(bit), "Bit position must be between 0 and 15");

//        if (value)
//        {
//            Value = (ushort)(_value | (1 << bit));
//        }
//        else
//        {
//            Value = (ushort)(_value & ~(1 << bit));
//        }
//    }

//    protected virtual void Dispose(bool disposing)
//    {
//        if (!_disposed)
//        {
//            if (disposing)
//            {
//                // 清理托管资源
//                ValueChanged = null;
//            }
//            _disposed = true;
//        }
//    }

//    public void Dispose()
//    {
//        Dispose(true);
//        GC.SuppressFinalize(this);
//    }

//    ~ModbusRegister()
//    {
//        Dispose(false);
//    }
//}

///// <summary>
///// 寄存器值改变事件参数
///// </summary>
//public class RegisterValueChangedEventArgs : EventArgs
//{
//    public int Address { get; }
//    public ushort OldValue { get; }
//    public ushort NewValue { get; }

//    public RegisterValueChangedEventArgs(int address, ushort oldValue, ushort newValue)
//    {
//        Address = address;
//        OldValue = oldValue;
//        NewValue = newValue;
//    }
//}