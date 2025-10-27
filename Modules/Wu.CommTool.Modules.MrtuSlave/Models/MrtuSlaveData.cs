//using Wu.CommTool.Core.Common;

//namespace Wu.CommTool.Modules.MrtuSlave.Models;

///// <summary>
///// ModbusRtu采集的数据
///// </summary>
//public partial class MrtuSlaveData : ObservableObject
//{
//    /// <summary>
//    /// 通讯状态 1通讯正常 0通讯失败
//    /// </summary>
//    public bool State => UpdateTime is not null && ((DateTime)UpdateTime).AddSeconds(10) > DateTime.Now;

//    /// <summary>
//    /// 名称
//    /// </summary>
//    [ObservableProperty] private string name = string.Empty;

//    /// <summary>
//    /// 寄存器类型
//    /// </summary>
//    [ObservableProperty] RegisterType registerType = RegisterType.Holding;

//    /// <summary>
//    /// 寄存器地址 起始地址 单位:word
//    /// </summary>
//    [ObservableProperty] private ushort registerAddr;

//    ///// 寄存器地址16进制
//    ///// </summary>
//    //[ObservableProperty]
//    //private ushort registerAddrHex;

//    /// <summary>
//    /// 寄存器地址 最后一个地址 单位:word
//    /// </summary>
//    public double RegisterLastWordAddr => RegisterAddr + MrtuDataTypeByteLength / 2 - 1;

//    /// <summary>
//    /// 数据类型需读取的长度 单位:字节
//    /// </summary>
//    public int MrtuDataTypeByteLength => ModbusUtils.GetMrtuDataTypeLengthForRead(MrtuDataType);

//    /// <summary>
//    /// 转换后的显示值
//    /// </summary>
//    [ObservableProperty]
//    double value;

//    /// <summary>
//    /// 数据类型
//    /// </summary>
//    [ObservableProperty]
//    [NotifyPropertyChangedFor(nameof(Value))]
//    [NotifyPropertyChangedFor(nameof(MrtuDataTypeByteLength))]
//    MrtuDataType mrtuDataType = MrtuDataType.Int;

//    /// <summary>
//    /// 数据更新时间
//    /// </summary>
//    [ObservableProperty]
//    [NotifyPropertyChangedFor (nameof(State))]
//    [property: JsonIgnore]
//    private DateTime? updateTime = null;

//    /// <summary>
//    /// 倍率
//    /// </summary>
//    [ObservableProperty]
//    private double rate = 1;

//    /// <summary>
//    /// 单位
//    /// </summary>
//    [ObservableProperty]
//    private string unit = string.Empty;

//    /// <summary>
//    /// 待写入的值
//    /// </summary>
//    [ObservableProperty]
//    private string writeValue;

//}
